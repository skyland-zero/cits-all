using System.Reflection;
using Cits.Authorization;
using Cits.DI;
using Cits.Permissions;

namespace MyApi.HttpApi.Extensions;

public static class ScrutorExtension
{
    public static IServiceCollection ConfigureScrutor(this IServiceCollection services)
    {
        services.Scan(scan =>
            {
                scan
                    .FromAssemblies(
                        Assembly.Load("MyApi.Application"),
                        Assembly.Load("MyApi.Application.Contracts"),
                        Assembly.Load("MyApi.Domain"),
                        Assembly.Load("MyApi.Domain.Shared")) // 指定程序集
                    .AddClasses(classes => classes
                            .AssignableTo(typeof(IPermissionProvider)) // 筛选实现接口的类
                    )
                    .AsImplementedInterfaces() // 作为实现的接口注册
                    .WithScopedLifetime() // 生命周期
                    .AddClasses(classes => classes
                            .AssignableTo(typeof(ITransientService)) // 筛选实现接口的类
                    )
                    .AsImplementedInterfaces() // 作为实现的接口注册
                    .WithTransientLifetime() // 生命周期
                    .AddClasses(classes => classes
                            .AssignableTo(typeof(IScopedService)) // 筛选实现接口的类
                    )
                    .AsImplementedInterfaces() // 作为实现的接口注册
                    .WithScopedLifetime()
                    .AddClasses(classes => classes
                            .AssignableTo(typeof(IApplicationService)) // 筛选实现接口的类
                    )
                    .AsImplementedInterfaces() // 作为实现的接口注册
                    .WithScopedLifetime()

                    //具体类注册Scoped
                    .AddClasses(classes => classes.AssignableTo(typeof(ISelfScopedService)))
                    .AsSelf()
                    .WithScopedLifetime()
                    //具体类注册Transient
                    .AddClasses(classes => classes.AssignableTo(typeof(ISelfTransientService)))
                    .AsSelf()
                    .WithTransientLifetime()
                    //具体类注册Singleton
                    .AddClasses(classes => classes.AssignableTo(typeof(ISelfSingletonService)))
                    .AsSelf()
                    .WithSingletonLifetime();
            }
            // 生命周期
        );

        //权限点提供服务
        services.AddSingleton<ICitsUserPermissionProvider, UserPermissionProvider>();

        return services;
    }
}