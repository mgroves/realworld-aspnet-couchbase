using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.ViewModels;
using Conduit.Web.Extensions;

namespace Conduit.Tests.TestHelpers.Dto;

public static class UpdateArticleRequestHelper
{
    public static UpdateArticleRequest Create(
        UpdateArticlePostModelArticle? model = null,
        string? slug = null)
    {
        var random = new Random();

        slug ??= $"this-is-a-valid-slug::{random.String(10)}";
        model ??= UpdateArticlePostModelHelper.Create();
        var request = new UpdateArticleRequest(model, slug);
        return request;
    }
}