using Core;
using Microsoft.AspNetCore.Diagnostics;

namespace Api.Errors;

public sealed class DomainExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not DomainException)
            return false;

        await TypedResults.Problem(statusCode: StatusCodes.Status409Conflict, title: "Конфлікт", detail: exception.Message).ExecuteAsync(context);

        return true;
    }
}