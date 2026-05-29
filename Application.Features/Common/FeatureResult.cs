namespace Application.Features.Common;

public abstract record FeatureResult;

public sealed record FeatureOkResult<T>(T Value) : FeatureResult;

public sealed record FeatureEmptyOk() : FeatureResult;

public sealed record FeatureNoContent() : FeatureResult;

public sealed record FeatureNotFound(string? Message = null) : FeatureResult;

public sealed record FeatureBadRequest(object Body) : FeatureResult;

public sealed record FeatureUnauthorized(object? Body = null) : FeatureResult;

public sealed record FeatureConflict(string Message) : FeatureResult;

public sealed record FeatureForbiddenResult<T>(T Body) : FeatureResult;

public sealed record FeatureCreatedResult<T>(T Value, string ActionName, object RouteValues) : FeatureResult;

public sealed record FeatureRedirect(string Url) : FeatureResult;
