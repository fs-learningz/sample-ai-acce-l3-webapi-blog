using BlogApi.Application.Dtos;
using BlogApi.Application.Interfaces;
using BlogApi.Api.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Api.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController(IPostService postService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<PostDto>> GetPosts() => Ok(postService.GetPublishedPosts());

    [HttpGet("{id:guid}")]
    public ActionResult<PostDto> GetPost(Guid id)
    {
        var post = postService.GetPostById(id);
        if (post is null || !post.IsPublished)
        {
            return NotFound();
        }

        return Ok(post);
    }
}
