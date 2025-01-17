﻿using Conduit.Tests.TestHelpers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Tests.TestHelpers.Dto.Handlers;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Users.Handlers;

[TestFixture]
public class LoginRequestHandlerTests : CouchbaseIntegrationTest
{
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private LoginRequestHandler _loginRequestHandler;

    [OneTimeSetUp]
    public override async Task Setup()
    {
        await base.Setup();

        _usersCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();

        // arrange the handler
        var authService = AuthServiceHelper.Create();
        _loginRequestHandler = new LoginRequestHandler(authService,
            new LoginRequestValidator(),
            new UserDataService(_usersCollectionProvider, authService));
    }

    [Test]
    public async Task LoginRequestHandler_Is_Successful()
    {
        // arrange the request
        var request = LoginRequestHelper.Create();

        // arrange for a user to already be in the database
        await _usersCollectionProvider.CreateUserInDatabase(email: request.Model.User.Email, password: request.Model.User.Password);

        // *** act
        var result = await _loginRequestHandler.Handle(request, CancellationToken.None);

        // *** assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsUnauthorized, Is.False);
        Assert.That(result.UserView, Is.Not.Null);
        Assert.That(result.UserView.Email, Is.EqualTo(request.Model.User.Email));
    }
}