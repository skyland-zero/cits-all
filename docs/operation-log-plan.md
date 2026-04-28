## Context
需要为 `backend-api` 增加统一的操作日志能力，用于审计用户在系统中的查询和变更操作。日志需覆盖：系统模块、操作类型、操作人员、操作地址、操作地点、操作状态、操作时间、消耗时间、所属部门、请求地址和方法、请求参数、返回参数。用户已明确：**GET 也要记录，但只记录业务 GET**，排除监控、下载/预览、Hub 协商以及类似匿名高频接口。当前项目已具备登录日志异步写入、IP 归属地解析、全局 Filter、用户部门缓存等能力，应优先复用，避免在各业务接口分散埋点。

## Recommended approach
### 1. 复用现有登录日志链路，新增 OperationLog 异步落库能力
沿用现有登录日志的“请求线程入队、后台线程批量落库”模式，新增 `OperationLog` 实体、`IOperationLogWriter`、`OperationLogWriter`、`OperationLogService` 和服务注册扩展。

可直接参考与复用：
- `backend-api/framework/Cits.Domain/LoginLogs/LoginLog.cs`
- `backend-api/framework/Cits.Identity/LoginLogs/LoginLogWriter.cs`
- `backend-api/framework/Cits.Identity/LoginLogs/LoginLogService.cs`
- `backend-api/framework/Cits.Identity/LoginLogs/LoginLogServiceCollectionExtension.cs`
- `backend-api/framework/Cits.Identity/LoginLogs/IpLocationService.cs`

建议 `OperationLog` 字段至少包含：
- `Module`
- `OperationType`
- `OperatorId`
- `OperatorName`
- `OperationIp`
- `OperationLocation`
- `Status`
- `OperationTime`
- `ElapsedMilliseconds`
- `DepartmentName`
- `RequestPath`
- `RequestMethod`
- `RequestParameters`
- `ResponseParameters`
- `ErrorMessage`（便于失败排查）

### 2. 采用全局 `IAsyncActionFilter`，不采用 Middleware 作为主方案
主采集入口推荐放在 MVC 全局 `IAsyncActionFilter`，原因：
- 能直接访问 `ActionArguments`，更容易准确拿到 GET query、route 参数和业务 DTO
- 能读取 Action/Controller 上的自定义特性，显式拿到“系统模块”“操作类型”
- 能在 `await next()` 前后同时获得参数、结果、异常状态和耗时
- 当前项目已经采用 Filter 扩展点，风格一致

相关现有位置：
- 全局注册：`backend-api/src/MyApi.HttpApi/Program.cs`
- 现有 Filter 参考：`backend-api/src/MyApi.HttpApi/Extensions/OnlineUserFilter.cs`
- 全局异常处理：`backend-api/framework/Cits.WebApi/GlobalExceptionFilter.cs`

说明：`GlobalExceptionFilter` 已将异常转成 `JsonResult` 并标记 `ExceptionHandled = true`，因此新的操作日志 Filter 不能仅依赖“是否抛异常”，还应结合 `ActionExecutedContext.Exception`、`HttpContext.Response.StatusCode`、`Result` 类型综合判定成功失败。

### 3. 增加 `OperationLogAttribute` 做业务语义标注
新增特性，如：
- `[OperationLog("用户管理", "查询")]`
- `[OperationLog("角色管理", "新增")]`

规则建议：
- 优先在 Action 上标注
- Controller 可提供默认模块名
- 未标注时，Filter 可退回到 `ControllerName/ActionName` 推断，但只作为兜底，不作为主规则

这样可避免“靠路由猜模块”的不稳定方案，也便于后续筛选和报表统计。

### 4. 字段采集来源与实现口径
#### 4.1 操作人员
从 `HttpContext.User` / 当前用户服务读取，优先使用现有 Claims：
- `ClaimTypes.Sid`
- `ClaimTypes.Name`
- `ClaimTypes.Surname`

参考：
- `backend-api/src/MyApi.Application/Identities/AccountAppService.cs`

#### 4.2 所属部门
通过当前用户 `Id -> OrganizationUnitId -> IdentityOrganizationUnit.Name / NamePath` 获取，优先复用缓存能力：
- `backend-api/src/MyApi.Domain/Identities/UserPermissionManager.cs` 中的 `GetUserOrganizationUnitAsync`
- 组织实体：`backend-api/src/MyApi.Domain/Identities/IdentityOrganizationUnit.cs`
- 用户实体：`backend-api/src/MyApi.Domain/Identities/IdentityUser.cs`

建议记录 `DepartmentName`；若你们后续有上下级部门审计需求，可额外考虑记录 `NamePath`。

#### 4.3 操作地址 / 操作地点
- IP：`HttpContext.Connection.RemoteIpAddress`
- 地点：复用 `IIpLocationService`

登录日志已有现成模式：
- `backend-api/src/MyApi.Application/Identities/AccountAppService.cs`
- `backend-api/framework/Cits.Identity/LoginLogs/IpLocationService.cs`

#### 4.4 请求地址和方法
直接取：
- `HttpContext.Request.Path`
- `HttpContext.Request.Method`

#### 4.5 请求参数
优先组合以下来源并序列化：
- `ActionExecutingContext.ActionArguments`
- `HttpContext.Request.Query`
- `RouteData.Values`

说明：当前需求并不要求保留原始 body 文本；使用 `ActionArguments` 更符合现有 MVC 架构，也避免读 request body 带来的额外复杂度。

#### 4.6 返回参数
在 `await next()` 后，从 `ActionExecutedContext.Result` 读取：
- `ObjectResult`
- `JsonResult`
- `ContentResult`（仅摘要）

对 `FileResult`、流式响应、非 JSON 大对象不记录完整响应体，只记录摘要，例如：
- `{"type":"file","status":200}`
- `{"type":"content","length":123}`

#### 4.7 耗时
在 Filter 中使用 `Stopwatch` 统计毫秒数。

### 5. GET 记录策略
用户已确认：**只记录业务 GET**。因此默认策略应为：
- 记录普通业务查询 GET
- 排除监控、下载/预览、Hub、匿名高频接口
- 对 GET 的请求参数正常记录
- 对 GET 的返回参数只记录可序列化 JSON 结果，且做长度截断

建议首批排除/特殊处理如下：
1. 监控接口
   - `backend-api/src/MyApi.HttpApi/Controllers/MonitorController.cs`
   - `GET server-info`
   - 原因：匿名、高频轮询、返回对象较大

2. 文件预览/下载
   - `backend-api/src/MyApi.HttpApi/Controllers/FileUpload/UploadController.cs`
   - `GET access/preview`
   - `GET access/download`
   - 原因：文件流响应，且 token 需要脱敏

3. Upload Hub / hub 协商相关请求
   - `backend-api/src/MyApi.HttpApi/Hubs/UploadHub.cs`
   - `backend-api/src/MyApi.HttpApi/Extensions/FileUploadExtension.cs`
   - 原因：非典型业务查询，且可能高频

4. 其他匿名高频配置类 GET
   - `backend-api/src/MyApi.HttpApi/Controllers/FileUpload/UploadController.cs`
   - `GET settings`
   - 视业务需要决定是否排除；默认建议排除

说明：`UploadController` 下的 `GET check/{hash}` 是否记录，需要按业务价值决定；如果它是用户可见的业务步骤，可保留，但要对 `hash` 和返回对象做截断。

### 6. 成功失败判定规则
建议采用统一规则：
1. `ActionExecutedContext` 有未处理异常：失败
2. `HttpContext.Response.StatusCode >= 400`：失败
3. `Result` 为 `JsonResult` 且值为 `ApiErrorResponse`：失败
   - 参考：`backend-api/framework/Cits.WebApi/ApiResponse/ApiErrorResponse.cs`
4. 其他情况默认成功

这样可以兼容：
- 业务异常被 `GlobalExceptionFilter` 转为 400
- 系统异常被转为 500
- 正常返回 200/204 的查询、删除、保存接口

### 7. 脱敏与截断规则
必须内置统一脱敏器，避免敏感信息进库：

#### 7.1 脱敏字段
对以下键名做大小写不敏感脱敏：
- `password`
- `oldPassword`
- `newPassword`
- `confirmPassword`
- `token`
- `refreshToken`
- `authorization`
- `accessToken`
- 文件对象 / 流 / 二进制内容

统一替换为：`***`

#### 7.2 截断规则
- `RequestParameters`：限制最大长度，如 4KB
- `ResponseParameters`：限制最大长度，如 8KB
- 超限时保留前缀并追加截断标记，避免表无限膨胀

### 8. 推荐修改文件
核心修改文件预计包括：
- `backend-api/src/MyApi.HttpApi/Program.cs`
- `backend-api/src/MyApi.HttpApi/Extensions/`（新增 `OperationLogFilter`）
- `backend-api/framework/Cits.Domain/`（新增 `OperationLog` 实体）
- `backend-api/framework/Cits.Identity/LoginLogs/` 或相邻日志目录（新增 Writer、HostedService、服务注册）
- `backend-api/src/MyApi.HttpApi/Controllers/**/*.cs`（为核心业务接口补充 `OperationLogAttribute`）
- `backend-api/src/MyApi.Application/**`（如本次还需要做日志查询应用服务）
- `backend-api/src/MyApi.HttpApi/Controllers/**`（如本次还需要做查询控制器）

### 9. 建议实施顺序
1. 新增 `OperationLog` 实体和异步写入链路
2. 新增 `OperationLogAttribute`
3. 新增全局 `OperationLogFilter`
4. 在 `Program.cs` 注册服务和 Filter
5. 先给核心业务 Controller/Action 打标
6. 配置默认排除规则和脱敏/截断逻辑
7. 如需要，再补操作日志查询接口和前端页面

## Verification
1. 启动 `backend-api`，确认 `OperationLog` 表自动建表/映射正常。
2. 调用一个普通业务 GET，确认写入日志，且包含模块、类型、人员、部门、IP、地点、状态、耗时、请求参数、返回参数。
3. 调用 POST/PUT/DELETE 接口，确认同样写入日志。
4. 调用会抛 `UserFriendlyException` 的接口，确认状态记录为失败。
5. 调用会触发 500 的接口，确认状态记录为失败并带 `ErrorMessage`。
6. 验证 `MonitorController.server-info`、文件下载/预览、Hub 相关请求不会记录完整响应体，或被正确排除。
7. 验证密码、token、Authorization、文件对象等字段被脱敏。
8. 验证高频查询下接口响应时间无明显恶化，后台异步落库正常。
