using BlogApi.Api.Auth;
using BlogApi.Application.Dtos;
using BlogApi.Application.Interfaces;
using BlogApi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Api.Controllers;

[ApiController]
[Route("api/v1/admin/posts")]
[Authorize(Policy = "Author")]
public class AdminPostsController(IPostService postService) : ControllerBase
{
    private bool IsAdmin => User.IsInRole(RoleNames.Admin);

    private Guid CurrentUserId => Guid.Parse(User.FindFirst(JwtClaimTypes.Subject)!.Value);

    [HttpGet]
    public ActionResult<IReadOnlyList<PostDto>> GetPosts() =>
        Ok(IsAdmin ? postService.GetAllPosts() : postService.GetPostsByAuthor(CurrentUserId));

    [HttpGet("{id:guid}")]
    public ActionResult<PostDto> GetPost(Guid id)
    {
        var post = postService.GetPostById(id);
        return CanAccess(post) ? Ok(post) : NotFound();
    }

    [HttpPost]
    public ActionResult<PostDto> CreatePost(CreatePostRequest request)
    {
        var username = User.FindFirst(JwtClaimTypes.Name)!.Value;
        var created = postService.CreatePost(request, CurrentUserId, username);
        return CreatedAtAction(nameof(GetPost), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<PostDto> UpdatePost(Guid id, UpdatePostRequest request)
    {
        if (!CanAccess(postService.GetPostById(id)))
        {
            return NotFound();
        }

        var updated = postService.UpdatePost(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeletePost(Guid id)
    {
        var post = postService.GetPostById(id);
        if (!CanAccess(post))
        {
            return NotFound();
        }

        if (post!.IsPublished)
        {
            return Conflict(new { message = "Unpublish this post before deleting it." });
        }

        return postService.DeletePost(id) ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/publish")]
    public ActionResult<PostDto> PublishPost(Guid id)
    {
        if (!CanAccess(postService.GetPostById(id)))
        {
            return NotFound();
        }

        var published = postService.SetPublished(id, true);
        return published is null ? NotFound() : Ok(published);
    }

    [HttpPost("{id:guid}/unpublish")]
    public ActionResult<PostDto> UnpublishPost(Guid id)
    {
        if (!CanAccess(postService.GetPostById(id)))
        {
            return NotFound();
        }

        var unpublished = postService.SetPublished(id, false);
        return unpublished is null ? NotFound() : Ok(unpublished);
    }

    private bool CanAccess(PostDto? post) =>
        post is not null && (IsAdmin || post.AuthorId == CurrentUserId);
}
