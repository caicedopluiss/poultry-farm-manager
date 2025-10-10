using System.Collections.Generic;
using System.Linq;

namespace PoultryFarmManager.WebAPI;

public record FieldValidationError(string FieldName, IEnumerable<string> Errors);

public record ErrorResponse
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public IEnumerable<FieldValidationError> ValidationErrors { get; init; } = [];

    public ErrorResponse()
    {
    }

    public ErrorResponse(int statusCode, string message)
    {
        StatusCode = statusCode;
        Message = message;
    }

    public ErrorResponse(string message, IEnumerable<(string propertyName, string errorMessage)> validationErrors) : this(400, message)
    {
        ValidationErrors = validationErrors
            .GroupBy(x => x.propertyName, x => x.errorMessage, (name, errors) => new FieldValidationError(name, errors));
    }

    public ErrorResponse(IEnumerable<(string, string)> validationErrors) : this("There are some properties validation errors.", validationErrors)
    {
    }

}
