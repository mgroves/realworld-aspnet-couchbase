using Conduit.Web.Articles.ViewModels;
using Conduit.Web.Extensions;

namespace Conduit.Tests.TestHelpers.Dto;

public static class CreateArticleSubmitModelHelper
{
    public static CreateArticleSubmitModel Create(
        string? title = null,
        string? description = null,
        string? body = null,
        List<string>? tags = null)
    {
        var random = new Random();
        title ??= $"valid title {random.String(15)}";
        description ??= $"valid description {random.String(64)}";
        body ??= $"valid body {random.String(256)}";
        tags ??= new List<string> { "baseball", "Couchbase" };

        var model = new CreateArticleSubmitModel();
        model.Article = new CreateArticleSubmitModelArticle
        {
            Title = title,
            Description = description,
            Body = body,
            Tags = tags
        };
        return model;
    }
}