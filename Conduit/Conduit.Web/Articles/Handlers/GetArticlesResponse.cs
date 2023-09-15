using Conduit.Web.Articles.ViewModels;
using FluentValidation.Results;

namespace Conduit.Web.Articles.Handlers;

public class GetArticlesResponse
{
    public List<ValidationFailure> ValidationErrors { get; set; }
    public List<ArticleViewModel> ArticlesView { get; set; }
    public bool IsFailure { get; set; }
}