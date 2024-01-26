using Conduit.Web.Adaptive.Services;
using Conduit.Web.Articles.Services;
using MediatR;

namespace Conduit.Web.Adaptive.Handlers;

public class GenerateSummaryHandler : IRequestHandler<GenerateSummaryRequest, GenerateSummaryResponse>
{
    private readonly IGenerativeAiService _genAiService;
    private readonly IArticlesDataService _articlesDataService;
    private readonly ISlugService _slugService;

    public GenerateSummaryHandler(IGenerativeAiService genAiService, IArticlesDataService articlesDataService, ISlugService slugService)
    {
        _genAiService = genAiService;
        _articlesDataService = articlesDataService;
        _slugService = slugService;
    }

    public async Task<GenerateSummaryResponse> Handle(GenerateSummaryRequest request, CancellationToken cancellationToken)
    {
        var article = await _genAiService.GenerateSummaryArticle(request.RawData, request.Tag);

        // make sure article is kosher for posting
        article.AuthorUsername = "summary-bot";
        article.TagList.Add("ai-generated");
        article.CreatedAt = DateTimeOffset.Now;
        article.Slug = _slugService.GenerateSlug(article.Title);
        article.FavoritesCount = 0;

        await _articlesDataService.Create(article);

        return new GenerateSummaryResponse
        {
            Slug = article.Slug
        };
    }
}