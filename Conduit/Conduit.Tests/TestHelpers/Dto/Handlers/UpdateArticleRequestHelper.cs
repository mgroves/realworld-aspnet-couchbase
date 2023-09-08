using Conduit.Tests.TestHelpers.Dto.ViewModels;
using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.ViewModels;
using Conduit.Web.Extensions;

namespace Conduit.Tests.TestHelpers.Dto.Handlers;

public static class UpdateArticleRequestHelper
{
    public static UpdateArticleRequest Create(
        UpdateArticlePostModelArticle? model = null,
        string? slug = null,
        string? currentUsername = null)
    {
        var random = new Random();

        slug ??= $"this-is-a-valid-slug::{random.String(10)}";
        model ??= UpdateArticlePostModelHelper.Create();
        currentUsername ??= $"username-{random.String(10)}";
        var request = new UpdateArticleRequest(model, slug, currentUsername);
        return request;
    }
}