using Conduit.Web.Articles.ViewModels;
using Conduit.Web.DataAccess.Dto.Articles;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class GetFeedRequest : IRequest<GetFeedResponse>
{
    public GetArticlesSpec Spec { get; set; }

    public GetFeedRequest(string username, ArticleFeedOptionsModel filter)
    {
        Spec = new GetArticlesSpec();
        Spec.Username = username;
        Spec.FollowedByUsername = username;
        Spec.Limit = filter.Limit;
        Spec.Offset = filter.Offset;
    }
}