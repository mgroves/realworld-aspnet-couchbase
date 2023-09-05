using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class FavoriteArticleRequest : IRequest<FavoriteArticleResponse>
{
    public string Username { get; set; }
    public string Slug { get; set; }
}