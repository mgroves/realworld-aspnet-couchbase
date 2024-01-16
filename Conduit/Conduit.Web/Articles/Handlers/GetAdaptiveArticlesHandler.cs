using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.Users.Services;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class GetAdaptiveArticlesHandler : IRequestHandler<GetAdaptiveArticlesRequest, GetAdaptiveArticlesResponse>
{
    private readonly IValidator<GetAdaptiveArticlesRequest> _validator;
    private readonly IArticlesDataService _articlesDataService;
    private readonly IUserDataService _userDataService;

    public GetAdaptiveArticlesHandler(IValidator<GetAdaptiveArticlesRequest> validator, IArticlesDataService articlesDataService, IUserDataService userDataService)
    {
        _validator = validator;
        _articlesDataService = articlesDataService;
        _userDataService = userDataService;
    }

    public async Task<GetAdaptiveArticlesResponse> Handle(GetAdaptiveArticlesRequest request, CancellationToken cancellationToken)
    {
        // validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new GetAdaptiveArticlesResponse
            {
                ValidationErrors = validationResult.Errors
            };
        }

        // create spec for request
        var spec = request.Spec;
        spec.Tags = await _userDataService.GetAdaptiveProfileTags(request.Spec.Username); // TODO: get tags from adaptive tags in profile

        var result = await _articlesDataService.GetAdaptiveArticles(spec);
        if (result.Status != DataResultStatus.Ok)
        {
            return new GetAdaptiveArticlesResponse
            {
                IsFailure = true
            };
        }

        return new GetAdaptiveArticlesResponse
        {
            ArticlesView = result.DataResult
        };
    }
}