using Newtonsoft.Json;

namespace Conduit.Web.DataAccess.Models;

public class Article
{
    [JsonIgnore] // ignoring this because the document ID is the slug, don't store duplicated data
    public string Slug { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public string Body { get; set; }
    public List<string> TagList { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool Favorited { get; set; }
    public int FavoritesCount { get; set; }
    public string AuthorUsername { get; set; }
}