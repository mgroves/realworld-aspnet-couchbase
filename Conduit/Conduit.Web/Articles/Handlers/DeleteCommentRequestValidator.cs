using FluentValidation;

namespace Conduit.Web.Articles.Handlers;

public class DeleteCommentRequestValidator : AbstractValidator<DeleteCommentRequest>
{
    public DeleteCommentRequestValidator()
    {
        RuleFor(x => x.Slug)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Slug is required.");

        RuleFor(x => x.CommentId)
            .Must(v => v > 0).WithMessage("Must be a valid CommentID");
    }
}