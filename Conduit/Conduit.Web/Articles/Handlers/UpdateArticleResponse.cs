using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Models;
using FluentValidation.Results;

namespace Conduit.Web.Articles.Handlers;

public class UpdateArticleResponse
{
    public bool IsNotFound { get; set; }
    public List<ValidationFailure> ValidationErrors { get; set; }
}