namespace Conduit.Web.Models;

// TODO: move this out of Users slice, since it's likely to be used
// by multiple slices
// maybe this goes into Models?
public class DataServiceResult<T>
{
    public T DataResult { get; }
    public DataResultStatus Status { get; }

    public DataServiceResult(T dataResult, DataResultStatus status)
    {
        DataResult = dataResult;
        Status = status;
    }
}