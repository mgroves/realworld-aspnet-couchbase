using FluentValidation;

namespace Conduit.Web.Articles.Handlers;

public class GetArticlesRequestValidator : AbstractValidator<GetArticlesRequest>
{
    public GetArticlesRequestValidator()
    {
        RuleFor(x => x.Limit)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).When(l => l.Limit.HasValue)
            .WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(50).When(l => l.Limit.HasValue)
            .WithMessage("Limit must be less than or equal to 50.");

        RuleFor(x => x.Offset)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0).When(o => o.Offset.HasValue)
            .WithMessage("Offset must be greater than or equal to 0.");
    }
}