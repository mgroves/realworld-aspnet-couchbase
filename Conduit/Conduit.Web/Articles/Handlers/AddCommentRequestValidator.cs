using FluentValidation;

namespace Conduit.Web.Articles.Handlers;

public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(x => x.Body)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Comment body is required.")
            .MaximumLength(1000).WithMessage("Comment body is limited to maximum 1000 characters.");
    }
}