namespace FoodOrdering.Domain.Common;

/// <summary>
/// Base exception for domain-specific errors.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown when a business rule is violated.
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleViolationException(string ruleName, string message)
        : base(message)
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId)
        : base($"{entityType} with ID '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Thrown when an invalid state transition is attempted.
/// </summary>
public class InvalidStateTransitionException : DomainException
{
    public string CurrentState { get; }
    public string AttemptedAction { get; }

    public InvalidStateTransitionException(string currentState, string attemptedAction)
        : base($"Cannot perform '{attemptedAction}' when in state '{currentState}'.")
    {
        CurrentState = currentState;
        AttemptedAction = attemptedAction;
    }
}