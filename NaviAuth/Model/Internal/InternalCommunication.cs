namespace NaviAuth.Model.Internal;

public enum ResultType
{
    Success,
    DataNotFound,
    DataConflicts,
    InvalidRequest,
    UnknownFailure
}

public class InternalCommunication<T>
{
    public ResultType ResultType { get; set; }
    public T? TargetObject { get; set; }
    public string Message { get; set; }

    public static InternalCommunication<T> Success(T? targetObject)
    {
        return new InternalCommunication<T>
        {
            ResultType = ResultType.Success,
            TargetObject = targetObject
        };
    }
}