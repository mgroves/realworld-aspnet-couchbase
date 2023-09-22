using Conduit.Web.Articles.ViewModels;

namespace Conduit.Web.Articles.Handlers;

public class GetCommentsResponse
{
    public bool IsArticleNotFound { get; set; }
    public bool IsFailed { get; set; }
    public List<CommentViewModel> CommentsView { get; set; }
}