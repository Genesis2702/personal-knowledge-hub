using Moq;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Implementations;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.UnitTests;

public class ResourceTagServiceTests
{
    private readonly Mock<IResourceTagRepository> _resourceTagRepository;
    private readonly Mock<IResourceRepository> _resourceRepository;
    private readonly Mock<ITagRepository> _tagRepository;
    private readonly IResourceTagService _resourceTagService;

    public ResourceTagServiceTests()
    {
        _resourceTagRepository = new Mock<IResourceTagRepository>();
        _resourceRepository = new Mock<IResourceRepository>();
        _tagRepository = new Mock<ITagRepository>();
        _resourceTagService = new ResourceTagService(_resourceTagRepository.Object, _resourceRepository.Object, _tagRepository.Object);
    }
    
    [Fact]
    public async Task AddResourceTag_WhenResourceTagDoesNotExist_AddsResourceTag()
    {
        int tagId = 1;
        int resourceId = 1;
        int userId = 10;

        Tag tag = new Tag
        {
            Name = "test",
            UserId = userId
        };

        Resource resource = new Resource
        {
            Title = "test",
            ResourceType = ResourceType.Article,
            UserId = userId
        };

        _resourceTagRepository.Setup(x =>
                x.IsResourceTagExistAsync(tagId, resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        _resourceTagRepository.Setup(x => x.AddResourceTagAsync(It.Is<ResourceTag>(rt => rt.TagId == tagId && rt.ResourceId == resourceId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResourceTag resourceTag, CancellationToken _) => resourceTag);
        
        var result = await _resourceTagService.AddResourceTag(tagId, resourceId, userId, CancellationToken.None);

        Assert.Contains(result.ResourceTags, rt => rt.Tag == tag && rt.Resource == resource);
        
        _resourceTagRepository.Verify(x => x.IsResourceTagExistAsync(tagId, resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _resourceTagRepository.Verify(x => x.AddResourceTagAsync(It.Is<ResourceTag>(rt => rt.TagId == tagId && rt.ResourceId == resourceId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddResourceTag_WhenResourceTagAlreadyExists_ThrowsConflictException()
    {
        int tagId = 1;
        int resourceId = 1;
        int userId = 10;

        _resourceTagRepository.Setup(x =>
                x.IsResourceTagExistAsync(tagId, resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        Func<Task> result = () => _resourceTagService.AddResourceTag(tagId, resourceId, userId, CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(result);
        
        _resourceTagRepository.Verify(x => x.IsResourceTagExistAsync(tagId, resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Never);
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Never);
        _resourceTagRepository.Verify(x => x.AddResourceTagAsync(It.Is<ResourceTag>(rt => rt.TagId == tagId && rt.ResourceId == resourceId), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddResourceTag_WhenResourceDoesNotExist_ThrowsNotFoundException()
    {
        int tagId = 1;
        int resourceId = 1;
        int userId = 10;

        Tag tag = new Tag
        {
            Name = "test",
            UserId = userId
        };

        _resourceTagRepository.Setup(x =>
                x.IsResourceTagExistAsync(tagId, resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource?)null);
        
        Func<Task> result = () => _resourceTagService.AddResourceTag(tagId, resourceId, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _resourceTagRepository.Verify(x => x.IsResourceTagExistAsync(tagId, resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _resourceTagRepository.Verify(x => x.AddResourceTagAsync(It.Is<ResourceTag>(rt => rt.TagId == tagId && rt.ResourceId == resourceId), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddResourceTag_WhenTagDoesNotExist_ThrowsNotFoundException()
    {
        int tagId = 1;
        int resourceId = 1;
        int userId = 10;

        _resourceTagRepository.Setup(x =>
                x.IsResourceTagExistAsync(tagId, resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);
        
        Func<Task> result = () => _resourceTagService.AddResourceTag(tagId, resourceId, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _resourceTagRepository.Verify(x => x.IsResourceTagExistAsync(tagId, resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Never);
        _resourceTagRepository.Verify(x => x.AddResourceTagAsync(It.Is<ResourceTag>(rt => rt.TagId == tagId && rt.ResourceId == resourceId), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task AddResourceTag_WhenUserDoesNotOwnResource_ThrowsForbiddenException()
    {
        int tagId = 1;
        int resourceId = 1;
        int userId = 10;

        Tag tag = new Tag
        {
            Name = "test",
            UserId = userId
        };

        Resource resource = new Resource
        {
            Title = "test",
            ResourceType = ResourceType.Article,
            UserId = 20
        };

        _resourceTagRepository.Setup(x =>
                x.IsResourceTagExistAsync(tagId, resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        
        Func<Task> result = () => _resourceTagService.AddResourceTag(tagId, resourceId, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ForbiddenException>(result);
        
        _resourceTagRepository.Verify(x => x.IsResourceTagExistAsync(tagId, resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _resourceTagRepository.Verify(x => x.AddResourceTagAsync(It.Is<ResourceTag>(rt => rt.TagId == tagId && rt.ResourceId == resourceId), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddResourceTag_WhenUserDoesNotOwnTag_ThrowsForbiddenException()
    {
        int tagId = 1;
        int resourceId = 1;
        int userId = 10;

        Tag tag = new Tag
        {
            Name = "test",
            UserId = 20
        };

        _resourceTagRepository.Setup(x =>
                x.IsResourceTagExistAsync(tagId, resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        
        Func<Task> result = () => _resourceTagService.AddResourceTag(tagId, resourceId, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ForbiddenException>(result);
        
        _resourceTagRepository.Verify(x => x.IsResourceTagExistAsync(tagId, resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Never);
        _resourceTagRepository.Verify(x => x.AddResourceTagAsync(It.Is<ResourceTag>(rt => rt.TagId == tagId && rt.ResourceId == resourceId), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task DeleteResourceTag_WhenResourceTagExists_DeletesResourceTag()
    {
        int tagId = 1;
        int resourceId = 1;
        int userId = 10;
        
        Tag tag = new Tag
        {
            Name = "test",
            UserId = userId
        };

        Resource resource = new Resource
        {
            Title = "test",
            ResourceType = ResourceType.Article,
            UserId = userId
        };

        ResourceTag resourceTag = new ResourceTag
        {
            TagId = tagId,
            ResourceId = resourceId,
            Tag = tag,
            Resource = resource
        };
        
        _resourceTagRepository.Setup(x => x.GetResourceTagByIdAsync(tagId, resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resourceTag);
        
        await _resourceTagService.DeleteResourceTag(tagId, resourceId, userId, CancellationToken.None);
        
        _resourceTagRepository.Verify(x => x.GetResourceTagByIdAsync(tagId, resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _resourceTagRepository.Verify(x => x.DeleteResourceTagAsync(resourceTag, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteResourceTag_WhenResourceTagDoesNotExist_ThrowsNotFoundException()
    {
        int tagId = 1;
        int resourceId = 1;
        int userId = 10;
        
        _resourceTagRepository.Setup(x => x.GetResourceTagByIdAsync(tagId, resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResourceTag?)null);
        
        Func<Task> result = () => _resourceTagService.DeleteResourceTag(tagId, resourceId, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _resourceTagRepository.Verify(x => x.GetResourceTagByIdAsync(tagId, resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _resourceTagRepository.Verify(x => x.DeleteResourceTagAsync(It.IsAny<ResourceTag>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteResourceTag_WhenUserDoesNotOwnResourceTag_ThrowsForbiddenException()
    {
        int tagId = 1;
        int resourceId = 1;
        int userId = 10;
        
        Tag tag = new Tag
        {
            Name = "test",
            UserId = 20
        };

        Resource resource = new Resource
        {
            Title = "test",
            ResourceType = ResourceType.Article,
            UserId = 20
        };

        ResourceTag resourceTag = new ResourceTag
        {
            TagId = tagId,
            ResourceId = resourceId,
            Tag = tag,
            Resource = resource
        };
        
        _resourceTagRepository.Setup(x => x.GetResourceTagByIdAsync(tagId, resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resourceTag);
        
        Func<Task> result = () => _resourceTagService.DeleteResourceTag(tagId, resourceId, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ForbiddenException>(result);
        
        _resourceTagRepository.Verify(x => x.GetResourceTagByIdAsync(tagId, resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _resourceTagRepository.Verify(x => x.DeleteResourceTagAsync(resourceTag, It.IsAny<CancellationToken>()), Times.Never);
    }
}