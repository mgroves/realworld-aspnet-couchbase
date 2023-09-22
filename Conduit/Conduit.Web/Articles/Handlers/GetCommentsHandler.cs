using Conduit.Web.Articles.Services;
using Conduit.Web.Articles.ViewModels;
using Conduit.Web.Users.ViewModels;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class GetCommentsHandler : IRequestHandler<GetCommentsRequest, GetCommentsResponse>
{
    private readonly IArticlesDataService _articlesDataService;
    private readonly ICommentsDataService _commentsDataService;

    public GetCommentsHandler(IArticlesDataService articlesDataService, ICommentsDataService commentsDataService)
    {
        _articlesDataService = articlesDataService;
        _commentsDataService = commentsDataService;
    }

    public async Task<GetCommentsResponse> Handle(GetCommentsRequest request, CancellationToken cancellationToken)
    {
        // article must exist
        var doesArticleExist = await _articlesDataService.Exists(request.Slug);
        if (!doesArticleExist)
        {
            return new GetCommentsResponse
            {
                IsArticleNotFound = true
            };
        }

        var results = await _commentsDataService.Get(request.Slug, request.Username);

        return new GetCommentsResponse
        {
            CommentsView = results.DataResult.Select(x => new CommentViewModel
            {
                Id = x.Id,
                Body = x.Body,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.CreatedAt,
                Author = new ProfileViewModel
                {
                    Bio = x.Author.Bio,
                    Following = x.Author.Following,
                    Image = x.Author.Image,
                    Username = x.Author.Username
                }
            }).ToList()
        };
    }
}