using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Implementations;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.UnitTests;

public class RoleServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepository;
    private readonly Mock<IPermissionRepository> _permissionRepository;
    private readonly IRoleService _roleService;
    
    public RoleServiceTests()
    {
        _roleRepository = new Mock<IRoleRepository>();
        _permissionRepository = new Mock<IPermissionRepository>();
        _roleService = new RoleService(_roleRepository.Object, _permissionRepository.Object,
            NullLogger<RoleService>.Instance);
    }

    [Fact]
    public async Task GetRoles_WhenRolesExist_ReturnsListOfRoles()
    {
        List<Role> roles =
        [
            new Role
            {
                Id = 1,
                Name = "role1",
            },
            new Role
            {
                Id = 2,
                Name = "role2",
            },
            new Role
            {
                Id = 3,
                Name = "role3",
            }
        ];

        _roleRepository.Setup(x => x.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        
        var result = await _roleService.GetRoles(CancellationToken.None);
        
        Assert.Equal(3, result.Count);
        Assert.Equal(roles, result);
        
        _roleRepository.Verify(x => x.GetRolesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRoles_WhenNoRolesExist_ReturnsEmptyList()
    {
        List<Role> roles = [];
        
        _roleRepository.Setup(x => x.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        
        var result = await _roleService.GetRoles(CancellationToken.None);
        
        Assert.Empty(result);
        
        _roleRepository.Verify(x => x.GetRolesAsync(It.IsAny<CancellationToken>()), Times.Once);   
    }

    [Fact]
    public async Task GetRoleById_WhenRoleExists_ReturnsRole()
    {
        int roleId = 1;

        Role role = new Role
        {
            Id = roleId,
            Name = "role"
        };

        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        
        var result = await _roleService.GetRoleById(roleId, CancellationToken.None);
        
        Assert.Equal(roleId, result.Id);
        Assert.Equal("role", result.Name);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);  
    }

    [Fact]
    public async Task GetRoleById_WhenRoleDoesNotExist_ThrowsNotFoundException()
    {
        int roleId = 1;
        
        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        Func<Task> result = () => _roleService.GetRoleById(roleId, CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddRole_WhenRoleDoesNotExist_ReturnsAddedRole()
    {
        int roleId = 1;
        string roleName = "ROLE";

        _roleRepository.Setup(x => x.IsRoleExistAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _roleRepository.Setup(x =>
                x.AddRoleAsync(It.Is<Role>(role => role.Name == roleName), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role addedRole, CancellationToken _) =>
            {
                addedRole.Id = roleId;
                return addedRole;
            });
        
        var result = await _roleService.AddRole(roleName, CancellationToken.None);
        
        Assert.Equal(roleId, result.Id);
        Assert.Equal(roleName, result.Name);
        
        _roleRepository.Verify(x => x.IsRoleExistAsync(roleName, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.AddRoleAsync(It.Is<Role>(role => role.Name == roleName), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddRole_WhenRoleAlreadyExists_ThrowsConflictException()
    {
        string roleName = "role";

        _roleRepository.Setup(x => x.IsRoleExistAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        Func<Task> result = () => _roleService.AddRole(roleName, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);
        
        _roleRepository.Verify(x => x.IsRoleExistAsync(roleName, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.AddRoleAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRoleById_WhenRoleExists_UpdatesRole()
    {
        int roleId = 1;
        string roleCurrentName = "role";
        string roleNewName = "new role";

        Role role = new Role
        {
            Id = roleId,
            Name = roleCurrentName
        };

        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _roleRepository.Setup(x => x.UpdateRoleAsync(role, roleNewName, It.IsAny<CancellationToken>()))
            .Callback<Role, string, CancellationToken>((r, n, _) =>
            {
                r.Name = n;
            })
            .Returns(Task.CompletedTask);
        
        await _roleService.UpdateRoleById(roleId, roleNewName, CancellationToken.None);
        
        Assert.Equal(roleNewName, role.Name);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.UpdateRoleAsync(role, roleNewName, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateRoleById_WhenRoleDoesNotExist_ThrowsNotFoundException()
    {
        int roleId = 1;
        string roleNewName = "new role";
        
        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);
        
        Func<Task> result = () => _roleService.UpdateRoleById(roleId, roleNewName, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.UpdateRoleAsync(It.IsAny<Role>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateRoleById_WhenRoleNameAlreadyExists_ThrowsConflictException()
    {
        int roleId = 1;
        string roleCurrentName = "role";
        string roleNewName = "role";

        Role role = new Role
        {
            Id = roleId,
            Name = roleCurrentName
        };

        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        Func<Task> result = () => _roleService.UpdateRoleById(roleId, roleNewName, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.UpdateRoleAsync(It.IsAny<Role>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);   
    }

    [Fact]
    public async Task DeleteRoleById_WhenRoleExists_DeletesRole()
    {
        int roleId = 1;
        string roleName = "role";

        Role role = new Role
        {
            Id = roleId,
            Name = roleName
        };

        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        
        await _roleService.DeleteRoleById(roleId, CancellationToken.None);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.DeleteRoleAsync(role, It.IsAny<CancellationToken>()), Times.Once);  
    }

    [Fact]
    public async Task DeleteRoleById_WhenRoleDoesNotExist_ThrowsNotFoundException()
    {
        int roleId = 1;
        
        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);
        
        Func<Task> result = () => _roleService.DeleteRoleById(roleId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.DeleteRoleAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never); 
    }

    [Fact] 
    public async Task DeleteRoleById_WhenRoleIsAdmin_ThrowsConflictException()
    {
        int roleId = 1;
        string roleName = "admin";

        Role role = new Role
        {
            Id = roleId,
            Name = roleName
        };
        
        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        
        Func<Task> result = () => _roleService.DeleteRoleById(roleId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.DeleteRoleAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task AddPermissionToRole_WhenRoleAndPermissionExist_AddsPermissionToRoleAndReturnsRoleWithPermission()
    {
        int roleId = 1;
        int permissionId = 1;
        string roleName = "role";
        string permissionName = "permission";

        Role role = new Role
        {
            Id = roleId,
            Name = roleName,
            RolePermissions = new List<RolePermission>()
        };

        Permission permission = new Permission
        {
            Id = permissionId,
            Name = permissionName
        };

        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _permissionRepository.Setup(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);
        _roleRepository.Setup(x =>
                x.AddPermissionToRoleAsync(
                    It.Is<RolePermission>(rp => rp.RoleId == roleId && rp.PermissionId == permissionId),
                    It.IsAny<CancellationToken>()))
            .Callback<RolePermission, CancellationToken>((rp, _) =>
            {
                role.RolePermissions.Add(rp);
            })
            .ReturnsAsync(role);

        var result = await _roleService.AddPermissionToRole(roleId, permissionId, CancellationToken.None);
        
        Assert.Contains(result.RolePermissions, rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
        Assert.Same(role, result);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _permissionRepository.Verify(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x =>
                x.AddPermissionToRoleAsync(
                    It.Is<RolePermission>(rp => rp.RoleId == roleId && rp.PermissionId == permissionId),
                    It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddPermissionToRole_WhenRoleDoesNotExist_ThrowsNotFoundException()
    {
        int roleId = 1;
        int permissionId = 1;
        
        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);
        
        Func<Task> result = () => _roleService.AddPermissionToRole(roleId, permissionId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _permissionRepository.Verify(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()), Times.Never);
        _roleRepository.Verify(x =>
                x.AddPermissionToRoleAsync(
                    It.IsAny<RolePermission>(),
                    It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task AddPermissionToRole_WhenPermissionDoesNotExist_ThrowsNotFoundException()
    {
        int roleId = 1;
        int permissionId = 1;

        Role role = new Role
        {
            Id = roleId,
            Name = "role",
        };

        Permission permission = new Permission
        {
            Id = permissionId,
            Name = "permission"
        };

        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _permissionRepository.Setup(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Permission?)null);

        Func<Task> result = () => _roleService.AddPermissionToRole(roleId, permissionId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _permissionRepository.Verify(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x =>
                x.AddPermissionToRoleAsync(
                    It.IsAny<RolePermission>(),
                    It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemovePermissionFromRole_WhenRoleAndPermissionAndRolePermissionExist_RemovesPermissionFromRole()
    {
        int roleId = 1;
        int permissionId = 1;
        string roleName = "role";
        string permissionName = "permission";

        Role role = new Role
        {
            Id = roleId,
            Name = roleName,
            RolePermissions = new List<RolePermission>()
        };

        Permission permission = new Permission
        {
            Id = permissionId,
            Name = permissionName
        };

        RolePermission rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            Role = role,
            Permission = permission
        };
        
        role.RolePermissions.Add(rolePermission);

        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _permissionRepository.Setup(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);
        _roleRepository.Setup(x => x.GetRolePermissionAsync(roleId, permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rolePermission);
        _roleRepository.Setup(x => x.RemovePermissionFromRoleAsync(rolePermission, It.IsAny<CancellationToken>()))
            .Callback<RolePermission, CancellationToken>((rp, _) =>
            {
                role.RolePermissions.Remove(rp);
            })
            .Returns(Task.CompletedTask);
        
        await _roleService.RemovePermissionFromRole(roleId, permissionId, CancellationToken.None);
        
        Assert.DoesNotContain(role.RolePermissions, rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _permissionRepository.Verify(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.GetRolePermissionAsync(roleId, permissionId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.RemovePermissionFromRoleAsync(rolePermission, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemovePermissionFromRole_WhenRoleDoesNotExist_ThrowsNotFoundException()
    {
        int roleId = 1;
        int permissionId = 1;
        string roleName = "role";
        string permissionName = "permission";

        Role role = new Role
        {
            Id = roleId,
            Name = roleName,
            RolePermissions = new List<RolePermission>()
        };

        Permission permission = new Permission
        {
            Id = permissionId,
            Name = permissionName
        };

        RolePermission rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            Role = role,
            Permission = permission
        };
        
        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);
        
        Func<Task> result = () => _roleService.RemovePermissionFromRole(roleId, permissionId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _permissionRepository.Verify(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()), Times.Never);
        _roleRepository.Verify(x => x.GetRolePermissionAsync(roleId, permissionId, It.IsAny<CancellationToken>()), Times.Never);
        _roleRepository.Verify(x => x.RemovePermissionFromRoleAsync(rolePermission, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemovePermissionFromRole_WhenPermissionDoesNotExist_ThrowsNotFoundException()
    {
        int roleId = 1;
        int permissionId = 1;
        string roleName = "role";
        string permissionName = "permission";

        Role role = new Role
        {
            Id = roleId,
            Name = roleName
        };

        Permission permission = new Permission
        {
            Id = permissionId,
            Name = permissionName
        };

        RolePermission rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            Role = role,
            Permission = permission
        };

        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _permissionRepository.Setup(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Permission?)null);
        
        Func<Task> result = () => _roleService.RemovePermissionFromRole(roleId, permissionId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _permissionRepository.Verify(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.GetRolePermissionAsync(roleId, permissionId, It.IsAny<CancellationToken>()), Times.Never);
        _roleRepository.Verify(x => x.RemovePermissionFromRoleAsync(rolePermission, It.IsAny<CancellationToken>()), Times.Never);   
    }
    
    [Fact]
    public async Task RemovePermissionFromRole_WhenRolePermissionDoesNotExist_ThrowsNotFoundException()
    {
        int roleId = 1;
        int permissionId = 1;
        string roleName = "role";
        string permissionName = "permission";

        Role role = new Role
        {
            Id = roleId,
            Name = roleName,
            RolePermissions = new List<RolePermission>()
        };

        Permission permission = new Permission
        {
            Id = permissionId,
            Name = permissionName
        };

        RolePermission rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            Role = role,
            Permission = permission
        };

        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _permissionRepository.Setup(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);
        _roleRepository.Setup(x => x.GetRolePermissionAsync(roleId, permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RolePermission?)null);
        
        Func<Task> result = () => _roleService.RemovePermissionFromRole(roleId, permissionId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _permissionRepository.Verify(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.GetRolePermissionAsync(roleId, permissionId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.RemovePermissionFromRoleAsync(rolePermission, It.IsAny<CancellationToken>()), Times.Never);  
    }
}