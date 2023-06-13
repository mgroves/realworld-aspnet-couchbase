using Conduit.Web.Auth.ViewModels;
using MediatR;

namespace Conduit.Web.Auth.Handlers;

public class LoginRequest : IRequest<LoginResult>
{
    public readonly LoginSubmitModel Model;

    public LoginRequest(LoginSubmitModel model)
    {
        Model = model;
    }
}