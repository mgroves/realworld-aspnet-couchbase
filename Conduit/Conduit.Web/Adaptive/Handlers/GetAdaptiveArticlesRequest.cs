using Conduit.Web.Adaptive.ViewModels;
using Conduit.Web.DataAccess.Dto.Articles;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Adaptive.Handlers;

public class GetAdaptiveArticlesRequest : IRequest<GetAdaptiveArticlesResponse>
{
    private readonly string? _username;
    private readonly AdaptiveArticleFilterOptionsModel _filter;
    private readonly IValidator<GetAdaptiveArticlesRequest> _validator;

    public GetAdaptiveArticlesRequest(string? username, AdaptiveArticleFilterOptionsModel filter)
    {
        _username = username;
        _filter = filter;

        Spec = new GetAdaptiveArticlesSpec();
        Spec.Username = username;
        Spec.Limit = filter.Limit;
        Spec.Offset = filter.Offset;
        Spec.Tags = new List<string>();
    }

    public GetAdaptiveArticlesSpec Spec { get; set; }
}