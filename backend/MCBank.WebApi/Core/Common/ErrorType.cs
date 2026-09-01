namespace MCBank.WebApi.Core.Common;

public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Forbidden,
    Conflict,
    Failure,
    Unauthorized
}