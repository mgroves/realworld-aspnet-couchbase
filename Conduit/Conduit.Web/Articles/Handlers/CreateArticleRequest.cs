using Conduit.Web.Articles.ViewModels;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class CreateArticleRequest : IRequest<CreateArticleResponse>
{
    public CreateArticleSubmitModel ArticleSubmission { get; }
    public string AuthorUsername { get; }

    public CreateArticleRequest(CreateArticleSubmitModel articleSubmission, string authorUsername)
    {
        ArticleSubmission = articleSubmission;
        AuthorUsername = authorUsername;
    }
}