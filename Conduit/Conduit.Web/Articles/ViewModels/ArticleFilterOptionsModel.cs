namespace Conduit.Web.Articles.ViewModels;

public class ArticleFilterOptionsModel
{
    public string? Tag { get; set; }
    public string? Author { get; set; }
    public string? Favorited { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}