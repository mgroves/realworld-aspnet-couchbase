using Conduit.Web.DataAccess.Models;
using Conduit.Web.Users.ViewModels;

namespace Conduit.Web.Articles.ViewModels;

public class ArticleViewModel
{
    public string Slug { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Body { get; set; }
    public List<string> TagList { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool Favorited { get; set; }
    public int FavoritesCount { get; set; }

    public ProfileViewModel Author { get; set; }
}