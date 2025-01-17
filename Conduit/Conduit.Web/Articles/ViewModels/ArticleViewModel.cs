﻿using Conduit.Web.Users.ViewModels;

namespace Conduit.Web.Articles.ViewModels;

public class ArticlesViewModel
{
    public int ArticlesCount { get; set; }
    public List<ArticleViewModel> Articles { get; set; }
}

public class ArticleViewModel
{
    public string Slug { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Body { get; set; }
    public List<string> TagList { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool Favorited { get; set; }
    public int FavoritesCount { get; set; }
    public ProfileViewModel Author { get; set; }
}

public class ArticleViewModelWithCount : ArticleViewModel
{
    public int ArticlesCount { get; set; }
}