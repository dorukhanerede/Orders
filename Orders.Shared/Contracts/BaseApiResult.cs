namespace Orders.Shared.Contracts;

public class BaseApiResult
{
    public bool Success { get; init; }
    public List<ApiResultErrorObject>? Errors { get; init; }
}

public class BaseApiResult<T> : BaseApiResult
{
    public T? Data { get; init; }
}