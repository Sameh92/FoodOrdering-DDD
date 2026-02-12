namespace FoodOrdering.Application.Common;



/// <summary>
/// Base exception for application errors.
/// </summary>
public class ApplicationException : Exception
{
    public ApplicationException(string message) : base(message) { }
    public ApplicationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception when an entity is not found.
/// </summary>
public class NotFoundException : ApplicationException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.") { }
}

/// <summary>
/// Exception when validation fails.
/// </summary>
public class ValidationException : ApplicationException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}