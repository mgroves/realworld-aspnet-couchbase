using Conduit.Web.DataAccess.Models;
using FluentValidation.Results;

namespace Conduit.Web.Articles.Handlers;

public class AddCommentResponse
{
    public List<ValidationFailure> ValidationErrors { get; set; }
    public bool IsArticleNotFound { get; set; }
    public Comment Comment { get; set; }
    public bool IsFailed { get; set; }
}