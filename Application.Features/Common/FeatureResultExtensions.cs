using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Common;

public static class FeatureResultExtensions
{
    public static ActionResult ToActionResult(this FeatureResult result, ControllerBase controller)
    {
        return result switch
        {
            FeatureEmptyOk => controller.Ok(),
            FeatureNoContent => controller.NoContent(),
            FeatureNotFound { Message: { } message } => controller.NotFound(message),
            FeatureNotFound => controller.NotFound(),
            FeatureBadRequest badRequest => controller.BadRequest(badRequest.Body),
            FeatureUnauthorized { Body: { } body } => controller.Unauthorized(body),
            FeatureUnauthorized => controller.Unauthorized(),
            FeatureConflict conflict => controller.Conflict(conflict.Message),
            FeatureRedirect redirect => controller.Redirect(redirect.Url),
            _ => MapValueResult(result, controller)
        };
    }

    public static ActionResult<T> ToActionResult<T>(this FeatureResult result, ControllerBase controller)
    {
        if (result is FeatureOkResult<T> ok)
        {
            return controller.Ok(ok.Value);
        }

        if (result is FeatureCreatedResult<T> created)
        {
            return controller.CreatedAtAction(created.ActionName, created.RouteValues, created.Value);
        }

        if (result is FeatureForbiddenResult<T> forbidden)
        {
            return (ActionResult<T>)(object)controller.StatusCode(StatusCodes.Status403Forbidden, forbidden.Body);
        }

        return (ActionResult<T>)result.ToActionResult(controller);
    }

    private static ActionResult MapValueResult(FeatureResult result, ControllerBase controller)
    {
        var resultType = result.GetType();
        if (resultType.IsGenericType)
        {
            var definition = resultType.GetGenericTypeDefinition();
            var value = resultType.GetProperty("Value")?.GetValue(result)
                ?? resultType.GetProperty("Body")?.GetValue(result);

            if (definition == typeof(FeatureOkResult<>))
            {
                return controller.Ok(value);
            }

            if (definition == typeof(FeatureCreatedResult<>))
            {
                var actionName = (string)resultType.GetProperty("ActionName")!.GetValue(result)!;
                var routeValues = resultType.GetProperty("RouteValues")!.GetValue(result)!;
                return controller.CreatedAtAction(actionName, routeValues, value);
            }

            if (definition == typeof(FeatureForbiddenResult<>))
            {
                return controller.StatusCode(StatusCodes.Status403Forbidden, value);
            }
        }

        throw new InvalidOperationException($"Unsupported feature result type: {result.GetType().Name}");
    }
}
