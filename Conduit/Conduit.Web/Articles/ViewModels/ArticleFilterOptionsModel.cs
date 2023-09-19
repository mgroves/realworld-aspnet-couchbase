namespace Conduit.Web.Articles.ViewModels;

public class ArticleFilterOptionsModel
{
    /// <summary>
    /// Filter articles to a given tag. (Optional)
    /// </summary>
    public string? Tag { get; set; }
    /// <summary>
    /// Filter articles to the given author by username (Optional)
    /// </summary>
    public string? Author { get; set; }
    /// <summary>
    /// Filter articles to those favorited by the given username (Optional)
    /// </summary>
    public string? Favorited { get; set; }
    /// <summary>
    /// Limit the number of articles returned (Default of 20, maximum of 50)
    /// </summary>
    public int? Limit { get; set; }
    /// <summary>
    /// Skip a number of articles (Default of 0)
    /// </summary>
    public int? Offset { get; set; }
}