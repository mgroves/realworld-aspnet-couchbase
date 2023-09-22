using Conduit.Web.DataAccess.Models;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class GetCommentsRequest : IRequest<GetCommentsResponse>
{
    public string? Username { get; set; }
    public string Slug { get; set; }

    public GetCommentsRequest(string slug, string? username)
    {
        Slug = slug;
        Username = username;
    }
}