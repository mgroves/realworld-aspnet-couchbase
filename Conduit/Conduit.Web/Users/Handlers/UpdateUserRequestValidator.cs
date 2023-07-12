using Conduit.Web.Models;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Couchbase.Query;
using FluentValidation;

namespace Conduit.Web.Users.Handlers;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    private readonly IUserDataService _userDataService;

    public UpdateUserRequestValidator(IUserDataService userDataService, SharedUserValidator<UpdateUserViewModelUser> sharedUser)
    {
        _userDataService = userDataService;

        RuleFor(x => x.Model.User)
            .SetValidator(sharedUser);

        RuleFor(u => u)
            .Cascade(CascadeMode.Stop)
            .Must(HaveAtLeastOneChange)
                .WithMessage("You must specify a value for at least one of: email, password, image, bio.");

        RuleFor(u => u.Model)
            .Cascade(CascadeMode.Stop)
            .MustAsync(NotMatchAnyOtherEmail).When(x => !string.IsNullOrEmpty(x.Model.User.Username))
            .WithMessage("That email is already in use.");

        RuleFor(u => u.Model.User.Bio)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(500).When(r => !string.IsNullOrEmpty(r.Model.User.Bio))
                .WithMessage("Bio is limited to 500 characters.");

        RuleFor(u => u.Model.User.Image)
            .Cascade(CascadeMode.Stop)
            .Must(BeAValidUrl).When(r => !string.IsNullOrEmpty(r.Model.User.Image)).WithMessage("Image URL must be valid.")
            .Must(BeAValidImageUrl).When(r => !string.IsNullOrEmpty(r.Model.User.Image)).WithMessage("Image URL must be JPG, JPEG, or PNG.");
    }

    private async Task<bool> NotMatchAnyOtherEmail(UpdateUserSubmitModel model, CancellationToken cancellationToken)
    {
        var doesADifferentUserExistWithTheGivenEmail = await _userDataService.DoesExistUserByEmailAndUsername(model.User.Email, model.User.Username);
        return !doesADifferentUserExistWithTheGivenEmail;
    }

    private bool BeAValidImageUrl(string url)
    {
        var uri = new Uri(url);

        var fileName = Path.GetFileName(uri.LocalPath);
        var extension = Path.GetExtension(fileName).ToLower();

        return extension is ".jpg" or ".png" or ".jpeg";
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult);
    }

    private bool HaveAtLeastOneChange(UpdateUserRequest model)
    {
        if (!string.IsNullOrEmpty(model.Model.User.Password))
            return true;
        if (!string.IsNullOrEmpty(model.Model.User.Bio))
            return true;
        if (!string.IsNullOrEmpty(model.Model.User.Image))
            return true;
        if (!string.IsNullOrEmpty(model.Model.User.Email))
            return true;
        return false;
    }
}