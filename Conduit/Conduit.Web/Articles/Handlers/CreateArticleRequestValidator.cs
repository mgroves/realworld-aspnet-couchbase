using Conduit.Web.Articles.Services;
using FluentValidation;

namespace Conduit.Web.Articles.Handlers;

public class CreateArticleRequestValidator : AbstractValidator<CreateArticleRequest>
{
    private readonly ITagsDataService _tagsDataService;

    public CreateArticleRequestValidator(ITagsDataService tagsDataService)
    {
        _tagsDataService = tagsDataService;

        RuleFor(x => x.ArticleSubmission.Article.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must be 100 characters or less.")
            .MinimumLength(10).WithMessage("Title must be at least 10 characters.")
            .Must(NotHaveSlugDelimeter).WithMessage("Double colon (::) is not allowed in titles. Sorry, it's weird.");

        RuleFor(x => x.ArticleSubmission.Article.Description)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(200).WithMessage("Description must be 200 characters or less.")
            .MinimumLength(10).WithMessage("Description must be 10 characters or more.");

        RuleFor(x => x.ArticleSubmission.Article.Body)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Body is required.")
            .MaximumLength(15000000).WithMessage("Body must be 15,000,000 characters or less.")
            .MinimumLength(10).WithMessage("Body must be 10 characters or more.");

        RuleFor(x => x.ArticleSubmission.Article.Tags)
            .Cascade(CascadeMode.Stop)
            .MustAsync(BeAllowedTags).WithMessage("At least one of those tags isn't allowed.");
    }

    private bool NotHaveSlugDelimeter(string title)
    {
        return !title.Contains(SlugService.SLUG_DELIMETER);
    }

    private async Task<bool> BeAllowedTags(List<string>? submittedTags, CancellationToken arg2)
    {
        if (submittedTags == null || !submittedTags.Any())
            return true;

        var allAllowedTags = await _tagsDataService.GetAllTags();

        return submittedTags.All(tag => allAllowedTags.Contains(tag));
    }
}