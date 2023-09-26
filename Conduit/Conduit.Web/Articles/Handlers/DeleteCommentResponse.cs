using FluentValidation.Results;

namespace Conduit.Web.Articles.Handlers;

public class DeleteCommentResponse
{
    public List<ValidationFailure> ValidationErrors { get; set; }
    public bool IsArticleNotFound { get; set; }
    public bool IsNotAuthorized { get; set; }
    public bool IsFailed { get; set; }
    public bool IsCommentNotFound { get; set; }
}