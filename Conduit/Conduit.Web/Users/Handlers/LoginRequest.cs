using Conduit.Web.Users.ViewModels;
using MediatR;

namespace Conduit.Web.Users.Handlers;

public class LoginRequest : IRequest<LoginResult>
{
    public readonly LoginSubmitModel Model;

    public LoginRequest(LoginSubmitModel model)
    {
        Model = model;
    }
}