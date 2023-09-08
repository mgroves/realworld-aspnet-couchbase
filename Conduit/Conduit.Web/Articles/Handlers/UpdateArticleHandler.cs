using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Models;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class UpdateArticleHandler : IRequestHandler<UpdateArticleRequest, UpdateArticleResponse>
{
    private readonly IArticlesDataService _articlesDataService;
    private readonly ISlugService _slugService;
    private readonly IValidator<UpdateArticleRequest> _validator;

    public UpdateArticleHandler(IArticlesDataService articlesDataService, ISlugService slugService, IValidator<UpdateArticleRequest> validator)
    {
        _articlesDataService = articlesDataService;
        _slugService = slugService;
        _validator = validator;
    }

    public async Task<UpdateArticleResponse> Handle(UpdateArticleRequest request, CancellationToken cancellationToken)
    {
        // validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new UpdateArticleResponse
            {
                ValidationErrors = validationResult.Errors
            };
        }

        var currentArticleResult = await _articlesDataService.Get(request.Slug);
        if (currentArticleResult.Status == DataResultStatus.NotFound)
            return new UpdateArticleResponse { IsNotFound = true };

        // TODO: check to see if current user matches author, if NOT, return unauthorized

        var currentArticle = currentArticleResult.DataResult;

        var updatedArticle = new Article();
        if (!string.IsNullOrEmpty(request.Title))
        {
            if (currentArticle.Title != request.Title)
            {
                updatedArticle.Title = request.Title.Trim();
                updatedArticle.Slug = _slugService.NewTitleSameSlug(request.Title, currentArticle.Slug);
            }
        }

        updatedArticle.Body = request.Body.Trim();
        updatedArticle.Description = request.Description.Trim();
        updatedArticle.TagList = request.Tags;

        await _articlesDataService.UpdateArticle(updatedArticle);

        return new UpdateArticleResponse();
    }
}