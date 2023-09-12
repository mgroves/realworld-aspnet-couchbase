using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class UnfavoriteArticleRequest : IRequest<UnfavoriteArticleResponse>
{
    public string Username { get; set; }
    public string Slug { get; set; }
}