namespace Conduit.Web.Users.ViewModels;

/// <summary>
/// Login credentials
/// </summary>
public record LoginSubmitModel
{
    public LoginUserViewModel User { get; set; }
}

public record LoginUserViewModel
{
    /// <summary>
    /// Email is required.
    /// Must match a user.
    /// Must be a valid-looking email address. (e.g. foo@edu is not valid, foo@bar.edu is valid)
    /// </summary>
    public string Email { get; set; }
    /// <summary>
    /// Password is requied.
    /// Must match the password for the given user.
    /// Password must be at least 10 characters long.
    /// Password must contain at least one digit.
    /// Password must contain at least one uppercase letter.
    /// Password must contain at least one lowercase letter.
    /// Password must contain at least one symbol.
    /// </summary>
    public string Password { get; set; }
}
