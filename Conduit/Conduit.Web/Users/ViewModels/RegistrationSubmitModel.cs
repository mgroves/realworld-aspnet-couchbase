using Conduit.Web.Users.Handlers;

namespace Conduit.Web.Users.ViewModels;

/// <summary>
/// Registration submission information
/// </summary>
public record RegistrationSubmitModel
{
    public RegistrationUserSubmitModel User { get; set; }
}

public record RegistrationUserSubmitModel : IHasUserPropertiesForValidation
{
    /// <summary>
    /// Username is required.
    /// Username must be unique.
    /// Username can be no longer than 100 characters.
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// Email is required.
    /// Must be a valid-looking email address. (e.g. foo@edu is not valid, foo@bar.edu is valid)
    /// </summary>
    public string Email { get; set; }
    /// <summary>
    /// Password is required.
    /// Password must be at least 10 characters long.
    /// Password must contain at least one digit.
    /// Password must contain at least one uppercase letter.
    /// Password must contain at least one lowercase letter.
    /// Password must contain at least one symbol.
    /// </summary>
    public string Password { get; set; }
}