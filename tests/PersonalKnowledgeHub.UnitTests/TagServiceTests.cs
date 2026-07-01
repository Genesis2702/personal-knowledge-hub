using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;
using Moq;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Services.Implementations;

namespace PersonalKnowledgeHub.UnitTests;

public class TagServiceTests
{
    private readonly ITagService _tagService;
    private readonly Mock<ITagRepository> _tagRepository;
    private readonly Mock<IAuthorizationService> _authorizationService;

    public TagServiceTests()
    {
        _tagRepository = new Mock<ITagRepository>();
        _authorizationService = new Mock<IAuthorizationService>();
        _tagService = new TagService(_tagRepository.Object, _authorizationService.Object, NullLogger<TagService>.Instance);
    }
    
    [Fact]
    public async Task AddTag_WithValidDataWhenTagDoesNotExist_ReturnsAddedTag()
    {
        int tagId = 1;
        long tagVersion = 0;
        int userId = 10;
        
        TagRequestDto tagRequest = new TagRequestDto { Name = " Test " };

        Tag tag = new Tag
        {
            Name = "test",
            UserId = userId,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null,
            Version = tagVersion
        };
        
        _tagRepository.Setup(x => x.IsTagExistAsync(tagRequest.Name, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _tagRepository
            .Setup(x => x.AddTagAsync(tag, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag addedTag, CancellationToken _) =>
            {
                addedTag.Id = tagId;
                return addedTag;
            });
        
        var result = await _tagService.AddTag(tagRequest, userId, CancellationToken.None);
        
        Assert.Equal(tagId, result.Id);
        Assert.Equal("test", result.Name);
        Assert.False(result.IsDeleted);
        Assert.Null(result.DeletedAt);
        Assert.Null(result.DeletedBy);
        Assert.Equal(tagVersion, result.Version);
        
        _tagRepository.Verify(x => x.AddTagAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task AddTag_WithValidDataWhenTagAlreadyExist_ThrowException()
    {
        int userId = 10;
        
        TagRequestDto tagRequest = new TagRequestDto
        {
            Name = " Test "
        };
        
        _tagRepository.Setup(x => x.IsTagExistAsync(tagRequest.Name, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        var result = async () => await _tagService.AddTag(tagRequest, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);
        
        _tagRepository.Verify(x => x.AddTagAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTagById_TagExist_ReturnsTag()
    {
        int tagId = 1;
        long tagVersion = 0;
        int userId = 10;
        
        Tag tag = new Tag
        {
            Id = tagId,
            Name = "test",
            UserId = userId,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null,
            Version = tagVersion
        };
        
        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        
        var result = await _tagService.GetTagById(tagId, userId, CancellationToken.None);
        
        Assert.Equal(tagId, result.Id);
        Assert.Equal("test", result.Name);
        Assert.Equal(userId, result.UserId);
        Assert.False(result.IsDeleted);
        Assert.Null(result.DeletedAt);
        Assert.Null(result.DeletedBy);
        Assert.Equal(tagVersion, result.Version);
        
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTagById_TagDoesNotExist_ThrowException()
    {
        int tagId = 1;
        int userId = 10;
        
        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);
        
        var result = async () => await _tagService.GetTagById(tagId, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTagById_TagExist_UpdatesTag()
    {
        int tagId = 1;
        int userId = 10;
        
        Tag tag = new Tag
        {
            Id = tagId,
            Name = "test",
            UserId = userId,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null,
            Version = 0
        };

        TagRequestDto tagRequest = new TagRequestDto
        {
            Name = " Updated Test "
        };

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));

        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        _authorizationService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), tag, "TagOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Success());
        _tagRepository.Setup(x => x.IsTagExistAsync("updated test", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _tagRepository.Setup(x => x.UpdateTagAsync(tagId, tag.Version, "updated test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _tagService.UpdateTagById(user, tagRequest, tagId, CancellationToken.None);
        
        _tagRepository.Verify(x => x.UpdateTagAsync(tagId, tag.Version, tagRequest.Name, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateTagById_TagDoesNotExist_ThrowException()
    {
        int tagId = 1;
        int userId = 10;

        TagRequestDto tagRequest = new TagRequestDto
        {
            Name = " Updated Test "
        };

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));
        
        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);
        
        var result = async () => await _tagService.UpdateTagById(user, tagRequest, tagId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _tagRepository.Verify(x => x.UpdateTagAsync(tagId, It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTagById_TagExist_DeletesTag()
    {
        int tagId = 1;
        int userId = 10;

        Tag tag = new Tag
        {
            Id = tagId,
            Name = "test",
            UserId = userId,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null,
            Version = 0
        };

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));

        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        _authorizationService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), tag, "TagOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Success());

        await _tagService.DeleteTagById(user, tagId, CancellationToken.None);
        
        _tagRepository.Verify(x => x.DeleteTagAsync(tag, userId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteTagById_TagDoesNotExist_ThrowException()
    {
        int tagId = 1;
        int userId = 10;

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));

        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        var result = async () => await _tagService.DeleteTagById(user, tagId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _tagRepository.Verify(x => x.DeleteTagAsync(It.IsAny<Tag>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}