using Conduit.Web.ViewModels;
using MediatR;

namespace Conduit.Web.Requests.Auth;

public class RegistrationRequest : IRequest<RegistrationResult>
{
    public readonly RegistrationSubmitModel Model;

    public RegistrationRequest(RegistrationSubmitModel model)
    {
        Model = model;
    }
}