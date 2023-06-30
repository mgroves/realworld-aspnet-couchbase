using Conduit.Web.Users.Handlers;

namespace Conduit.Web.Users.ViewModels;

public class UpdateUserSubmitModel
{
    public UpdateUserViewModelUser User { get; set; }
}

/// <summary>
/// At least one of these fields is required: username, password, image, bio
/// </summary>
public class UpdateUserViewModelUser : IHasUserPropertiesForValidation
{
    /// <summary>
    /// Email is required.
    /// Must match a user.
    /// Must be a valid-looking email address. (e.g. foo@edu is not valid, foo@bar.edu is valid)
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Username must not be in use by another user.
    /// Username can be no longer than 100 characters.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Password must be at least 10 characters long.
    /// Password must contain at least one digit.
    /// Password must contain at least one uppercase letter.
    /// Password must contain at least one lowercase letter.
    /// Password must contain at least one symbol.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Image URL must be valid.
    /// Image URL must be JPG, JPEG, or PNG.
    /// </summary>
    public string Image { get; set; }

    /// <summary>
    /// Bio is limited to 500 characters.
    /// </summary>
    public string Bio { get; set; }
}