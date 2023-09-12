using FluentValidation.Results;

namespace Conduit.Web.Articles.Handlers;

public class UnfavoriteArticleResponse
{
    public List<ValidationFailure> ValidationErrors { get; set; }
}