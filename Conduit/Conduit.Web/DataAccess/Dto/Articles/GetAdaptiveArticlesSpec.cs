namespace Conduit.Web.DataAccess.Dto.Articles;

public class GetAdaptiveArticlesSpec
{
    public int? Offset { get; set; }
    public int? Limit { get; set; }
    public string? Username { get; set; }
    public List<string>? Tags { get; set; }
}