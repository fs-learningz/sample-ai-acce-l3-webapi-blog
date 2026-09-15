using BlogApi.Application.Dtos;

namespace BlogApi.Application.Interfaces;

public interface IPostService
{
    IReadOnlyList<PostDto> GetPublishedPosts();
    PostDto? GetPostById(Guid id);
    IReadOnlyList<PostDto> GetAllPosts();
    IReadOnlyList<PostDto> GetPostsByAuthor(Guid authorId);
    PostDto CreatePost(CreatePostRequest request, Guid authorId, string authorUsername);
    PostDto? UpdatePost(Guid id, UpdatePostRequest request);
    bool DeletePost(Guid id);
    PostDto? SetPublished(Guid id, bool isPublished);
}
