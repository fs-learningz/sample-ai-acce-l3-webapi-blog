using BlogApi.Api.Auth;
using BlogApi.Application.Dtos;
using BlogApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Api.Controllers;

[ApiController]
[Route("api/admin/posts")]
[Authorize]
public class AdminPostsController(IPostService postService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<PostDto>> GetPosts() =>
        Ok(User.IsAdmin() ? postService.GetAllPosts() : postService.GetPostsByAuthor(User.GetUserId()));

    [HttpGet("{id:guid}")]
    public ActionResult<PostDto> GetPost(Guid id)
    {
        var post = postService.GetPostById(id);
        if (post is null)
        {
            return NotFound();
        }

        if (!CanManage(post))
        {
            return Forbid();
        }

        return Ok(post);
    }

    [HttpPost]
    public ActionResult<PostDto> CreatePost(CreatePostRequest request)
    {
        var created = postService.CreatePost(request, User.GetUserId());
        return CreatedAtAction(nameof(GetPost), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<PostDto> UpdatePost(Guid id, UpdatePostRequest request)
    {
        var existing = postService.GetPostById(id);
        if (existing is null)
        {
            return NotFound();
        }

        if (!CanManage(existing))
        {
            return Forbid();
        }

        var updated = postService.UpdatePost(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeletePost(Guid id)
    {
        var existing = postService.GetPostById(id);
        if (existing is null)
        {
            return NotFound();
        }

        if (!CanManage(existing))
        {
            return Forbid();
        }

        return postService.DeletePost(id) ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/publish")]
    public ActionResult<PostDto> PublishPost(Guid id) => SetPublished(id, true);

    [HttpPost("{id:guid}/unpublish")]
    public ActionResult<PostDto> UnpublishPost(Guid id) => SetPublished(id, false);

    private ActionResult<PostDto> SetPublished(Guid id, bool isPublished)
    {
        var existing = postService.GetPostById(id);
        if (existing is null)
        {
            return NotFound();
        }

        if (!CanManage(existing))
        {
            return Forbid();
        }

        var updated = postService.SetPublished(id, isPublished);
        return updated is null ? NotFound() : Ok(updated);
    }

    private bool CanManage(PostDto post) => User.IsAdmin() || post.AuthorId == User.GetUserId();
}
