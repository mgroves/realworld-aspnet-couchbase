using Conduit.Web.Adaptive.Services;
using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.Users.Services;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Adaptive.Handlers;

public class GetAdaptiveArticlesHandler : IRequestHandler<GetAdaptiveArticlesRequest, GetAdaptiveArticlesResponse>
{
    private readonly IValidator<GetAdaptiveArticlesRequest> _validator;
    private readonly IAdaptiveDataService _adaptiveDataService;
    private readonly IUserDataService _userDataService;

    public GetAdaptiveArticlesHandler(IValidator<GetAdaptiveArticlesRequest> validator, IAdaptiveDataService adaptiveDataService, IUserDataService userDataService)
    {
        _validator = validator;
        _adaptiveDataService = adaptiveDataService;
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
        spec.Tags = await _adaptiveDataService.GetAdaptiveProfileTags(request.Spec.Username);

        var result = await _adaptiveDataService.GetAdaptiveArticles(spec);
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