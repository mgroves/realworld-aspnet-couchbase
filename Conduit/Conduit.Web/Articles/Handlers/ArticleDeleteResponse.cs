using FluentValidation.Results;

namespace Conduit.Web.Articles.Handlers;

public class ArticleDeleteResponse
{
    public List<ValidationFailure> ValidationErrors { get; set; }
    public bool ArticleNotFound { get; set; }
    public bool IsUnauthorized { get; set; }
}