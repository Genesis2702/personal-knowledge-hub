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
    public async Task AddTag_WhenTagDoesNotExist_ReturnsAddedTag()
    {
        int tagId = 1;
        int userId = 10;
        
        TagRequestDto tagRequest = new TagRequestDto { Name = " Test " };
        
        string normalizedTagName = tagRequest.Name.Trim().ToLower();
        
        _tagRepository.Setup(x => x.IsTagExistAsync(normalizedTagName, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _tagRepository
            .Setup(x => x.AddTagAsync(It.Is<Tag>(t =>
                t.Name == normalizedTagName &&
                t.UserId == userId &&
                t.IsDeleted == false &&
                t.DeletedAt == null &&
                t.DeletedBy == null), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag tag, CancellationToken _) =>
            {
                tag.Id = tagId;
                return tag;
            });
        
        var result = await _tagService.AddTag(tagRequest, userId, CancellationToken.None);
        
        Assert.Equal(tagId, result.Id);
        Assert.Equal(normalizedTagName, result.Name);
        Assert.False(result.IsDeleted);
        Assert.Null(result.DeletedAt);
        Assert.Null(result.DeletedBy);
        
        _tagRepository.Verify(x => x.IsTagExistAsync(normalizedTagName, userId, It.IsAny<CancellationToken>()), Times.Once);
        _tagRepository.Verify(x => x.AddTagAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task AddTag_WhenTagAlreadyExists_ThrowsConflictException()
    {
        int userId = 10;
        
        TagRequestDto tagRequest = new TagRequestDto
        {
            Name = " Test "
        };
        
        string normalizedTagName = tagRequest.Name.Trim().ToLower();
        
        _tagRepository.Setup(x => x.IsTagExistAsync(normalizedTagName, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        Func<Task> result = () => _tagService.AddTag(tagRequest, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);
        
        _tagRepository.Verify(x => x.IsTagExistAsync(normalizedTagName, userId, It.IsAny<CancellationToken>()), Times.Once);
        _tagRepository.Verify(x => x.AddTagAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTagsByUserId_WhenTagsExist_ReturnsListOfTags()
    {
        int userId = 10;

        List<Tag> tags =
        [
            new Tag
            {
                Id = 1,
                Name = "test1",
                UserId = userId,
                IsDeleted = false,
                DeletedAt = null,
                DeletedBy = null,
            },
            new Tag
            {
                Id = 2,
                Name = "test2",
                UserId = userId,
                IsDeleted = false,
                DeletedAt = null,
                DeletedBy = null,
            },
            new Tag
            {
                Id = 3,
                Name = "test3",
                UserId = userId,
                IsDeleted = false,
                DeletedAt = null,
                DeletedBy = null,
            }
        ];

        _tagRepository.Setup(x => x.GetTagsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);
        
        var result = await _tagService.GetTags(userId, CancellationToken.None);
        
        Assert.Equal(3, result.Count);
        Assert.Equal(tags, result);
        
        _tagRepository.Verify(x => x.GetTagsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTagsByUserId_WhenNoTagsExist_ReturnsEmptyList()
    {
        int userId = 10;

        List<Tag> tags = [];
        
        _tagRepository.Setup(x => x.GetTagsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);
        
        var result = await _tagService.GetTags(userId, CancellationToken.None);
        
        Assert.Empty(result);
        
        _tagRepository.Verify(x => x.GetTagsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTagById_WhenTagExists_ReturnsTag()
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
    public async Task GetTagById_WhenTagDoesNotExist_ThrowsNotFoundException()
    {
        int tagId = 1;
        int userId = 10;
        
        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);
        
        Func<Task> result = () => _tagService.GetTagById(tagId, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTagById_WhenUserDoesNotOwnTag_ThrowsForbiddenException()
    {
        int tagId = 1;
        int userId = 10;

        Tag tag = new Tag
        {
            Id = tagId,
            Name = "test",
            UserId = 20,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null,
        };
        
        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        
        Func<Task> result = () => _tagService.GetTagById(tagId, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ForbiddenException>(result);
        
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);   
    }

    [Fact]
    public async Task UpdateTagById_WhenTagExists_UpdatesTag()
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
        
        string normalizedTagName = tagRequest.Name.Trim().ToLower();

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));

        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        _authorizationService.Setup(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Success());
        _tagRepository.Setup(x => x.IsTagExistAsync(normalizedTagName, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _tagRepository.Setup(x => x.UpdateTagAsync(tagId, tag.Version, normalizedTagName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _tagService.UpdateTagById(user, tagRequest, tagId, CancellationToken.None);
        
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"), Times.Once);
        _tagRepository.Verify(x => x.IsTagExistAsync(normalizedTagName, userId, It.IsAny<CancellationToken>()), Times.Once);
        _tagRepository.Verify(x => x.UpdateTagAsync(tagId, tag.Version, normalizedTagName, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateTagById_WhenTagDoesNotExist_ThrowsNotFoundException()
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
        
        Func<Task> result = () => _tagService.UpdateTagById(user, tagRequest, tagId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);

        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _tagRepository.Verify(x => x.UpdateTagAsync(tagId, It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTagById_WhenUserDoesNotOwnTag_ThrowsForbiddenException()
    {
        int tagId = 1;
        int userId = 10;

        TagRequestDto tagRequest = new TagRequestDto
        {
            Name = " Updated Test "
        };
        
        Tag tag = new Tag
        {
            Id = tagId,
            Name = "test",
            UserId = 20,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null,
        };

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));

        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        _authorizationService.Setup(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Failed());
        
        Func<Task> result = () => _tagService.UpdateTagById(user, tagRequest, tagId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ForbiddenException>(result);
        
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"), Times.Once);  
        _tagRepository.Verify(x => x.UpdateTagAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never); 
    }

    [Fact]
    public async Task UpdateTagById_WhenNewTagNameAlreadyExists_ThrowsConflictException()
    {
        int tagId = 1;
        int userId = 10;

        TagRequestDto tagRequest = new TagRequestDto
        {
            Name = " Updated Test "
        };
        
        string normalizedTagName = tagRequest.Name.Trim().ToLower();
        
        Tag tag = new Tag
        {
            Id = tagId,
            Name = "test",
            UserId = userId,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null,
        };

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));

        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        _authorizationService.Setup(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Success());
        _tagRepository.Setup(x => x.IsTagExistAsync(normalizedTagName, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        Func<Task> result = () => _tagService.UpdateTagById(user, tagRequest, tagId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);
        
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"), Times.Once);
        _tagRepository.Verify(x => x.IsTagExistAsync(normalizedTagName, userId, It.IsAny<CancellationToken>()), Times.Once);  
        _tagRepository.Verify(x => x.UpdateTagAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTagById_WhenTagAlreadyUpdatedByAnotherUser_ThrowsConflictException()
    {
        int tagId = 1;
        int userId = 10;

        TagRequestDto tagRequest = new TagRequestDto
        {
            Name = " Updated Test "
        };
        
        Tag tag = new Tag
        {
            Id = tagId,
            Name = "test",
            UserId = userId,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null,
        };

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));

        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        _authorizationService.Setup(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Success());
        _tagRepository.Setup(x => x.IsTagExistAsync(tagRequest.Name.Trim().ToLower(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _tagRepository.Setup(x => x.UpdateTagAsync(tagId, tag.Version, tagRequest.Name.Trim().ToLower(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        
        Func<Task> result = () => _tagService.UpdateTagById(user, tagRequest, tagId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);
        
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"), Times.Once);
        _tagRepository.Verify(x => x.IsTagExistAsync(tagRequest.Name.Trim().ToLower(), userId, It.IsAny<CancellationToken>()), Times.Once); 
        _tagRepository.Verify(x => x.UpdateTagAsync(tagId, tag.Version, tagRequest.Name.Trim().ToLower(), It.IsAny<CancellationToken>()), Times.Once);  
    }

    [Fact]
    public async Task DeleteTagById_WhenTagExists_DeletesTag()
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
        _authorizationService.Setup(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Success());

        await _tagService.DeleteTagById(user, tagId, CancellationToken.None);
        
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"), Times.Once);
        _tagRepository.Verify(x => x.DeleteTagAsync(tag, userId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteTagById_WhenTagDoesNotExist_ThrowsNotFoundException()
    {
        int tagId = 1;
        int userId = 10;

        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));

        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);
        
        Func<Task> result = () => _tagService.DeleteTagById(user, tagId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _tagRepository.Verify(x => x.DeleteTagAsync(It.IsAny<Tag>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTagById_WhenUserDoesNotOwnTag_ThrowsForbiddenException()
    {
        int tagId = 1;
        int userId = 10;
        
        Tag tag = new Tag
        {
            Id = tagId,
            Name = "test",
            UserId = 20,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null,
        };
        
        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));
        
        _tagRepository.Setup(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        _authorizationService.Setup(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Failed());
        
        Func<Task> result = () => _tagService.DeleteTagById(user, tagId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ForbiddenException>(result);
        
        _tagRepository.Verify(x => x.GetTagByIdAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"), Times.Once); 
        _tagRepository.Verify(x => x.DeleteTagAsync(It.IsAny<Tag>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CleanUpTags_WhenCalled_CallsRepository()
    {
        await _tagService.CleanUpTags(CancellationToken.None);
        
        _tagRepository.Verify(x => x.CleanUpTagsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task RestoreTagById_WhenTagExists_RestoresTag()
    {
        int tagId = 1;
        int userId = 10;

        Tag tag = new Tag
        {
            Id = tagId,
            Name = "test",
            UserId = userId,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = userId,
        };
        
        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));
        
        _tagRepository.Setup(x => x.GetTagByIdForRestoreAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        _authorizationService.Setup(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Success());
        _tagRepository.Setup(x => x.RestoreTagAsync(tag, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag restoredTag, CancellationToken _) =>
            {
                restoredTag.IsDeleted = false;
                restoredTag.DeletedAt = null;
                restoredTag.DeletedBy = null;
                return restoredTag;
            });
        
        var result = await _tagService.RestoreTagById(user, tagId, CancellationToken.None);
        
        Assert.Equal(tagId, result.Id);
        Assert.Equal("test", result.Name);
        Assert.Equal(userId, result.UserId);
        Assert.False(result.IsDeleted);
        Assert.Null(result.DeletedAt);
        Assert.Null(result.DeletedBy);
        
        _tagRepository.Verify(x => x.GetTagByIdForRestoreAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"), Times.Once);
        _tagRepository.Verify(x => x.RestoreTagAsync(tag, It.IsAny<CancellationToken>()), Times.Once);  
    }
    
    [Fact]
    public async Task RestoreTagById_WhenTagDoesNotExist_ThrowsNotFoundException()
    {
        int tagId = 1;
        int userId = 10;
        
        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));
        
        _tagRepository.Setup(x => x.GetTagByIdForRestoreAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        Func<Task> result = () => _tagService.RestoreTagById(user, tagId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _tagRepository.Verify(x => x.GetTagByIdForRestoreAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _tagRepository.Verify(x => x.RestoreTagAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RestoreTagById_WhenUserDoesNotOwnTag_ThrowsForbiddenException()
    {
        int tagId = 1;
        int userId = 10;

        Tag tag = new Tag
        {
            Id = tagId,
            Name = "test",
            UserId = 20,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = userId,
        };
        
        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));
        
        _tagRepository.Setup(x => x.GetTagByIdForRestoreAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        _authorizationService.Setup(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"))
            .ReturnsAsync(AuthorizationResult.Failed());
        
        Func<Task> result = () => _tagService.RestoreTagById(user, tagId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ForbiddenException>(result);
        
        _tagRepository.Verify(x => x.GetTagByIdForRestoreAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        _authorizationService.Verify(x => x.AuthorizeAsync(user, tag, "TagOwnerOrAdmin"), Times.Once);
        _tagRepository.Verify(x => x.RestoreTagAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}