namespace Conduit.Web.Users.Services;

// TODO: move this out of Users slice, since it's likely to be used
// by multiple slices
// maybe this goes into Models?
public enum DataResultStatus
{
    NotFound = 0,
    Ok = 1,
    FailedToInsert = 2
}