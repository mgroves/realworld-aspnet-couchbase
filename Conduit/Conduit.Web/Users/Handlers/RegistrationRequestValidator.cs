using Conduit.Web.DataAccess.Dto;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Couchbase.Query;
using FluentValidation;

namespace Conduit.Web.Users.Handlers;

public class RegistrationRequestValidator : AbstractValidator<RegistrationRequest>
{
    private readonly IUserDataService _userDataService;

    public RegistrationRequestValidator(SharedUserValidator<RegistrationUserSubmitModel> sharedUser, IUserDataService userDataService)
    {
        _userDataService = userDataService;

        RuleFor(x => x.Model.User.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email address must not be empty.");

        RuleFor(x => x.Model.User)
            .SetValidator(sharedUser);

        RuleFor(x => x.Model.User.Username)
            .Cascade(CascadeMode.Stop)
            .MustAsync(async (username, cancellation) => await NotAlreadyExist(username, cancellation))
            .WithMessage("That username is already in use.");

        RuleFor(x => x.Model.User.Password)
            .NotEmpty().WithMessage("Password must not be empty.");

        // TODO: consider using zxcvbn library to provide a better measure of password strength
        // as the above password policy may be weak
    }

    private async Task<bool> NotAlreadyExist(string username, CancellationToken cancellationToken)
    {
        var user = await _userDataService.GetUserByUsername(username);

        var userAlreadyExists = user.Status == DataResultStatus.Ok;

        return !userAlreadyExists;
    }
}