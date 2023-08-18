namespace Conduit.Web.Articles.ViewModels;

public class CreateArticleSubmitModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Body { get; set; }
    public List<string> Tags { get; set; }
}