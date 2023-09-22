using Conduit.Web.DataAccess.Models;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.ViewModels;

namespace Conduit.Web.Articles.ViewModels;

public class CommentViewModel
{
    public ulong Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string Body { get; init; }
    public ProfileViewModel Author { get; init; }
}