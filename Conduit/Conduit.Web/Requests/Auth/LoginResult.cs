using Conduit.Web.ViewModels;
using MediatR;

namespace Conduit.Web.Requests.Auth;

public class LoginResult : IRequest
{
    public bool IsUnauthorized { get; set; }
    public UserViewModel UserView { get; set; }
}