using Microsoft.Extensions.Configuration;
using OpenAI_API;

namespace Conduit.Tests.Integration.Articles.Services.OpenAiService;

[TestFixture]
public class GetTagsFromContentTests
{
    private IConfigurationRoot _config;
    private Web.Users.Services.OpenAiService _openAi;

    [SetUp]
    public async Task Setup()
    {
        _config = new ConfigurationBuilder()
            .AddUserSecrets<CouchbaseIntegrationTest>()
            .AddEnvironmentVariables()
            .Build();

        var openAiApi = new OpenAIAPI(_config["OpenAIApiKey"]);
        _openAi = new Web.Users.Services.OpenAiService(openAiApi);
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase(" ")]
    public async Task Empty_content_gets_no_tags(string content)
    {
        // nothing to arrange

        // act
        var tags = await _openAi.GetTagsFromContent(content);

        // assert
        Assert.That(tags, Is.Empty);
    }

    public async Task Get_tags()
    {
        // arrange a bio with some hopefully obvious tags for chatgpt to pick up on
        var bio = "This is my bio. Here are tags that are interesting to me: sailing, waffles, bananas.";

        // act
        var tags = await _openAi.GetTagsFromContent(bio);

        // assert
        Assert.That(tags.Contains("sailing"), Is.True);
        Assert.That(tags.Contains("waffles"), Is.True);
        Assert.That(tags.Contains("bananas"), Is.True);
    }
}