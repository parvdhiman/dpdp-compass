namespace DPDP.Application.Common.Exceptions;

/// <summary>
/// Always use the generic message "Invalid email or password" at the
/// catch site regardless of whether the email existed — never let this
/// distinguish "no such account" from "wrong password" to the caller.
/// </summary>
public sealed class AuthenticationFailedException(string message) : Exception(message);
