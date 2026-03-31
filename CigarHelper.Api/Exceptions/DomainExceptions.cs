namespace CigarHelper.Api.Exceptions;

/// <summary>Ресурс не найден → HTTP 404.</summary>
public sealed class NotFoundException(string message) : Exception(message);

/// <summary>Доступ запрещён (авторизован, но не имеет права) → HTTP 403.</summary>
public sealed class ForbiddenException(string message) : Exception(message);

/// <summary>Конфликт состояния (дублирование и т.п.) → HTTP 409.</summary>
public sealed class ConflictException(string message) : Exception(message);
