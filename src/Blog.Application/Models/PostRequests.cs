using System.ComponentModel.DataAnnotations;
using Blog.Domain;

namespace Blog.Application.Models;

public class CreatePostRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [Required]
    public string Content { get; init; } = string.Empty;

    public PostStatus Status { get; init; } = PostStatus.Draft;
}

public class UpdatePostRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [Required]
    public string Content { get; init; } = string.Empty;

    public PostStatus Status { get; init; } = PostStatus.Draft;
}