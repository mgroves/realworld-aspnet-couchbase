using MediatR;

namespace Conduit.Web.Users.Handlers;

public class GetCurrentUserRequest : IRequest<GetCurrentUserResult>
{
    public GetCurrentUserRequest(string bearerToken)
    {
        BearerToken = bearerToken;
    }

    public string BearerToken { get; private set; }
}