using Cits;
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

        await SeedMenusAsync(freeSql, role.Id, cancellationToken);
    }

    private async Task SeedMenusAsync(IFreeSql freeSql, Guid adminRoleId, CancellationToken ct)
    {
        // 先创建页面数据 - 路径调整为 component 要求的格式
        var pages = new List<IdentityPage>
        {
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505201"), Name = "分析页", Path = "/dashboard/analytics/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505202"), Name = "工作台", Path = "/dashboard/workspace/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505203"), Name = "页面管理", Path = "/system/pages/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505204"), Name = "工单管理", Path = "/workorder/manages/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505205"), Name = "创建工单", Path = "/workorder/manages/create" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505206"), Name = "用户管理", Path = "/permission/users/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505207"), Name = "角色管理", Path = "/permission/roles/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505208"), Name = "部门管理", Path = "/permission/organizations/index" },
            new() { Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505209"), Name = "菜单管理", Path = "/permission/menus/index" },
        };

        foreach (var page in pages)
        {
            if (!await freeSql.Select<IdentityPage>().AnyAsync(x => x.Id == page.Id, ct))
            {
                await freeSql.Insert(page).ExecuteAffrowsAsync(ct);
            }
            else
            {
                await freeSql.Update<IdentityPage>(page.Id).Set(x => x.Path, page.Path).ExecuteAffrowsAsync(ct);
            }
        }

        var menus = new List<IdentityMenu>
        {
            // Group: 概览
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505101"),
                Name = "概览",
                Path = "/",
                Redirect = "/analytics",
                Icon = "mdi:home",
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
                Icon = "mdi:analytics",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 1,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505201")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505103"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505101"),
                Name = "工作台",
                Path = "/workspace",
                Icon = "mdi:work",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 2,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505202")
            },
            // Group: 工单模块
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505120"),
                Name = "工单模块",
                Path = "/workorder",
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
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 1,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505204")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505122"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505120"),
                Name = "创建工单",
                Path = "/workorder/manages/create",
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 1,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505205")
            },
            // Group: 权限设置
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505130"),
                Name = "权限设置",
                Path = "/permission",
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
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 1,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505208")
            },
            new()
            {
                Id = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505132"),
                ParentId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505130"),
                Name = "角色管理",
                Path = "/permission/role",
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
                Path = "/system",
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
                Type = IdentityMenuType.Menu,
                Level = 2,
                Order = 1,
                PageId = Guid.Parse("f6e804b2-0ea7-e13a-7787-b03c1b505203")
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
                    .Set(x => x.Path, menu.Path)
                    .Set(x => x.Redirect, menu.Redirect)
                    .Set(x => x.Icon, menu.Icon)
                    .Set(x => x.Order, menu.Order)
                    .Set(x => x.PageId, menu.PageId)
                    .Set(x => x.ParentId, menu.ParentId)
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
