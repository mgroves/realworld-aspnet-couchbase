using MediatR;

namespace Conduit.Web.Auth.Handlers;

public class GetUserRequest : IRequest<GetUserResult>
{
    public GetUserRequest(string bearerToken)
    {
        BearerToken = bearerToken;
    }

    public string BearerToken { get; private set; }
}