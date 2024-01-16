using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Models;

namespace Conduit.Web.Users.Services;

public interface IGenerativeAiService
{
    Task<List<string>> GetTagsFromContent(string content);
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
}