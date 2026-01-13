namespace DMS.Api.Shared
{
    /// <summary>
    /// Standard response status codes for API responses
    /// </summary>
    public enum ResponseStatus
    {
        // Success codes (1xxx)
        Success = 1000,
        LoginSuccess = 1001,
        DataRetrieved = 1002,
        DataSaved = 1003,
        DataUpdated = 1004,
        DataDeleted = 1005,

        // Client Error codes (4xxx)
        BadRequest = 4000,
        ValidationError = 4001,
        NotFound = 4002,
        AlreadyExists = 4003,
        Unauthorized = 4004,
        Forbidden = 4005,

        // Authentication/Login specific codes (4100-4199)
        InvalidCredentials = 4100,
        UserNotFound = 4101,
        UserInactive = 4102,
        CompanyInactive = 4103,
        CenterInactive = 4104,
        AccountLocked = 4105,
        PasswordExpired = 4106,

        // Server Error codes (5xxx)
        InternalServerError = 5000,
        DatabaseError = 5001,
        ExternalServiceError = 5002
    }
}
