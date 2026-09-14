using Blog.Application.Abstractions;
using Blog.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;

[ApiController]
[Route("api/admin/posts")]
[Authorize]
public sealed class AdminPostsController(IBlogPostService blogPostService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminPostResponse>>> GetAllPosts(CancellationToken cancellationToken)
        => Ok(await blogPostService.GetAllPostsAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminPostResponse>> GetPost(int id, CancellationToken cancellationToken)
    {
        var post = await blogPostService.GetPostByIdAsync(id, cancellationToken);

        return post is null ? NotFound() : Ok(post);
    }

    [HttpPost]
    public async Task<ActionResult<AdminPostResponse>> CreatePost([FromBody] CreatePostRequest request, CancellationToken cancellationToken)
    {
        var created = await blogPostService.CreatePostAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetPost), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminPostResponse>> UpdatePost(int id, [FromBody] UpdatePostRequest request, CancellationToken cancellationToken)
    {
        var updated = await blogPostService.UpdatePostAsync(id, request, cancellationToken);

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePost(int id, CancellationToken cancellationToken)
        => await blogPostService.DeletePostAsync(id, cancellationToken) ? NoContent() : NotFound();
}