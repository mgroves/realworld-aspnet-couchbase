using Conduit.Web.Auth.ViewModels;
using MediatR;

namespace Conduit.Web.Auth.Handlers;

public class LoginResult : IRequest
{
    public bool IsUnauthorized { get; set; }
    public UserViewModel UserView { get; set; }
}