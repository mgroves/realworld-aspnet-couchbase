﻿using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Dto;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class GetArticlesHandler : IRequestHandler<GetArticlesRequest, GetArticlesResponse>
{
    private readonly IValidator<GetArticlesRequest> _validator;
    private readonly IArticlesDataService _articlesDataService;

    public GetArticlesHandler(IValidator<GetArticlesRequest> validator, IArticlesDataService articlesDataService)
    {
        _validator = validator;
        _articlesDataService = articlesDataService;
    }

    public async Task<GetArticlesResponse> Handle(GetArticlesRequest request, CancellationToken cancellationToken)
    {
        // validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new GetArticlesResponse
            {
                ValidationErrors = validationResult.Errors
            };
        }

        // create spec for request
        var spec = request.Spec;
        spec.Username = spec.Username?.Trim();
        spec.AuthorUsername = spec.AuthorUsername?.Trim();
        spec.FavoritedByUsername = spec.FavoritedByUsername?.Trim();
        spec.Tag = spec.Tag?.Trim();

        var result = await _articlesDataService.GetArticles(spec);
        if (result.Status != DataResultStatus.Ok)
        {
            return new GetArticlesResponse
            {
                IsFailure = true
            };
        }

        return new GetArticlesResponse
        {
            ArticlesView = result.DataResult
        };
    }
}