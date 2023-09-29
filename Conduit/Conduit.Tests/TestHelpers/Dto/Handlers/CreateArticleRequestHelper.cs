using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.ViewModels;
using Conduit.Web.Extensions;

namespace Conduit.Tests.TestHelpers.Dto.Handlers;

public static class CreateArticleRequestHelper
{
    /// <summary>
    /// Create a well-formed Article object in memory.
    /// You can specify values with the optional parameters
    /// </summary>
    /// <param name="makeTagsNull">If set to true, the tags will be set to null</param>
    /// <returns>Article object</returns>
    public static CreateArticleRequest Create(
        string? body = null,
        string? description = null,
        string? title = null,
        List<string>? tags = null,
        string? username = null,
        bool makeTagsNull = false)
    {
        var random = new Random();

        body ??= random.String(1000);
        description ??= random.String(100);
        title ??= random.String(60);
        tags ??= new List<string> { "Couchbase", "cruising" };
        username ??= Path.GetRandomFileName();
        if (makeTagsNull) tags = null;


        var article = new CreateArticleSubmitModel
        {
            Article = new CreateArticleSubmitModelArticle
            {
                Body = body,
                Description = description,
                Title = title,
                TagList = tags
            }
        };


        return new CreateArticleRequest(article, username);
    }
}