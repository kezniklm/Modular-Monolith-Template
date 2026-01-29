using System.Net;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Module.Domain.Common;

namespace Module.Presentation.Extensions;

internal static class ResultHttpErrors
{
    public static IResult ToHttpResult(this IEnumerable<IError> errorsEnumerable)
    {
        IReadOnlyList<IError> errors = errorsEnumerable.ToList();

        if (errors.OfType<ValidationError>().Any())
        {
            return Results.BadRequest(ToProblem(errors, HttpStatusCode.BadRequest));
        }

        if (errors.OfType<NotFoundError>().Any())
        {
            return Results.NotFound(ToProblem(errors, HttpStatusCode.NotFound));
        }

        if (errors.OfType<ForbiddenError>().Any())
        {
            return Results.Forbid();
        }

        return Results.UnprocessableEntity(ToProblem(errors, HttpStatusCode.UnprocessableEntity));
    }

    private static ProblemDetails ToProblem(IEnumerable<IError> errorsEnumerable, HttpStatusCode status)
    {
        IReadOnlyList<IError> errors = errorsEnumerable.ToList();

        return new ProblemDetails
        {
            Status = (int?)status,
            Title = "Request failed",
            Detail = string.Join(" ", errors.Select(e => e.Message)),
            Extensions =
            {
                ["errors"] = errors.Select(e => new
                {
                    message = e.Message,
                    code = e.Metadata.TryGetValue("code", out var c) ? c : null,
                    meta = e.Metadata
                })
            }
        };
    }
}
