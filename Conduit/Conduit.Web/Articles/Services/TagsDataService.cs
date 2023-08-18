using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;

namespace Conduit.Web.Articles.Services;

public interface ITagsDataService
{
    Task<List<string>> GetAllTags();
}

public class TagsDataService : ITagsDataService
{
    private readonly IConduitTagsCollectionProvider _tagsCollectionProvider;

    public TagsDataService(IConduitTagsCollectionProvider tagsCollectionProvider)
    {
        _tagsCollectionProvider = tagsCollectionProvider;
    }

    public async Task<List<string>> GetAllTags()
    {
        var collection = await _tagsCollectionProvider.GetCollectionAsync();

        var tagsDoc = await collection.GetAsync("tagData");
        var tagData = tagsDoc.ContentAs<TagData>();

        return tagData.Tags;
    }
}