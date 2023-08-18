using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Models;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class CreateArticleHandler : IRequestHandler<CreateArticleRequest, CreateArticleResponse>
{
    private readonly IValidator<CreateArticleRequest> _validator;
    private readonly IArticlesDataService _articleDataService;
    private readonly ISlugService _slugService;

    public CreateArticleHandler(IValidator<CreateArticleRequest> validator, IArticlesDataService articleDataService, ISlugService slugService)
    {
        _validator = validator;
        _articleDataService = articleDataService;
        _slugService = slugService;
    }

    public async Task<CreateArticleResponse> Handle(CreateArticleRequest request, CancellationToken cancellationToken)
    {
        // validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new CreateArticleResponse
            {
                ValidationErrors = validationResult.Errors
            };
        }

        var articleToInsert = new Article
        {
            Title = request.ArticleSubmission.Title.Trim(),
            Description = request.ArticleSubmission.Description.Trim(),
            Body = request.ArticleSubmission.Body.Trim(),
            Slug = await _slugService.GenerateSlug(request.ArticleSubmission.Title.Trim()),
            TagList = request.ArticleSubmission.Tags,
            CreatedAt = new DateTimeOffset(DateTime.Now),
            Favorited = false, // brand new article, no one can have favorited it yet
            FavoritesCount = 0, // brand new article, no one can have favorited it yet
            AuthorUsername = request.AuthorUsername
        };

        await _articleDataService.Create(articleToInsert);

        var response = new CreateArticleResponse();
        response.Article = articleToInsert;
        response.Article.Slug = articleToInsert.Slug;

        return response;
    }
}