namespace Conduit.Web.DataAccess.Dto;

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