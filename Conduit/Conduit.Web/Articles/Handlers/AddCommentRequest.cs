using System.Data;
using Conduit.Web.Articles.ViewModels;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class AddCommentRequest : IRequest<AddCommentResponse>
{
    public string Username { get; set; }
    public string Slug { get; set; }
    public string Body { get; set; }

    public AddCommentRequest(string username, CommentBodyModel model, string slug)
    {
        Username = username;
        Slug = slug;
        Body = model?.Comment?.Body;
    }
}