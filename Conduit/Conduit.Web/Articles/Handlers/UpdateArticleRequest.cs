using Conduit.Web.Articles.ViewModels;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class UpdateArticleRequest : IRequest<UpdateArticleResponse>
{
    public string Slug { get; }
    public List<string>? Tags { get; }
    public string? Description { get; }
    public string? Body { get; }
    public string? Title { get; }

    public UpdateArticleRequest(UpdateArticlePostModelArticle model, string slug)
    {
        this.Title = model.Article.Title;
        this.Body = model.Article.Body;
        this.Description = model.Article.Description;
        this.Tags = model.Article.Tags;
        this.Slug = slug;
    }
}