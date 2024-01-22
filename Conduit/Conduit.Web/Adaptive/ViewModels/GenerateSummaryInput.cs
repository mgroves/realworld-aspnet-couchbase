namespace Conduit.Web.Adaptive.ViewModels;

public class GenerateSummaryInput
{
    /// <summary>
    /// Raw data, possibly output from a complex analytics query
    /// to provide information to summarize.
    /// </summary>
    public string RawData { get; set; }

    /// <summary>
    /// A single tag that this summary relates to.
    /// </summary>
    public string Tag { get; set; }
}