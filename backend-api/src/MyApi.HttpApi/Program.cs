using Cits;
using Cits.IdGenerator;
using Cits.Jwt;
using Cits.LoginLogs;
using Cits.OperationLogs;
using Cits.Permissions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MyApi.HttpApi.Json;
using MyApi.Domain.DomainServices.CorpWx;
using MyApi.Domain.DomainServices.WorkOrders;
using MyApi.Application.Identities;
using MyApi.Application.Exports;
using MyApi.Application.Imports;
using MyApi.HttpApi.Extensions;
using MyApi.HttpApi.Hubs;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("正在启动！");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//基础服务
builder.Services.AddMemoryCache();
builder.Services.AddSerilogService(builder.Configuration);
builder.Services.AddIdGenerator();
builder.Services.ConfigureHybridCache(builder.Configuration);
builder.Services.AddScoped<GlobalExceptionFilter>();
builder.Services.AddScoped<OperationLogFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
    options.Filters.Add<OperationLogFilter>();
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())); //路由转换为小写加下划线格式
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new FlexibleDateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new FlexibleNullableDateTimeConverter());
});
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});
builder.Services.ConfigureApiBehavior();
//认证服务
builder.Services.AddJwtService();
builder.Services.ConfigureAuthentication(builder.Configuration);
//授权
builder.Services.AddPermission(builder.Configuration);
//其他服务
builder.Services.ConfigureFreeSql(builder.Configuration);
builder.Services.ConfigureDataSeed();
builder.Services.AddLoginLog();
builder.Services.AddOperationLog(builder.Configuration);
builder.Services.ConfigureUpload(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddSingleton<IExportTaskNotifier, ExportTaskSignalRNotifier>();

builder.Services.AddFreeRedis(builder.Configuration);
builder.Services.AddHangfireService(builder.Configuration);
builder.Services.AddSingleton<PdfAntiCounterfeitService>();
builder.Services.AddSingleton<WorkOrderPdfGenerator>();


builder.Services.AddScalar();
builder.Services.AddCorpWxService(builder.Configuration);


//业务服务
builder.Services.ConfigureScrutor();
builder.Services.AddHostedService<UserPermissionPreWarmBackgroundService>();
builder.Services.AddHostedService<ExportTaskBackgroundService>();
builder.Services.AddHostedService<ImportTaskBackgroundService>();

//配置管道
var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapScalar();
app.MapControllers();
app.MapHub<ExportTaskHub>("/hub/export-tasks");
app.UseUploadHub();

app.Run();
