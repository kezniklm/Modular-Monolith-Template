using FluentResults;

namespace Module.Domain.Common;

public abstract class ValidationError(string message) : Error(message);

public abstract class DomainError(string message) : Error(message);

public abstract class NotFoundError(string message) : Error(message);

public abstract class ForbiddenError(string message) : Error(message);
