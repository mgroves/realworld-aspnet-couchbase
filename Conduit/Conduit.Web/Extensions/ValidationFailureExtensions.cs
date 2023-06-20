using Conduit.Web.Auth.Handlers;
using FluentValidation.Results;

namespace Conduit.Web.Extensions;

public static class ValidationFailureExtensions
{
    public static string ToCsv(this List<ValidationFailure>? @this)
    {
        if (@this == null)
            return string.Empty;

        return string.Join(",", @this.Select(e => e.ErrorMessage));
    }
}