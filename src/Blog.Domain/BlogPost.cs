namespace Blog.Domain;

public class BlogPost
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public PostStatus Status { get; set; } = PostStatus.Draft;

    public string Author { get; set; } = "admin";

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public DateTime? PublishedAtUtc { get; set; }
}
