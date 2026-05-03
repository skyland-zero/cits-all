using Cits;
using Cits.Domain.SystemSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyApi.Domain.Identities;
using MyApi.Domain.Shared.Identities;

namespace MyApi.Domain;

public class SystemDataSeedContributor : BackgroundService
{
    private readonly ILogger<SystemDataSeedContributor> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SystemDataSeedContributor(IServiceScopeFactory serviceScopeFactory,
        ILogger<SystemDataSeedContributor> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("初始化系统数据开始");
        await DoWorkAsync(stoppingToken);
        _logger.LogInformation("初始化系统数据完成");
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var freeSql = scope.ServiceProvider.GetRequiredService<IFreeSql>();

        var role = new IdentityRole()
        {
            Id = Guid.Parse("d76404b2-0ea7-e13a-7787-b03c1b50519a"),
            Name = "系统管理员",
            Code = "SysAdmin",
            IsDefault = false,
            IsStatic = true
        };

        if (!await freeSql.Select<IdentityRole>().AnyAsync(x => x.Id == role.Id, cancellationToken))
        {
            await freeSql.Insert(role).ExecuteAffrowsAsync(cancellationToken);
        }

        var org = new IdentityOrganizationUnit
        {
            Id = Guid.Parse("138d91a3-fb53-db0b-e4c9-6c49223cae40"),
            Name = "天空之城",
            Code = "SKYLAND",
            Sort = 1,
            ParentId = null,
            Level = 1
        };
        org.Path = org.Id + ",";
        org.NamePath = org.Name;

        var childOrg1 = new IdentityOrganizationUnit()
        {
            Id = Guid.Parse("71f329fb-2c1b-90b3-5724-3e6c73417800"),
            Name = "指挥中心",
            Sort = 1,
            ParentId = org.Id,
            Path = "",
            Level = 1
        };
        childOrg1.Path = org.Path + childOrg1.Id + ",";
        childOrg1.NamePath = org.NamePath + ">" + childOrg1.Name;

        if (!await freeSql.Select<IdentityOrganizationUnit>().AnyAsync(x => x.Id == org.Id, cancellationToken))
        {
            await freeSql.Insert(org).ExecuteAffrowsAsync(cancellationToken);
        }

        if (!await freeSql.Select<IdentityOrganizationUnit>().AnyAsync(x => x.Id == childOrg1.Id, cancellationToken))
        {
            await freeSql.Insert(childOrg1).ExecuteAffrowsAsync(cancellationToken);
        }

        var user = new IdentityUser()
        {
            Id = Guid.Parse("a47ec0b4-a7d0-8c19-3300-90f8b095ca76"),
            UserName = "admin",
            Surname = "系统管理员",
            OrganizationUnitId = Guid.Empty,
            IsActive = true,
            PasswordHash = PasswordHasher.HashPassword("123456")
        };

        if (!await freeSql.Select<IdentityUser>().AnyAsync(x => x.Id == user.Id, cancellationToken))
        {
            await freeSql.Insert(user).ExecuteAffrowsAsync(cancellationToken);
        }

        var userRole = new IdentityUserRole()
        {
            UserId = user.Id,
            RoleId = role.Id,
        };
        if (!await freeSql.Select<IdentityUserRole>()
                .AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id, cancellationToken))
        {
            await freeSql.Insert(userRole).ExecuteAffrowsAsync(cancellationToken);
        }

        await SeedSystemSettingsAsync(freeSql, cancellationToken);
        await SeedMenusAsync(freeSql, role.Id, cancellationToken);
    }

    private static async Task SeedSystemSettingsAsync(IFreeSql freeSql, CancellationToken ct)
    {
        var settings = new List<SystemSetting>
        {
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505301"),
                Key = "system.siteName",
                Name = "系统名称",
                Value = "Cits Admin",
                ValueType = SystemSettingValueTypes.String,
                Group = SystemSettingGroups.Basic,
                Description = "浏览器标题、登录页和基础布局中展示的系统名称",
                Sort = 1
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505302"),
                Key = "security.password.minLength",
                Name = "密码最小长度",
                Value = "8",
                ValueType = SystemSettingValueTypes.Number,
                Group = SystemSettingGroups.Security,
                Description = "用户密码允许的最小字符长度",
                Sort = 10
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505303"),
                Key = "security.password.requireUppercase",
                Name = "密码要求大写字母",
                Value = "false",
                ValueType = SystemSettingValueTypes.Boolean,
                Group = SystemSettingGroups.Security,
                Description = "开启后密码必须包含至少一个大写字母",
                Sort = 20
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505304"),
                Key = "security.password.requireLowercase",
                Name = "密码要求小写字母",
                Value = "false",
                ValueType = SystemSettingValueTypes.Boolean,
                Group = SystemSettingGroups.Security,
                Description = "开启后密码必须包含至少一个小写字母",
                Sort = 30
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505305"),
                Key = "security.password.requireDigit",
                Name = "密码要求数字",
                Value = "true",
                ValueType = SystemSettingValueTypes.Boolean,
                Group = SystemSettingGroups.Security,
                Description = "开启后密码必须包含至少一个数字",
                Sort = 40
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505306"),
                Key = "security.password.requireNonAlphanumeric",
                Name = "密码要求特殊字符",
                Value = "false",
                ValueType = SystemSettingValueTypes.Boolean,
                Group = SystemSettingGroups.Security,
                Description = "开启后密码必须包含至少一个特殊字符",
                Sort = 50
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505307"),
                Key = "security.login.maxFailedAttempts",
                Name = "登录失败锁定次数",
                Value = "5",
                ValueType = SystemSettingValueTypes.Number,
                Group = SystemSettingGroups.Security,
                Description = "连续登录失败达到该次数后锁定账号",
                Sort = 60
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505308"),
                Key = "security.login.lockoutMinutes",
                Name = "登录锁定分钟数",
                Value = "10",
                ValueType = SystemSettingValueTypes.Number,
                Group = SystemSettingGroups.Security,
                Description = "账号被锁定后的自动解锁时间",
                Sort = 70
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505309"),
                Key = "security.password.forceChangeInitial",
                Name = "强制修改初始密码",
                Value = "false",
                ValueType = SystemSettingValueTypes.Boolean,
                Group = SystemSettingGroups.Security,
                Description = "开启后初始密码用户登录后必须先修改密码",
                Sort = 80
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505310"),
                Key = "upload.maxFileSizeMb",
                Name = "上传文件大小限制(MB)",
                Value = "100",
                ValueType = SystemSettingValueTypes.Number,
                Group = SystemSettingGroups.Upload,
                Description = "通用上传文件大小限制，后续可接入上传校验服务",
                Sort = 10
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505311"),
                Key = "import.maxRows",
                Name = "单次导入最大行数",
                Value = "5000",
                ValueType = SystemSettingValueTypes.Number,
                Group = SystemSettingGroups.Import,
                Description = "xlsx 导入任务单个文件允许的最大数据行数",
                Sort = 10
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505312"),
                Key = "announcement.loginPopupEnabled",
                Name = "登录弹出公告",
                Value = "true",
                ValueType = SystemSettingValueTypes.Boolean,
                Group = SystemSettingGroups.Announcement,
                Description = "开启后用户登录时展示未读公告弹窗",
                Sort = 10
            }
        };

        foreach (var setting in settings)
        {
            if (!await freeSql.Select<SystemSetting>().AnyAsync(x => x.Id == setting.Id, ct))
            {
                await freeSql.Insert(setting).ExecuteAffrowsAsync(ct);
                continue;
            }

            await freeSql.Update<SystemSetting>(setting.Id)
                .Set(x => x.Key, setting.Key)
                .Set(x => x.Name, setting.Name)
                .Set(x => x.ValueType, setting.ValueType)
                .Set(x => x.Group, setting.Group)
                .Set(x => x.Description, setting.Description)
                .Set(x => x.Sort, setting.Sort)
                .ExecuteAffrowsAsync(ct);
        }
    }

    private async Task SeedMenusAsync(IFreeSql freeSql, Guid adminRoleId, CancellationToken ct)
    {
        // 先创建页面数据 - 路径调整为 component 要求的格式 (去掉 views/ 和 .vue)
        var pages = new List<IdentityPage>
        {
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505201"), Name = "分析页", RouteName = "Analytics", Path = "dashboard/analytics/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505202"), Name = "工作台", RouteName = "Workspace", Path = "dashboard/workspace/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505203"), Name = "页面管理", RouteName = "Pages", Path = "system/pages/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505204"), Name = "工单管理", RouteName = "WorkOrderManages", Path = "workorder/manages/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505205"), Name = "新增工单", RouteName = "WorkOrderCreate", Path = "workorder/manages/create" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505206"), Name = "用户管理", RouteName = "Users", Path = "permission/users/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505207"), Name = "角色管理", RouteName = "Roles", Path = "permission/roles/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505208"), Name = "部门管理", RouteName = "Organizations", Path = "permission/organizations/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505209"), Name = "菜单管理", RouteName = "Menus", Path = "permission/menus/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505210"), Name = "服务器监控", RouteName = "ServerMonitor", Path = "monitor/server/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505211"), Name = "登录日志", RouteName = "LoginLog", Path = "monitor/login-log/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505212"), Name = "操作日志", RouteName = "OperationLog", Path = "monitor/operation-log/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505213"), Name = "数据字典", RouteName = "Dict", Path = "system/dict/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505214"), Name = "字典数据", RouteName = "DictItems", Path = "system/dict/item" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505215"), Name = "任务中心", RouteName = "Jobs", Path = "system/jobs/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505216"), Name = "系统参数", RouteName = "SystemSettings", Path = "system/settings/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505217"), Name = "在线用户", RouteName = "OnlineUsers", Path = "monitor/online-users/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505218"), Name = "文件管理", RouteName = "FileManagement", Path = "system/files/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505219"), Name = "数据导入", RouteName = "ImportTasks", Path = "system/imports/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505220"), Name = "系统公告", RouteName = "SystemAnnouncements", Path = "system/announcements/index" },
        };

        foreach (var page in pages)
        {
            if (!await freeSql.Select<IdentityPage>().AnyAsync(x => x.Id == page.Id, ct))
            {
                await freeSql.Insert(page).ExecuteAffrowsAsync(ct);
            }
            else
            {
                await freeSql.Update<IdentityPage>(page.Id)
                    .Set(x => x.Path, page.Path)
                    .Set(x => x.RouteName, page.RouteName)
                    .ExecuteAffrowsAsync(ct);
            }
        }

        var menus = new List<IdentityMenu>
        {
            // Group: Dashboard
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505101"),
                Name = "概览",
                RouteName = "Dashboard",
                Path = "/dashboard",
                Redirect = "/analytics",
                Icon = "lucide:layout-dashboard",
                Type = IdentityMenuType.Menu,
                Order = 1,
                Level = 1
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505102"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505101"),
                Name = "分析页",
                Path = "/analytics",
                Icon = "lucide:area-chart",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 1,
                AffixTab = true,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505201")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505103"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505101"),
                Name = "工作台",
                Path = "/workspace",
                Icon = "carbon:workspace",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 2,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505202")
            },
            // Group: 工单管理
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505120"),
                Name = "工单管理",
                RouteName = "WorkOrder",
                Path = "/workorder",
                Icon = "lucide:layout-dashboard",
                Type = IdentityMenuType.Menu,
                Order = 2,
                Level = 1
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505121"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505120"),
                Name = "工单管理",
                Path = "/workorder/manages/index",
                Icon = "lucide:area-chart",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 1,
                KeepAlive = true,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505204")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505122"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505120"),
                Name = "新增工单",
                Path = "/workorder/manages/create",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 2,
                HideInMenu = true,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505205")
            },
            // Group: 权限设置
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505130"),
                Name = "权限设置",
                RouteName = "Permission",
                Path = "/permission",
                Icon = "lucide:layout-dashboard",
                Type = IdentityMenuType.Menu,
                Order = 90,
                Level = 1
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505131"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505130"),
                Name = "用户管理",
                Path = "/permission/user",
                Icon = "lucide:area-chart",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 1,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505206")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505133"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505130"),
                Name = "部门管理",
                Path = "/permission/organization",
                Icon = "lucide:area-chart",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 2,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505208")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505132"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505130"),
                Name = "角色管理",
                Path = "/permission/role",
                Icon = "lucide:area-chart",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 3,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505207")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505134"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505130"),
                Name = "菜单管理",
                Path = "/permission/menu",
                Icon = "lucide:area-chart",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 4,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505209")
            },
            // Group: 系统设置
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505110"),
                Name = "系统设置",
                RouteName = "System",
                Path = "/system",
                Icon = "lucide:layout-dashboard",
                Type = IdentityMenuType.Menu,
                Order = 91,
                Level = 1
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505111"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505110"),
                Name = "页面管理",
                Path = "/system/page",
                Icon = "lucide:area-chart",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 1,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505203")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505112"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505110"),
                Name = "数据字典",
                Path = "/system/dict",
                Icon = "lucide:book-open",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 2,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505213")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505113"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505110"),
                Name = "字典数据",
                Path = "/system/dict/items/:id",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 3,
                HideInMenu = true,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505214")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505114"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505110"),
                Name = "任务中心",
                Path = "/system/jobs",
                Icon = "lucide:calendar-clock",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 4,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505215")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505115"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505110"),
                Name = "系统参数",
                Path = "/system/settings",
                Icon = "lucide:settings",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 5,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505216")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505116"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505110"),
                Name = "文件管理",
                Path = "/system/files",
                Icon = "lucide:folder-open",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 6,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505218")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505117"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505110"),
                Name = "数据导入",
                Path = "/system/imports",
                Icon = "lucide:file-up",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 7,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505219")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505118"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505110"),
                Name = "系统公告",
                Path = "/system/announcements",
                Icon = "lucide:megaphone",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 8,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505220")
            },
            // Group: 系统监控
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505140"),
                Name = "系统监控",
                RouteName = "Monitor",
                Path = "/monitor",
                Icon = "lucide:monitor",
                Type = IdentityMenuType.Menu,
                Order = 80,
                Level = 1
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505141"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505140"),
                Name = "服务器监控",
                Path = "/monitor/server",
                Icon = "lucide:cpu",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 1,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505210")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505142"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505140"),
                Name = "登录日志",
                Path = "/monitor/login-log",
                Icon = "lucide:file-text",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 2,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505211")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505143"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505140"),
                Name = "操作日志",
                Path = "/monitor/operation-log",
                Icon = "lucide:clipboard-list",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 3,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505212")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505144"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505140"),
                Name = "在线用户",
                Path = "/monitor/online-users",
                Icon = "lucide:users",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 4,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505217")
            },

            // 权限点示例 (以角色管理为例)
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505301"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505132"), // 角色管理
                Name = "查看",
                Path = "MyApi.Roles",
                Type = IdentityMenuType.AuthPoint,
                Level = 3,
                Order = 1
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505302"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505132"), // 角色管理
                Name = "创建",
                Path = "MyApi.Roles.Create",
                Type = IdentityMenuType.AuthPoint,
                Level = 3,
                Order = 2
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505303"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505132"), // 角色管理
                Name = "更新",
                Path = "MyApi.Roles.Update",
                Type = IdentityMenuType.AuthPoint,
                Level = 3,
                Order = 3
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505304"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505132"), // 角色管理
                Name = "删除",
                Path = "MyApi.Roles.Delete",
                Type = IdentityMenuType.AuthPoint,
                Level = 3,
                Order = 4
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505311"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505118"),
                Name = "查看",
                Path = "MyApi.Announcements",
                Type = IdentityMenuType.AuthPoint,
                Level = 3,
                Order = 1
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505312"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505118"),
                Name = "创建",
                Path = "MyApi.Announcements.Create",
                Type = IdentityMenuType.AuthPoint,
                Level = 3,
                Order = 2
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505313"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505118"),
                Name = "更新",
                Path = "MyApi.Announcements.Update",
                Type = IdentityMenuType.AuthPoint,
                Level = 3,
                Order = 3
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505314"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505118"),
                Name = "删除",
                Path = "MyApi.Announcements.Delete",
                Type = IdentityMenuType.AuthPoint,
                Level = 3,
                Order = 4
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505315"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505118"),
                Name = "发布/下线",
                Path = "MyApi.Announcements.Publish",
                Type = IdentityMenuType.AuthPoint,
                Level = 3,
                Order = 5
            },
        };

        foreach (var menu in menus)
        {
            if (!await freeSql.Select<IdentityMenu>().AnyAsync(x => x.Id == menu.Id, ct))
            {
                await freeSql.Insert(menu).ExecuteAffrowsAsync(ct);
            }
            else
            {
                await freeSql.Update<IdentityMenu>(menu.Id)
                    .Set(x => x.Name, menu.Name)
                    .Set(x => x.RouteName, menu.RouteName)
                    .Set(x => x.Path, menu.Path)
                    .Set(x => x.Redirect, menu.Redirect)
                    .Set(x => x.Icon, menu.Icon)
                    .Set(x => x.Order, menu.Order)
                    .Set(x => x.PageId, menu.PageId)
                    .Set(x => x.ParentId, menu.ParentId)
                    .Set(x => x.AffixTab, menu.AffixTab)
                    .Set(x => x.KeepAlive, menu.KeepAlive)
                    .Set(x => x.HideInMenu, menu.HideInMenu)
                    .ExecuteAffrowsAsync(ct);
            }

            // 分配权限
            if (!await freeSql.Select<IdentityRoleMenu>()
                    .AnyAsync(x => x.RoleId == adminRoleId && x.MenuId == menu.Id, ct))
            {
                await freeSql.Insert(new IdentityRoleMenu { RoleId = adminRoleId, MenuId = menu.Id })
                    .ExecuteAffrowsAsync(ct);
            }
        }
    }
}
