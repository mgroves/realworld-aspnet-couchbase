namespace Conduit.Web.DataAccess.Models;

public class CommentListDataView
{
    public string Body { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public ulong Id { get; init; }
    public CommentListDataAuthorView Author { get; init; }
}

public class CommentListDataAuthorView
{
    public string Bio { get; init; }
    public string Image { get; init; }
    public string Username { get; init; }
    public bool Following { get; init; }
}