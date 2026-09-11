namespace DPDP.Application.Common.Exceptions;

public sealed class NotFoundException(string entityName, object key)
    : Exception($"{entityName} '{key}' was not found.");
