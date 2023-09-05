using Conduit.Web.Extensions;
using Slugify;

namespace Conduit.Web.Articles.Services;

public interface ISlugService
{
    string GenerateSlug(string title);
    string NewTitleSameSlug(string newTitle, string currentSlug);
}

public class SlugService : ISlugService
{
    public const string SLUG_DELIMETER = "::";

    private readonly ISlugHelper _slugHelper;
    private readonly Random _random;

    public SlugService(ISlugHelper slugHelper)
    {
        _slugHelper = slugHelper;
        _random = new Random();
    }

    /// <summary>
    /// Create a slug of the form "foo-bar-baz-[random string]"
    /// This method does NOT check the length or truncate anything
    /// </summary>
    /// <param name="title">Title like "foo bar baz"</param>
    /// <returns>Slugified string</returns>
    public string GenerateSlug(string title)
    {
        var slug = _slugHelper.GenerateSlug(title);

        slug += SLUG_DELIMETER + _random.String(12);

        return slug;
    }

    public string NewTitleSameSlug(string newTitle, string currentSlug)
    {
        var slug = _slugHelper.GenerateSlug(newTitle);

        var currentArticleKey = currentSlug.GetArticleKey();

        return $"{slug}{SLUG_DELIMETER}{currentArticleKey}";
    }
}