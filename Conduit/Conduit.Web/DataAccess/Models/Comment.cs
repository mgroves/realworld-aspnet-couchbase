using Newtonsoft.Json;

namespace Conduit.Web.DataAccess.Models;

public class Comment
{
    public ulong Id { get; set; }
    public string Body { get; set; }
    public string AuthorUsername { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    [JsonIgnore] // there is no "update comment" functionality yet, so it's pointless to store this, but updatedAt is part of the conduit spec
    public DateTimeOffset UpdatedAt => CreatedAt;
}