using Cits;
using Cits.Entities;
using Cits.Extensions;
using Cits.IdGenerator;
using FreeSql.Aop;
// ReSharper disable once CheckNamespace
using FreeSql;

namespace Microsoft.Extensions.Hosting;

public static class FreeSqlExtension
{
    /// <summary>
    ///     配置、注入FreeSql服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureFreeSql(this IServiceCollection services, IConfiguration configuration)
    {
        var useLocal = configuration.GetValue<bool>("DbConfig:UseLocal");
        var connectionString = configuration.GetConnectionString(useLocal ? "Local" : "Remote");
        var dataType = useLocal ? DataType.Sqlite : DataType.PostgreSQL;

        Func<IServiceProvider, IFreeSql> fsqlFactory = r =>
        {
            var fsql = new FreeSqlBuilder()
                .UseConnectionString(dataType, connectionString)
                // .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}"))//监听SQL语句
                .UseAutoSyncStructure(true) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
                .Build();

            fsql.GlobalFilter.Apply<IDelete>("IsDeleted", a => a.IsDeleted == false); //软删除全局过滤器

            var provider = services.BuildServiceProvider();
            var user = provider.GetRequiredService<ICurrentUser>();
            var idGenerator = provider.GetRequiredService<IIdGenerator>();
            fsql.Aop.AuditValue += (s, e) =>
            {
                if ((e.AuditValueType is AuditValueType.Insert or AuditValueType.InsertOrUpdate)
                    && e.Property.Name == "Id"
                    && (e.Value == null || (Guid)e.Value == Guid.Empty || (Guid?)e.Value == null))
                {
                    e.Value = idGenerator.Create();
                }

                if (user.Id == Guid.Empty)
                {
                    return;
                }

                if (e.AuditValueType is AuditValueType.Insert or AuditValueType.InsertOrUpdate)
                {
                    switch (e.Property.Name)
                    {
                        case "Id":
                            if (e.Value == null || (Guid)e.Value == Guid.Empty || (Guid?)e.Value == null)
                            {
                                e.Value = user.Id;
                            }

                            break;
                        case "CreatorUserId":
                            if (e.Value == null || (Guid)e.Value == Guid.Empty || (Guid?)e.Value == null)
                            {
                                e.Value = user.Id;
                            }

                            break;
                        case "CreatorUserName":
                            if (e.Value == null || ((string)e.Value).IsNull())
                            {
                                e.Value = user.UserName;
                            }

                            break;
                        case "LastModifierUserId":
                            e.Value = user.Id;
                            break;
                        case "LastModifierUserName":
                            e.Value = user.UserName;
                            break;
                    }
                }

                if (e.AuditValueType is AuditValueType.Update or AuditValueType.InsertOrUpdate)
                {
                    switch (e.Property.Name)
                    {
                        case "LastModifierUserId":
                            e.Value = user.Id;
                            break;
                        case "LastModifierUserName":
                            e.Value = user.UserName;
                            break;
                    }
                }
            };
            return fsql;
        };
        services.AddSingleton(fsqlFactory);
        return services;
    }
}