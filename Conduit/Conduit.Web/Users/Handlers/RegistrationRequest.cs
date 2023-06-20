using Conduit.Web.Users.ViewModels;
using MediatR;

namespace Conduit.Web.Users.Handlers;

public class RegistrationRequest : IRequest<RegistrationResult>
{
    public RegistrationSubmitModel Model { get; private set; }

    public RegistrationRequest(RegistrationSubmitModel model)
    {
        Model = model;
    }
}