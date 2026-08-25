using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Implementations;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.UnitTests;

public class PermissionServiceTests
{
    private readonly Mock<IPermissionRepository> _permissionRepository;
    private readonly IPermissionService _permissionService;
    
    public PermissionServiceTests()
    {
        _permissionRepository = new Mock<IPermissionRepository>();
        _permissionService = new PermissionService(_permissionRepository.Object, NullLogger<PermissionService>.Instance);
    }

    [Fact]
    public async Task GetPermissions_WhenPermissionsExist_ReturnsListOfPermissions()
    {
        List<Permission> permissions = 
        [
            new Permission
            {
                Id = 1,
                Name = "permission1",
            },
            new Permission
            {
                Id = 2,
                Name = "permission2",
            },
            new Permission
            {
                Id = 3,
                Name = "permission3",
            }
        ];
        
        _permissionRepository.Setup(x => x.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var result = await _permissionService.GetPermissions(CancellationToken.None);
        
        Assert.Equal(3, result.Count);
        Assert.Equal(permissions, result);
        
        _permissionRepository.Verify(x => x.GetPermissionsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPermissions_WhenNoPermissionsExist_ReturnsEmptyList()
    {
        List<Permission> permissions = [];
        
        _permissionRepository.Setup(x => x.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);
        
        var result = await _permissionService.GetPermissions(CancellationToken.None);
        
        Assert.Empty(result);
        
        _permissionRepository.Verify(x => x.GetPermissionsAsync(It.IsAny<CancellationToken>()), Times.Once);  
    }

    [Fact]
    public async Task GetPermissionById_WhenPermissionExists_ReturnsPermission()
    {
        int permissionId = 1;
        string permissionName = "permission";
        
        Permission permission = new Permission
        {
            Id = permissionId,
            Name = permissionName
        };
        
        _permissionRepository.Setup(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);
        
        var result = await _permissionService.GetPermissionById(permissionId, CancellationToken.None);
        
        Assert.Equal(permissionId, result.Id);
        Assert.Equal(permissionName, result.Name);
        
        _permissionRepository.Verify(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()), Times.Once); 
    }

    [Fact]
    public async Task GetPermissionById_WhenPermissionDoesNotExist_ThrowsNotFoundException()
    {
        int permissionId = 1;
        
        _permissionRepository.Setup(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Permission?)null);

        Func<Task> result = () => _permissionService.GetPermissionById(permissionId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _permissionRepository.Verify(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task AddPermission_WhenPermissionDoesNotExist_ReturnsAddedPermission()
    {
        int permissionId = 1;
        string permissionName = "permission";

        _permissionRepository.Setup(x => x.IsPermissionExistAsync(permissionName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _permissionRepository.Setup(x => x.AddPermissionAsync(It.Is<Permission>(permission =>
                permission.Name == permissionName), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Permission addedPermission, CancellationToken _) =>
            {
                addedPermission.Id = permissionId;
                return addedPermission;
            });
        
        var result = await _permissionService.AddPermission(permissionName, CancellationToken.None);
        
        Assert.Equal(permissionId, result.Id);
        Assert.Equal(permissionName, result.Name);
        
        _permissionRepository.Verify(x => x.IsPermissionExistAsync(permissionName, It.IsAny<CancellationToken>()), Times.Once);
        _permissionRepository.Verify(x => x.AddPermissionAsync(It.Is<Permission>(permission =>
                permission.Name == permissionName), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddPermission_WhenPermissionAlreadyExists_ThrowsConflictException()
    {
        int permissionId = 1;
        string permissionName = "permission";
        
        _permissionRepository.Setup(x => x.IsPermissionExistAsync(permissionName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        Func<Task> result = () => _permissionService.AddPermission(permissionName, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);
        
        _permissionRepository.Verify(x => x.IsPermissionExistAsync(permissionName, It.IsAny<CancellationToken>()), Times.Once);
        _permissionRepository.Verify(x => x.AddPermissionAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdatePermissionById_WhenPermissionExists_ReturnsUpdatedPermission()
    {
        int permissionId = 1;
        string permissionCurrentName = "permission";
        string permissionNewName = "new permission";
        
        Permission permission = new Permission
        {
            Id = permissionId,
            Name = permissionCurrentName
        };

        _permissionRepository.Setup(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);
        _permissionRepository.Setup(x => x.UpdatePermissionAsync(permission, permissionNewName, It.IsAny<CancellationToken>()))
            .Callback<Permission, string, CancellationToken>((p, n, _) =>
                {
                    p.Name = n;
                })
            .Returns(Task.CompletedTask);
        
        await _permissionService.UpdatePermissionById(permissionId, permissionNewName, CancellationToken.None);
        
        Assert.Equal(permissionNewName, permission.Name);
        
        _permissionRepository.Verify(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()), Times.Once);
        _permissionRepository.Verify(x => x.UpdatePermissionAsync(permission, permissionNewName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePermissionById_WhenPermissionDoesNotExist_ThrowsNotFoundException()
    {
        int permissionId = 1;
        string permissionNewName = "new permission";
        
        _permissionRepository.Setup(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Permission?)null);
        
        Func<Task> result = () => _permissionService.UpdatePermissionById(permissionId, permissionNewName, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _permissionRepository.Verify(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()), Times.Once);
        _permissionRepository.Verify(x => x.UpdatePermissionAsync(It.IsAny<Permission>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeletePermissionById_WhenPermissionExists_ReturnsDeletedPermission()
    {
        int permissionId = 1;
        string permissionName = "permission";
        
        Permission permission = new Permission
        {
            Id = permissionId,
            Name = permissionName
        };
        
        _permissionRepository.Setup(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);
        
        await _permissionService.DeletePermissionById(permissionId, CancellationToken.None);
        
        _permissionRepository.Verify(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()), Times.Once);
        _permissionRepository.Verify(x => x.DeletePermissionAsync(permission, It.IsAny<CancellationToken>()), Times.Once); 
    }

    [Fact]
    public async Task DeletePermissionById_WhenPermissionDoesNotExist_ThrowsNotFoundException()
    {
        int permissionId = 1;
        
        _permissionRepository.Setup(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Permission?)null);
        
        Func<Task> result = () => _permissionService.DeletePermissionById(permissionId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _permissionRepository.Verify(x => x.GetPermissionByIdAsync(permissionId, It.IsAny<CancellationToken>()), Times.Once);
        _permissionRepository.Verify(x => x.DeletePermissionAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}