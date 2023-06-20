using MediatR;

namespace Conduit.Web.Users.Handlers;

public class GetUserRequest : IRequest<GetUserResult>
{
    public GetUserRequest(string bearerToken)
    {
        BearerToken = bearerToken;
    }

    public string BearerToken { get; private set; }
}