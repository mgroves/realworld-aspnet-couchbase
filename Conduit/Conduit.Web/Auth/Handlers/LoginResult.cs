using Conduit.Web.Auth.ViewModels;
using FluentValidation.Results;
using MediatR;

namespace Conduit.Web.Auth.Handlers;

public class LoginResult
{
    public bool IsUnauthorized { get; set; }
    public UserViewModel UserView { get; set; }
    public List<ValidationFailure> ValidationErrors { get; set; }
}