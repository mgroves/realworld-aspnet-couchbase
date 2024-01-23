using Conduit.Web.Adaptive.Handlers;
using Conduit.Web.Adaptive.Services;
using Conduit.Web.Adaptive.ViewModels;
using Conduit.Web.Extensions;
using Conduit.Web.Users.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Web.Adaptive.Controllers;

[ApiController]
[Authorize]
public class AdaptiveController : Controller
{
    private readonly IMediator _mediator;
    private readonly IAuthService _authService;
    private readonly IDemoColumnarData _demoColumnarData;

    public AdaptiveController(IMediator mediator, IAuthService authService, IDemoColumnarData demoColumnarData)
    {
        _mediator = mediator;
        _authService = authService;
        _demoColumnarData = demoColumnarData;
    }

    [HttpPost]
    [Route("/api/articles/generateSummary")]
    public async Task<IActionResult> GenerateSummary([FromBody] GenerateSummaryInput input)
    {
        // TODO: these results should come from a Columnar query/view and/or dataset manipulated by Tableau reporting UI
        // var generateSummaryRequest = new GenerateSummaryRequest();
        // generateSummaryRequest.RawData = input.RawData;
        // generateSummaryRequest.Tag = input.Tag;

        // instead, pull results from a hardcoded regular SQL++ query
        // which matches the Tableau report.
        var generateSummaryRequest = await _demoColumnarData.GetReportRawData();

        var getArticlesResponse = await _mediator.Send(generateSummaryRequest);

        return Ok(new { slug = getArticlesResponse.Slug});
    }

    /// <summary>
    /// Adaptive Articles Feed
    /// </summary>
    /// <remarks>
    /// Returns articles that system determines (via AI/Analytics/Vector/etc) would be of interest to the user.
    /// </remarks>
    /// <param name="options">Options</param>
    /// <returns>List of articles</returns>
    /// <response code="200">Successfully queried articles</response>
    /// <response code="422">Article request is invalid</response>
    [HttpGet]
    [Route("/api/articles/adaptive")]
    public async Task<IActionResult> GetAdaptiveFeed([FromQuery] AdaptiveArticleFilterOptionsModel filter)
    {
        // get (optional) auth info
        string username = null;
        var headers = Request.Headers["Authorization"];
        var isUserAnonymous = headers.All(string.IsNullOrEmpty);
        if (!isUserAnonymous)
        {
            var claims = _authService.GetAllAuthInfo(Request.Headers["Authorization"]);
            username = claims.Username.Value;
        }

        // TODO: make sure adaptive tags have been created?

        var getArticlesRequest = new GetAdaptiveArticlesRequest(username, filter);
        var getArticlesResponse = await _mediator.Send(getArticlesRequest);

        if (getArticlesResponse.IsFailure)
            return UnprocessableEntity("There was an error retrieving articles.");
        if (getArticlesResponse.ValidationErrors?.Any() ?? false)
            return UnprocessableEntity(getArticlesResponse.ValidationErrors.ToCsv());

        return Ok(new { articles = getArticlesResponse.ArticlesView.Articles, articlesCount = getArticlesResponse.ArticlesView.ArticlesCount }); //, articlesCount = getArticlesResponse.NumTotalArticles });
    }
}