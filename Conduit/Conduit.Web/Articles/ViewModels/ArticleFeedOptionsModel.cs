﻿namespace Conduit.Web.Articles.ViewModels;

public class ArticleFeedOptionsModel
{
    /// <summary>
    /// Limit the number of articles returned (Default of 20, maximum of 50)
    /// </summary>
    public int? Limit { get; set; }
    /// <summary>
    /// Skip a number of articles (Default of 0)
    /// </summary>
    public int? Offset { get; set; }
}