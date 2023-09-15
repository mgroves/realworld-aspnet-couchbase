using Conduit.Web.Articles.ViewModels;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class GetArticlesRequest : IRequest<GetArticlesResponse>
{
    public string? Tag { get; set; }
    public int? Offset { get; set; }
    public int? Limit { get; set; }
    public string? FavoritedByUsername { get; set; }
    public string? AuthorUsername { get; set; }
    public string? Username { get; set; }

    public GetArticlesRequest(string? username, ArticleFilterOptionsModel filter)
    {
        Username = username;
        AuthorUsername = filter.Author;
        FavoritedByUsername = filter.Favorited;
        Limit = filter.Limit;
        Offset = filter.Offset;
        Tag = filter.Tag;
    }
}