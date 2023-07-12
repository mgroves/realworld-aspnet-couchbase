namespace Conduit.Web.Models;

/// <summary>
/// This should only be used to return documents from SQL++ queries
/// i.e. QueryAsync<KeyWrapper<T>>
/// The SQL++ query SELECT should be of the form: SELECT u AS document, META(u).id FROM collection u
/// And this should only be used within Data Services, but NOT returned BY Data Services
/// </summary>
public class KeyWrapper<T>
{
    public T Document { get; set; }
    public string Id { get; set; }
}