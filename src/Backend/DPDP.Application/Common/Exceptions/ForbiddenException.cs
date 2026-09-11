namespace DPDP.Application.Common.Exceptions;

/// <summary>
/// Distinct from UnauthorizedAccessException: this means "authenticated,
/// but not allowed" — e.g. a tenant-boundary violation caught at the
/// application layer as defense in depth, beyond the query-filter layer.
/// </summary>
public sealed class ForbiddenException(string message) : Exception(message);
