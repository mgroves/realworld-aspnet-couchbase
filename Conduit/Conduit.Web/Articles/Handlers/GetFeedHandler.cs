using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Dto;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class GetFeedHandler : IRequestHandler<GetFeedRequest, GetFeedResponse>
{
    private readonly IArticlesDataService _articlesDataService;
    private readonly IValidator<GetFeedRequest> _validator;

    public GetFeedHandler(IArticlesDataService articlesDataService, IValidator<GetFeedRequest> validator)
    {
        _articlesDataService = articlesDataService;
        _validator = validator;
    }

    public async Task<GetFeedResponse> Handle(GetFeedRequest request, CancellationToken cancellationToken)
    {
        // validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new GetFeedResponse
            {
                ValidationErrors = validationResult.Errors
            };
        }

        // create spec for request
        var spec = request.Spec;
        spec.Username = spec.Username.Trim();
        spec.FollowedByUsername = spec.FollowedByUsername.Trim();

        var result = await _articlesDataService.GetArticles(spec);
        if (result.Status != DataResultStatus.Ok)
        {
            return new GetFeedResponse
            {
                IsFailure = true
            };
        }

        return new GetFeedResponse
        {
            ArticlesView = result.DataResult,
        };
    }
}