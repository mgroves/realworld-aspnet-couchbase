using Conduit.Web.Articles.ViewModels;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.Users.ViewModels;
using FluentValidation.Results;

namespace Conduit.Web.Articles.Handlers;

public class GetArticleResponse
{
    public GetArticleResponse() { }

    public GetArticleResponse(Article article, User user)
    {
        ArticleView = new ArticleViewModel();
        ArticleView.Slug = article.Slug;
        ArticleView.Title = article.Title;
        ArticleView.Description = article.Description;
        ArticleView.Body = article.Body;
        ArticleView.TagList = article.TagList;
        ArticleView.CreatedAt = article.CreatedAt;
        ArticleView.Favorited = article.Favorited;
        ArticleView.FavoritesCount = article.FavoritesCount;

        ArticleView.Author = new ProfileViewModel();
        ArticleView.Author.Bio = user.Bio;
        ArticleView.Author.Image = user.Image;
        ArticleView.Author.Username = user.Username;
    }

    public ArticleViewModel ArticleView { get; set; }
    public List<ValidationFailure> ValidationErrors { get; set; }
}