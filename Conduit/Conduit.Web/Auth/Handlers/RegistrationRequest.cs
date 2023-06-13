using Conduit.Web.Auth.ViewModels;
using MediatR;

namespace Conduit.Web.Auth.Handlers;

public class RegistrationRequest : IRequest<RegistrationResult>
{
    public readonly RegistrationSubmitModel Model;

    public RegistrationRequest(RegistrationSubmitModel model)
    {
        Model = model;
    }
}