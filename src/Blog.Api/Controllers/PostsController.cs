using Blog.Application.Abstractions;
using Blog.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;

[ApiController]
[Route("api/posts")]
public sealed class PostsController(IBlogPostService blogPostService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PostSummaryResponse>>> GetPublishedPosts(CancellationToken cancellationToken)
        => Ok(await blogPostService.GetPublishedPostsAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDetailResponse>> GetPublishedPost(int id, CancellationToken cancellationToken)
    {
        var post = await blogPostService.GetPublishedPostAsync(id, cancellationToken);

        return post is null ? NotFound() : Ok(post);
    }
}