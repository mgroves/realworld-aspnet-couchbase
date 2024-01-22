using Conduit.Web.Articles.ViewModels;
using Conduit.Web.DataAccess.Models;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Models;

namespace Conduit.Web.Users.Services;

public interface IGenerativeAiService
{
    Task<List<string>> GetTagsFromContent(string content);
    Task<Article> GenerateSummaryArticle(string requestRawData, string tag);
}

public class OpenAiService : IGenerativeAiService
{
    private readonly IOpenAIAPI _openAiApi;

    public OpenAiService(IOpenAIAPI openAiApi)
    {
        _openAiApi = openAiApi;
    }

    public async Task<List<string>> GetTagsFromContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new List<string>();

        var prompt = @$"Suggest tags to index this content: ""{content}"". Only give me the tags in a JSON array form," +
            " and only give me tags that might be useful for others who are looking for content," +
            " and keep tags to 2 words or less.";

        var chat = _openAiApi.Chat.CreateConversation();
        chat.Model = Model.ChatGPTTurbo;    // 3.5-turbo, faster and cheaper than GPT4 for demoing purposes
        chat.RequestParameters.Temperature = 0.1;
        chat.AppendUserInput(prompt);
        var response = await chat.GetResponseFromChatbotAsync();

        // remove JSON markup and any text before/after JSON necessary
        if (response.Contains("```json"))
        {
            response = response.Split("```json")[1];
            response = response.Split("```")[0];
        }

        try
        {
            return JsonConvert.DeserializeObject<List<string>>(response);
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<Article> GenerateSummaryArticle(string rawContentToSummarize, string tag)
    {
        if (string.IsNullOrWhiteSpace(rawContentToSummarize))
            return null;

        var prompt =
            $@"Write a single blog post to summarize the articles in the following data that pertains to the ""{tag}"" tag
            within a certain date range (like a week). Use quotes from and links (via the slugs) to the articles when appropriate.
            I want your response to be in pure JSON format only, following this template:

            {{
            ""title"" : ""<put your title here>"",
            ""description : ""<put a short description here>"",
            ""body"" : ""<put your body here, it should be at least 1400 words, in markdown format>"",
            ""tagList"" : [""<tag 1>"", ""<tag 2"", ... etc ],
            }}

            Make sure to summarize the comments, including any
            interesting data, popularity and engagement, and the overall sentiment.

            Don't list a summary of each article individually: create a single cohesive summary
            that ties all of the content together. Use a casual blog style, avoid formality.

            Here is the content to summarize:

            {rawContentToSummarize}";

        var chat = _openAiApi.Chat.CreateConversation();
        chat.Model = Model.ChatGPTTurbo;    // 3.5-turbo, faster and cheaper than GPT4 for demoing purposes
        chat.RequestParameters.Temperature = 0.1;
        chat.AppendUserInput(prompt);
        var response = await chat.GetResponseFromChatbotAsync();

        // remove JSON markup and any text before/after JSON necessary
        if (response.Contains("```json"))
        {
            response = response.Split("```json")[1];
            response = response.Split("```")[0];
        }

        try
        {
            var article = JsonConvert.DeserializeObject<Article>(response);
            return article;
        }
        catch
        {
            return null;
        }
    }
}