using Conduit.Web;
using Conduit.Web.Users.Services;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Conduit.Tests.Functional;

public abstract class FunctionalTestBase
{
    protected WebApplicationFactory<Program> WebAppFactory;
    protected HttpClient WebClient;
    protected IAuthService AuthSvc;

    [OneTimeSetUp]
    public virtual async Task Setup()
    {
        WebAppFactory = GlobalFunctionalSetUp.WebAppFactory;
        WebClient = GlobalFunctionalSetUp.WebClient;
        AuthSvc = GlobalFunctionalSetUp.AuthSvc;
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        // cleanup happens in GlobalFunctionalSetUp
    }
}