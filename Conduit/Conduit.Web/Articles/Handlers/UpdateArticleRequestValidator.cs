using Conduit.Web.Articles.Services;
using FluentValidation;

namespace Conduit.Web.Articles.Handlers;

public class UpdateArticleRequestValidator : AbstractValidator<UpdateArticleRequest>
{
    private readonly ITagsDataService _tagsDataService;

    public UpdateArticleRequestValidator(ITagsDataService tagsDataService)
    {
        _tagsDataService = tagsDataService;

        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEqual("").WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must be 100 characters or less.")
            .MinimumLength(10).WithMessage("Title must be at least 10 characters.")
            .Must(NotHaveSlugDelimeter).When(x => x.Title != null).WithMessage("Double colon (::) is not allowed in titles. Sorry, it's weird.");

        RuleFor(x => x.Description)
            .Cascade(CascadeMode.Stop)
            .NotEqual("").WithMessage("Description is required.")
            .MaximumLength(200).WithMessage("Description must be 200 characters or less.")
            .MinimumLength(10).WithMessage("Description must be 10 characters or more.");

        RuleFor(x => x.Body)
            .Cascade(CascadeMode.Stop)
            .NotEqual("").WithMessage("Body is required.")
            .MaximumLength(15000000).WithMessage("Body must be 15,000,000 characters or less.")
            .MinimumLength(10).WithMessage("Body must be 10 characters or more.");

        RuleFor(x => x.Tags)
            .Cascade(CascadeMode.Stop)
            .MustAsync(BeAllowedTags).WithMessage("At least one of those tags isn't allowed.");

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(HaveAtLeastOneValueSpecified).WithMessage("Update Failed: Please make sure to modify at least one of the following fields: Body, Title, Description, or Tags.");
    }

    private bool HaveAtLeastOneValueSpecified(UpdateArticleRequest request)
    {
        return request.Body != null
               || request.Description != null
               || request.Title != null
               || request.Tags != null;
    }

    private async Task<bool> BeAllowedTags(List<string>? submittedTags, CancellationToken arg2)
    {
        if (submittedTags == null || !submittedTags.Any())
            return true;

        var allAllowedTags = await _tagsDataService.GetAllTags();

        return submittedTags.All(tag => allAllowedTags.Contains(tag));
    }

    private bool NotHaveSlugDelimeter(string title)
    {
        return !title.Contains(SlugService.SLUG_DELIMETER);
    }
}