namespace Conduit.Web.DataAccess.Dto.Articles;

public class GetArticlesSpec
{
    public string? Tag { get; set; }
    public int? Offset { get; set; }
    public int? Limit { get; set; }
    public string? FavoritedByUsername { get; set; }
    public string? AuthorUsername { get; set; }
    public string? Username { get; set; }
    public string? FollowedByUsername { get; set; }
}