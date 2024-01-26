﻿using Conduit.Web.Articles.ViewModels;
using FluentValidation.Results;

namespace Conduit.Web.Adaptive.Handlers;

public class GetAdaptiveArticlesResponse
{
    public bool IsFailure { get; set; }
    public List<ValidationFailure> ValidationErrors { get; set; }
    public ArticlesViewModel ArticlesView { get; set; }
}