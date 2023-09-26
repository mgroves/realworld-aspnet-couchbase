using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class DeleteCommentRequest : IRequest<DeleteCommentResponse>
{
    public string Slug { get; init; }
    public ulong CommentId { get; init; }
    public string Username { get; init; }
}