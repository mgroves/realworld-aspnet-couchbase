using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Dto;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class GetArticlesHandler : IRequestHandler<GetArticlesRequest, GetArticlesResponse>
{
    private readonly IValidator<GetArticlesRequest> _validator;
    private readonly IArticlesDataService _articlesDataService;

    public GetArticlesHandler(IValidator<GetArticlesRequest> validator, IArticlesDataService articlesDataService)
    {
        _validator = validator;
        _articlesDataService = articlesDataService;
    }

    public async Task<GetArticlesResponse> Handle(GetArticlesRequest request, CancellationToken cancellationToken)
    {
        // validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new GetArticlesResponse
            {
                ValidationErrors = validationResult.Errors
            };
        }

        // clean up the request
        request.Username = request.Username?.Trim();
        request.AuthorUsername = request.AuthorUsername?.Trim();
        request.FavoritedByUsername = request.FavoritedByUsername?.Trim();
        request.Tag = request.Tag?.Trim();

        var result = await _articlesDataService.GetArticles(request);
        if (result.Status != DataResultStatus.Ok)
        {
            return new GetArticlesResponse
            {
                IsFailure = true
            };
        }

        return new GetArticlesResponse
        {
            ArticlesView = result.DataResult
        };
    }
}