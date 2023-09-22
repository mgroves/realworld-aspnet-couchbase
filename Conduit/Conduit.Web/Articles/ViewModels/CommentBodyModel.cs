namespace Conduit.Web.Articles.ViewModels;

public class CommentBodyModel
{
    public CommentBody Comment { get; set; }
}

public class CommentBody
{
    /// <summary>
    /// Body of the comment, required, must be less than 1000 characters
    /// </summary>
    public string Body { get; set; }
}