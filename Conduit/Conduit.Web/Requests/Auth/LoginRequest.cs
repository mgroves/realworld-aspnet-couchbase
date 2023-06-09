using Conduit.Web.ViewModels;
using MediatR;

namespace Conduit.Web.Requests.Auth;

public class LoginRequest : IRequest<LoginResult>
{
    public readonly LoginSubmitModel Model;

    public LoginRequest(LoginSubmitModel model)
    {
        Model = model;
    }
}