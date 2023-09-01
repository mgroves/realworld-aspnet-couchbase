using Conduit.Web.Articles.Services;

namespace Conduit.Web.Extensions;

public static class StringExtension
{
    // in this implementation, slug is doing double duty!
    // each slug is of the form "my-title::xyxyxyx"
    // where the "my-title" part is derived from the title
    // and the xyxyxyx part is generated and used as the unique document key
    // SO THAT the title and slug can be mutable WITHOUT having to
    // create a copy of the article AND all the references in favorites documents remain the same
    //
    // ONLY use this extension WITHIN the data access portions of the application
    // OR within tests/test helpers
    public static string GetArticleKey(this string slug)
    {
        if (string.IsNullOrEmpty(slug))
            throw new ArgumentException("Slug must not be empty or null", nameof(slug));

        if (!slug.Contains(SlugService.SLUG_DELIMETER))
            throw new ArgumentException("Slug isn't valid, it must be of the form <title>::<articleKey>", nameof(slug));

        return slug.Split(SlugService.SLUG_DELIMETER)[1];
    }
}