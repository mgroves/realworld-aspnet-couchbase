using Conduit.Web.Articles.Handlers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Web.Articles.Controllers;

public class TagController : Controller
{
    private readonly IMediator _mediator;

    public TagController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// List all (allowed) tags
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints/#get-tags">Conduit Spec for get tags endpoint</a>
    /// </remarks>
    /// <returns>List of tags</returns>
    /// <response code="200">Returns all allowed tags</response>
    [Route("/api/tags")]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var tags = await _mediator.Send(new GetTagsRequest());

        return Ok(tags);
    }
}