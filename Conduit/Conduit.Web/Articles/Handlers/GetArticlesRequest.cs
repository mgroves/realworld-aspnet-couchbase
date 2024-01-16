using Conduit.Web.Articles.ViewModels;
using Conduit.Web.DataAccess.Dto.Articles;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class GetArticlesRequest : IRequest<GetArticlesResponse>
{
    public GetArticlesSpec Spec { get; set; }

    public GetArticlesRequest(string? username, ArticleFilterOptionsModel filter)
    {
        Spec = new GetArticlesSpec();
        Spec.Username = username;
        Spec.AuthorUsername = filter.Author;
        Spec.FavoritedByUsername = filter.Favorited;
        Spec.Limit = filter.Limit;
        Spec.Offset = filter.Offset;
        Spec.Tag = filter.Tag;
    }
}
