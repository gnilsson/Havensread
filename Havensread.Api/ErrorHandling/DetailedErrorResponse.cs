namespace Havensread.Api.ErrorHandling;

public sealed class DetailedErrorResponse : ErrorResponse
{
    public required string Reason { get; init; }
};
