﻿using System.Security.Cryptography.X509Certificates;

namespace Conduit.Web.Articles.ViewModels;

public class CreateArticleSubmitModel
{
     public CreateArticleSubmitModelArticle Article { get; set; }
}

public class CreateArticleSubmitModelArticle
{
    /// <summary>
    /// Article title, required. Must be between 10-100 characters
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Article description, required. Must be between 10-200 characters
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Article body, required. Must be between 10 and 15,000,000 characters
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// Tags, optional. Any tags specified must be from the allowed list of tags.
    /// </summary>
    public List<string> Tags { get; set; }
}