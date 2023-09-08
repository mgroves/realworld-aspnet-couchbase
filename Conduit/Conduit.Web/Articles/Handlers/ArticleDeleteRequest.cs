using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class ArticleDeleteRequest : IRequest<ArticleDeleteResponse>
{
    public string Slug { get; }
    public string Username { get; set; }

    public ArticleDeleteRequest(string slug, string username)
    {
        Slug = slug;
        Username = username;
    }
}