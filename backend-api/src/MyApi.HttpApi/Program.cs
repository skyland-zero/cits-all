using Cits;
using Cits.IdGenerator;
using Cits.Jwt;
using Cits.LoginLogs;
using Cits.Permissions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MyApi.Domain.DomainServices.CorpWx;
using MyApi.Application.Identities;
using MyApi.HttpApi.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("正在启动！");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//基础服务
builder.Services.AddSerilogService(builder.Configuration);
builder.Services.AddIdGenerator();
builder.Services.ConfigureHybridCache(builder.Configuration);
builder.Services.AddScoped<GlobalExceptionFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())); //路由转换为小写加下划线格式
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
builder.Services.AddPermission();
//其他服务
builder.Services.ConfigureFreeSql(builder.Configuration);
builder.Services.ConfigureDataSeed();
builder.Services.AddLoginLog();
builder.Services.ConfigureUpload(builder.Configuration);

builder.Services.AddFreeRedis(builder.Configuration);


builder.Services.AddScalar();
// builder.Services.AddCorpWxService(builder.Configuration); //企业微信相关注册（不需要了）


//业务服务
builder.Services.ConfigureScrutor();
builder.Services.AddHostedService<UserPermissionPreWarmBackgroundService>();

//配置管道
var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapScalar();
app.MapControllers();
app.UseUploadHub();

app.Run();