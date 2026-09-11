namespace DPDP.Application.Common.Exceptions;

public sealed class AccountLockedException(DateTimeOffset lockoutEnd)
    : Exception($"Account is locked until {lockoutEnd:O}.")
{
    public DateTimeOffset LockoutEnd { get; } = lockoutEnd;
}
