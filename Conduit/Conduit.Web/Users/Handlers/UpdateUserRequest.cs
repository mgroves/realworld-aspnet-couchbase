using Conduit.Web.Users.ViewModels;
using MediatR;

namespace Conduit.Web.Users.Handlers;

public class UpdateUserRequest : IRequest<UpdateUserResult>
{
    public UpdateUserSubmitModel Model { get; private set; }

    public UpdateUserRequest(UpdateUserSubmitModel updateUser)
    {
        Model = updateUser;
    }
}