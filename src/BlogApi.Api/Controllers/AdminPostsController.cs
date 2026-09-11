using BlogApi.Application.Dtos;
using BlogApi.Application.Interfaces;
using BlogApi.Api.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Api.Controllers;

[ApiController]
[Route("api/admin/posts")]
[RequireAdmin]
public class AdminPostsController(IPostService postService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<PostDto>> GetPosts() => Ok(postService.GetAllPosts());

    [HttpGet("{id:guid}")]
    public ActionResult<PostDto> GetPost(Guid id)
    {
        var post = postService.GetPostById(id);
        return post is null ? NotFound() : Ok(post);
    }

    [HttpPost]
    public ActionResult<PostDto> CreatePost(CreatePostRequest request)
    {
        var created = postService.CreatePost(request);
        return CreatedAtAction(nameof(GetPost), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<PostDto> UpdatePost(Guid id, UpdatePostRequest request)
    {
        var updated = postService.UpdatePost(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeletePost(Guid id) =>
        postService.DeletePost(id) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/publish")]
    public ActionResult<PostDto> PublishPost(Guid id)
    {
        var published = postService.SetPublished(id, true);
        return published is null ? NotFound() : Ok(published);
    }

    [HttpPost("{id:guid}/unpublish")]
    public ActionResult<PostDto> UnpublishPost(Guid id)
    {
        var unpublished = postService.SetPublished(id, false);
        return unpublished is null ? NotFound() : Ok(unpublished);
    }
}
