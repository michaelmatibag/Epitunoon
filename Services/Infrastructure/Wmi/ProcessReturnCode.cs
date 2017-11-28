namespace Services.Infrastructure.Wmi
{
    public enum ProcessReturnCode
    {
        Error = -1,
        Success = 0,
        AccessDenied = 2,
        InsufficientPrivileges = 3,
        UnknownFailure = 8,
        PathNotFound = 9,
        InvalidParameter = 21
    }
}