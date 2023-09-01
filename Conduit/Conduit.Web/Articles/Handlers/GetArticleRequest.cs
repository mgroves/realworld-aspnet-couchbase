using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class GetArticleRequest : IRequest<GetArticleResponse>
{
    public string Slug { get; }
    public string CurrentUser { get; }

    public bool IsUserAuthenticated => !string.IsNullOrEmpty(CurrentUser);

    public GetArticleRequest(string slug, string currentUsername)
    {
        Slug = slug;
        CurrentUser = currentUsername;
    }
}