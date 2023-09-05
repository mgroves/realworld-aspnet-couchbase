using Conduit.Web.Articles.ViewModels;
using Conduit.Web.Extensions;

namespace Conduit.Tests.TestHelpers.Dto;

public static class UpdateArticlePostModelHelper
{
    public static UpdateArticlePostModelArticle Create(
        string? title = null,
        string? body = null,
        string? description = null,
        List<string>? tags = null)
    {
        var random = new Random();

        title ??= random.String(20);
        body ??= random.String(2048);
        description ??= random.String(100);
        tags ??= new List<string> { "Couchbase", "cruising" };

        var wrapper = new UpdateArticlePostModelArticle
        {
            Article = new UpdateArticlePostModel
            {
                Title = title,
                Body = body,
                Description = description,
                Tags = tags
            }
        };
        return wrapper;
    }
}