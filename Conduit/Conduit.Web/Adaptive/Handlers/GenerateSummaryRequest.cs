using MediatR;

namespace Conduit.Web.Adaptive.Handlers;

public class GenerateSummaryRequest : IRequest<GenerateSummaryResponse>
{
    public string RawData { get; set; }
    public string Tag { get; set; }
}