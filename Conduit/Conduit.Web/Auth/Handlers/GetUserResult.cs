using Conduit.Web.Auth.ViewModels;
using MediatR;

namespace Conduit.Web.Auth.Handlers;

public class GetUserResult : IRequest
{
    public UserViewModel UserView { get; set; }

    public bool IsInvalidToken { get; set; }
}