using MediatR;

namespace Conduit.Web.Users.Handlers;

public class GetProfileRequest : IRequest<GetProfileResult>
{
    public GetProfileRequest(string username, string optionalBearerToken)
    {
        Username = username;
        OptionalBearerToken = optionalBearerToken;
    }

    public string Username { get; private set; }
    public string OptionalBearerToken { get; private set; }
}