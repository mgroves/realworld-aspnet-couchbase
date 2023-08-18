using Conduit.Web.Extensions;
using Slugify;

namespace Conduit.Web.Articles.Services;

public interface ISlugService
{
    Task<string> GenerateSlug(string title);
}

public class SlugService : ISlugService
{
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
    public async Task<string> GenerateSlug(string title)
    {
        var slug = _slugHelper.GenerateSlug(title);

        slug += "-" + _random.String(12);

        return slug;
    }
}