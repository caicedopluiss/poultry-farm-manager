using System.Collections.Generic;
using System.Linq;

namespace SharedLib.CQRS;

public interface IAppRequestResultBase
{
    bool IsSuccess { get; }
    public IEnumerable<(string field, string error)> ValidationErrors { get; }
    string Message { get; }
}

public interface IAppRequestResult<out TValue> : IAppRequestResultBase
{
    TValue? Value { get; }
}


public record AppRequestResultBase : IAppRequestResultBase
{
    public bool IsSuccess { get; }
    public IEnumerable<(string field, string error)> ValidationErrors { get; } = [];
    public string Message { get; init; } = string.Empty;

    public AppRequestResultBase(bool isSuccess = false)
    {
        IsSuccess = isSuccess;
        Message = isSuccess ? string.Empty : "Application request error result.";
    }

    public AppRequestResultBase(IEnumerable<(string field, string error)> validationErrors)
    {
        IsSuccess = validationErrors == null || !validationErrors.Any();
        Message = IsSuccess ? string.Empty : "There are some validation errors.";
        ValidationErrors = validationErrors ?? [];
    }
}

public record AppRequestResult<TValue> : AppRequestResultBase, IAppRequestResult<TValue>
{
    public TValue? Value { get; }

    public AppRequestResult() : base(false)
    {
        Value = default;
    }

    public AppRequestResult(IEnumerable<(string field, string error)> validationErrors) : base(validationErrors)
    {
        Value = default;
    }

    public AppRequestResult(TValue value) : base(true)
    {
        Value = value;
    }
}