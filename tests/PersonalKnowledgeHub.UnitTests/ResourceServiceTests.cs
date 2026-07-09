using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Implementations;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.UnitTests;

public class ResourceServiceTests
{
    private readonly Mock<IResourceRepository> _resourceRepository;
    private readonly Mock<ITagRepository> _tagRepository;
    private readonly Mock<IAuthorizationService> _authorizationService;
    private readonly IResourceService _resourceService;

    public ResourceServiceTests()
    {
        _resourceRepository = new Mock<IResourceRepository>();
        _tagRepository = new Mock<ITagRepository>();
        _authorizationService = new Mock<IAuthorizationService>();
        _resourceService = new ResourceService(_resourceRepository.Object, _tagRepository.Object,
            _authorizationService.Object, NullLogger<ResourceService>.Instance);
    }

    [Fact]
    public async Task GetResources_WhenResourcesExist_ReturnsListOfResources()
    {
        int pageIndex = 1;
        int pageSize = 3;
        
        int userId = 10;

        int resourcesCount = 1;

        List<Resource> resources =
        [
            new Resource
            {
                Id = 1,
                Title = "test1",
                ResourceType = ResourceType.Article,
                UserId = userId
            },
            new Resource
            {
                Id = 2,
                Title = "test2",
                ResourceType = ResourceType.Video,
                UserId = userId
            },
            new Resource
            {
                Id = 3,
                Title = "test3",
                ResourceType = ResourceType.File,
                UserId = userId
            }
        ];

        ResourceQueryRequestDto resourceQueryRequest = new ResourceQueryRequestDto
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            ResourceType = ResourceType.Article,
            Search = "test",
            TagId = null,
        };

        _resourceRepository.Setup(x => x.GetResourcesAsync(
                userId,
                resourceQueryRequest.PageIndex,
                resourceQueryRequest.PageSize,
                resourceQueryRequest.TagId,
                resourceQueryRequest.ResourceType,
                resourceQueryRequest.Search,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((resources, resourcesCount));
        
        var result = await _resourceService.GetResources(userId, resourceQueryRequest, CancellationToken.None);
        
        Assert.NotEmpty(result.Items);
        Assert.Equal((int)Math.Ceiling((double)resourcesCount / pageSize), result.PageCount);
        Assert.Equal(pageIndex, result.PageIndex);
        Assert.Equal(pageSize, result.PageSize);
        
        _resourceRepository.Verify(x => x.GetResourcesAsync(userId,
            resourceQueryRequest.PageIndex,
            resourceQueryRequest.PageSize,
            resourceQueryRequest.TagId,
            resourceQueryRequest.ResourceType,
            resourceQueryRequest.Search,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetResources_WhenNoResourcesExist_ReturnsEmptyList()
    {
        int pageIndex = 1;
        int pageSize = 3;
        
        int userId = 10;

        int resourcesCount = 0;

        List<Resource> resources = [];

        ResourceQueryRequestDto resourceQueryRequest = new ResourceQueryRequestDto
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            ResourceType = ResourceType.Article,
            Search = null,
            TagId = null,
        };
        
        _resourceRepository.Setup(x => x.GetResourcesAsync(
                userId,
                resourceQueryRequest.PageIndex,
                resourceQueryRequest.PageSize,
                resourceQueryRequest.TagId,
                resourceQueryRequest.ResourceType,
                resourceQueryRequest.Search,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((resources, resourcesCount));

        var result = await _resourceService.GetResources(userId, resourceQueryRequest, CancellationToken.None);
        
        Assert.Empty(result.Items);
        
        _resourceRepository.Verify(x => x.GetResourcesAsync(userId,
            resourceQueryRequest.PageIndex,
            resourceQueryRequest.PageSize,
            resourceQueryRequest.TagId,
            resourceQueryRequest.ResourceType,
            resourceQueryRequest.Search,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetResourceById_WhenResourceExists_ReturnsResource()
    {
        int resourceId = 1;
        int userId = 10;

        Resource resource = new Resource
        {
            Id = resourceId,
            Title = "test",
            ResourceType = ResourceType.Article,
            UserId = userId
        };
        
        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        
        var result = await _resourceService.GetResourceById(resourceId, userId, CancellationToken.None);
        
        Assert.Equal(resourceId, result.Id);
        Assert.Equal("test", result.Title);
        Assert.Equal(ResourceType.Article, result.ResourceType);
        Assert.Equal(userId, result.UserId);
        
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task GetResourceById_WhenResourceDoesNotExist_ThrowsNotFoundException()
    {
        int resourceId = 1;
        int userId = 10;
        
        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource?)null);
        
        Func<Task> result = () => _resourceService.GetResourceById(resourceId, userId, CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task GetResourceById_WhenUserDoesNotOwnResource_ThrowsForbiddenException()
    {
        int resourceId = 1;
        int userId = 10;

        Resource resource = new Resource
        {
            Id = resourceId,
            Title = "test",
            ResourceType = ResourceType.Article,
            UserId = 20
        };

        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        
        Func<Task> result = () => _resourceService.GetResourceById(resourceId, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ForbiddenException>(result);
        
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddResource_WhenResourceDoesNotExist_ReturnsAddedResource()
    {
        int resourceId = 1;
        int userId = 10;

        ResourceRequestDto resourceRequest = new ResourceRequestDto
        {
            Title = "test title",
            Url = "test url",
            Description = "test description",
            ResourceType = ResourceType.Article
        };

        _resourceRepository
            .Setup(x => x.IsTitleExistAsync(resourceRequest.Title, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _resourceRepository.Setup(x => x.AddResourceAsync(It.Is<Resource>(r =>
                r.Title == resourceRequest.Title &&
                r.Url == resourceRequest.Url &&
                r.Description == resourceRequest.Description &&
                r.ResourceType == resourceRequest.ResourceType), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource addedResource, CancellationToken _) =>
            {
                addedResource.Id = resourceId;
                return addedResource;
            });
        
        var result = await _resourceService.AddResource(resourceRequest, userId, CancellationToken.None);
        
        Assert.Equal(resourceId, result.Id);
        Assert.Equal(resourceRequest.Title, result.Title);
        Assert.Equal(resourceRequest.Url, result.Url);
        Assert.Equal(resourceRequest.Description, result.Description);
        Assert.Equal(resourceRequest.ResourceType, result.ResourceType);
        
        _resourceRepository.Verify(x => x.IsTitleExistAsync(resourceRequest.Title, userId, It.IsAny<CancellationToken>()), Times.Once);
        _resourceRepository.Verify(x => x.AddResourceAsync(It.Is<Resource>(r =>
                r.Title == resourceRequest.Title &&
                r.Url == resourceRequest.Url &&
                r.Description == resourceRequest.Description &&
                r.ResourceType == resourceRequest.ResourceType), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddResource_WhenResourceExists_ThrowsConflictException()
    {
        int userId = 10;
        
        ResourceRequestDto resourceRequest = new ResourceRequestDto
        {
            Title = "test title",
            Url = "test url",
            Description = "test description",
            ResourceType = ResourceType.Article
        };
        
        _resourceRepository
            .Setup(x => x.IsTitleExistAsync(resourceRequest.Title, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        Func<Task> result = () => _resourceService.AddResource(resourceRequest, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);
        
        _resourceRepository.Verify(x => x.IsTitleExistAsync(resourceRequest.Title, userId, It.IsAny<CancellationToken>()), Times.Once);
        _resourceRepository.Verify(x => x.AddResourceAsync(It.IsAny<Resource>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateResourceById_WhenResourceExists_UpdatesResource()
    {
        int resourceId = 1;
        int userId = 10;

        Resource resource = new Resource
        {
            Id = resourceId,
            Title = "test",
            ResourceType = ResourceType.Article,
            UserId = userId
        };

        ResourceUpdateRequestDto resourceUpdateRequest = new ResourceUpdateRequestDto
        {
            Title = "updated title"
        };

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));
        
        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        _authorizationService.Setup(x => x.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Success());
        _resourceRepository.Setup(x => x.UpdateResourceAsync(resourceId, resource.Version, resourceUpdateRequest.Title, resourceUpdateRequest.Url, resourceUpdateRequest.Description, It.IsAny<CancellationToken>()))
            .Callback<int, long, string, string, string, CancellationToken>((id, version, title, url, description, _) =>
            {
                resource.Title = title;
                resource.Url = url;
                resource.Description = description;
            })
            .ReturnsAsync(1);
        
        await _resourceService.UpdateResourceById(user, resourceId, resourceUpdateRequest, CancellationToken.None);
        
        Assert.Equal(resourceUpdateRequest.Title, resource.Title);
        Assert.Equal(resource.Url, resourceUpdateRequest.Url);
        Assert.Equal(resource.Description, resourceUpdateRequest.Description);
        
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin"), Times.Once);
        _resourceRepository.Verify(x => x.UpdateResourceAsync(resourceId, resource.Version, resourceUpdateRequest.Title, resourceUpdateRequest.Url, resourceUpdateRequest.Description, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateResourceById_WhenResourceDoesNotExist_ThrowsNotFoundException()
    {
        int resourceId = 1;
        int userId = 10;

        ResourceUpdateRequestDto resourceUpdateRequest = new ResourceUpdateRequestDto
        {
            Title = "updated title"
        };

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));
        
        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource?)null);
        
        Func<Task> result = () => _resourceService.UpdateResourceById(user, resourceId, resourceUpdateRequest, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, It.IsAny<Resource>(), "ResourceOwnerOrAdmin"), Times.Never);
        _resourceRepository.Verify(x => x.UpdateResourceAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        
    }
    
    [Fact]
    public async Task UpdateResourceById_WhenUserDoesNotOwnResource_ThrowsForbiddenException()
    {
        int resourceId = 1;
        int userId = 10;

        Resource resource = new Resource
        {
            Id = resourceId,
            Title = "test",
            ResourceType = ResourceType.Article,
            UserId = 20
        };

        ResourceUpdateRequestDto resourceUpdateRequest = new ResourceUpdateRequestDto
        {
            Title = "updated title"
        };

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));

        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        _authorizationService.Setup(x => x.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Failed());
        
        Func<Task> result = () => _resourceService.UpdateResourceById(user, resourceId, resourceUpdateRequest, CancellationToken.None);
        
        await Assert.ThrowsAsync<ForbiddenException>(result);
        
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin"), Times.Once);
        _resourceRepository.Verify(x => x.UpdateResourceAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteResourceById_WhenResourceExists_DeletesResource()
    {
        int resourceId = 1;
        int userId = 10;

        Resource resource = new Resource
        {
            Id = resourceId,
            Title = "test",
            ResourceType = ResourceType.Article,
            UserId = userId,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null
        };

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));
        
        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        _authorizationService.Setup(x => x.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Success());
        _resourceRepository.Setup(x => x.DeleteResourceAsync(resource, userId, It.IsAny<CancellationToken>()))
            .Callback<Resource, int, CancellationToken>((r, _, _) =>
            {
                r.IsDeleted = true;
                r.DeletedAt = DateTime.UtcNow;
                r.DeletedBy = userId;
            }).Returns(Task.CompletedTask);
        
        await _resourceService.DeleteResourceById(user, resourceId, CancellationToken.None);
        
        Assert.True(resource.IsDeleted);
        Assert.NotNull(resource.DeletedAt);
        Assert.Equal(userId, resource.DeletedBy);
        
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin"), Times.Once);
        _resourceRepository.Verify(x => x.DeleteResourceAsync(resource, userId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteResourceById_WhenResourceDoesNotExist_ThrowsNotFoundException()
    {
        int resourceId = 1;
        int userId = 10;

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));
        
        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource?)null);
        
        Func<Task> result = () => _resourceService.DeleteResourceById(user, resourceId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, It.IsAny<Resource>(), "ResourceOwnerOrAdmin"), Times.Never);
        _resourceRepository.Verify(x => x.DeleteResourceAsync(It.IsAny<Resource>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task DeleteResourceById_WhenUserDoesNotOwnResource_ThrowsForbiddenException()
    {
        int resourceId = 1;
        int userId = 10;

        Resource resource = new Resource
        {
            Id = resourceId,
            Title = "test",
            ResourceType = ResourceType.Article,
            UserId = 20,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null
        };

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));
        
        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        _authorizationService.Setup(x => x.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Failed());
        
        Func<Task> result = () => _resourceService.DeleteResourceById(user, resourceId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ForbiddenException>(result);
        
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin"), Times.Once);
        _resourceRepository.Verify(x => x.DeleteResourceAsync(It.IsAny<Resource>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CleanUpResources_WhenCalled_CallsRepository()
    {
        await _resourceService.CleanUpResources(CancellationToken.None);
        
        _resourceRepository.Verify(x => x.CleanUpResourcesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreResourceById_WhenResourceExists_ReturnsRestoredResource()
    {
        int resourceId = 1;
        int userId = 10;

        Resource resource = new Resource
        {
            Id = resourceId,
            Title = "test",
            ResourceType = ResourceType.Article,
            UserId = userId,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = userId
        };

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));
        
        _resourceRepository.Setup(x => x.GetResourceByIdForRestoreAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        _authorizationService.Setup(x => x.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Success());
        _resourceRepository.Setup(x => x.RestoreResourceAsync(resource, It.IsAny<CancellationToken>()))
            .Callback<Resource, CancellationToken>((r, _) =>
            {
                r.IsDeleted = false;
                r.DeletedAt = null;
                r.DeletedBy = null;
            }).ReturnsAsync(resource);
        
        var result = await _resourceService.RestoreResourceById(user, resourceId, CancellationToken.None);
        
        Assert.False(result.IsDeleted);
        Assert.Null(result.DeletedAt);
        Assert.Null(result.DeletedBy);
        
        _resourceRepository.Verify(x => x.GetResourceByIdForRestoreAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin"), Times.Once);
        _resourceRepository.Verify(x => x.RestoreResourceAsync(resource, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreResourceById_WhenResourceDoesNotExist_ThrowsNotFoundException()
    {
        int resourceId = 1;
        int userId = 10;

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));
        
        _resourceRepository.Setup(x => x.GetResourceByIdForRestoreAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource?)null);
        
        Func<Task> result = () => _resourceService.RestoreResourceById(user, resourceId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _resourceRepository.Verify(x => x.GetResourceByIdForRestoreAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, It.IsAny<Resource>(), "ResourceOwnerOrAdmin"), Times.Never);
        _resourceRepository.Verify(x => x.RestoreResourceAsync(It.IsAny<Resource>(), It.IsAny<CancellationToken>()), Times.Never);
        
    }

    [Fact]
    public async Task RestoreResourceById_WhenUserDoesNotOwnResource_ThrowsForbiddenException()
    {
        int resourceId = 1;
        int userId = 10;

        Resource resource = new Resource
        {
            Id = resourceId,
            Title = "test",
            ResourceType = ResourceType.Article,
            UserId = 20,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null
        };

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));
        
        _resourceRepository.Setup(x => x.GetResourceByIdForRestoreAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        _authorizationService.Setup(x => x.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Failed());
        
        Func<Task> result = () => _resourceService.RestoreResourceById(user, resourceId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ForbiddenException>(result);
        
        _resourceRepository.Verify(x => x.GetResourceByIdForRestoreAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin"), Times.Once);
        _resourceRepository.Verify(x => x.RestoreResourceAsync(It.IsAny<Resource>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}