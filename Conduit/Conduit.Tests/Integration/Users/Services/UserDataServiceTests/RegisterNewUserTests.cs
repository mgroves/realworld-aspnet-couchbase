using Conduit.Web.Models;
using Conduit.Web.Users.Services;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Users.Services.UserDataServiceTests;

[TestFixture]
public class RegisterNewUserTests : CouchbaseIntegrationTest
{
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private Web.Users.Services.UserDataService _userDataService;

    [OneTimeSetUp]
    public override async Task Setup()
    {
        await base.Setup();

        ServiceCollection.AddCouchbaseBucket<IConduitBucketProvider>("ConduitIntegrationTests", b =>
        {
            b
                .AddScope("_default")
                .AddCollection<IConduitUsersCollectionProvider>("Users");
        });

        _usersCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();
        _userDataService = new UserDataService(_usersCollectionProvider, new AuthService());
    }

    [Test]
    public async Task Given_a_user_with_a_username_the_username_is_only_stored_as_document_id()
    {
        // arrange
        var usernameKey = $"key-{Path.GetRandomFileName()}";
        var userToInsert = new User
        {
            Bio = "lorem ipsum",
            Email = "foo@bar.baz",
            Image = "http://example.net/foo.jpg",
            Password = "abc",
            PasswordSalt = "abcsalty",
            Username = usernameKey
        };

        // act
        var result = await _userDataService.RegisterNewUser(userToInsert);

        // assert that username was only stored as key/id
        var collection = await _usersCollectionProvider.GetCollectionAsync();
        var doc = await collection.GetAsync(usernameKey);
        var docContent = doc.ContentAs<User>();
        Assert.That(docContent.Username, Is.Null);
    }
}