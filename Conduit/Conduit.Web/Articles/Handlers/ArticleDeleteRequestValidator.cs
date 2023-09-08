using FluentValidation;

namespace Conduit.Web.Articles.Handlers;

public class ArticleDeleteRequestValidator : AbstractValidator<ArticleDeleteRequest>
{
    public ArticleDeleteRequestValidator()
    {
        RuleFor(x => x.Slug)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Article slug is required.");

        RuleFor(x => x.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Author username is required.");
    }
}