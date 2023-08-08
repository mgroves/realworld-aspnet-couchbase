using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class GetTagsRequest : IRequest<GetTagsResult>
{
    
}

public class GetTagsResult
{
    public List<string> Tags { get; set; }
}