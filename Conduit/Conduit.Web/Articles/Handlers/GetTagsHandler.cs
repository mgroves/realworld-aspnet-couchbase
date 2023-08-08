using Conduit.Web.Articles.Services;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class GetTagsHandler : IRequestHandler<GetTagsRequest, GetTagsResult>
{
    private readonly ITagsDataService _tagsDataService;

    public GetTagsHandler(ITagsDataService tagsDataService)
    {
        _tagsDataService = tagsDataService;
    }

    public async Task<GetTagsResult> Handle(GetTagsRequest request, CancellationToken cancellationToken)
    {
        var result = await _tagsDataService.GetAllTags();

        return new GetTagsResult
        {
            Tags = result
        };
    }
}