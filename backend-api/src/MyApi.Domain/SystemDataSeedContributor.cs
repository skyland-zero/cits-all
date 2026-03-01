using Cits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyApi.Domain.Identities;

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
    }
}