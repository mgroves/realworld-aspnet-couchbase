using FluentValidation.Results;

namespace Conduit.Web.Articles.Handlers;

public class FavoriteArticleResponse
{
    public List<ValidationFailure> ValidationErrors { get; set; }
}