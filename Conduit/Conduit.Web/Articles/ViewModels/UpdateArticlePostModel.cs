namespace Conduit.Web.Articles.ViewModels;

public class UpdateArticlePostModelArticle
{
    public UpdateArticlePostModel Article { get; set; }
}

/// <summary>
/// At least one of Title, Description, Body, and Tags must be specified
/// </summary>
public class UpdateArticlePostModel
{
    /// <summary>
    /// Article title, optional. Must be between 10-100 characters
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Article description, optional. Must be between 10-200 characters
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Article body, optional. Must be between 10 and 15,000,000 characters
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Tags, optional. Any tags specified must be from the allowed list of tags.
    /// </summary>
    public List<string>? Tags { get; set; }
}