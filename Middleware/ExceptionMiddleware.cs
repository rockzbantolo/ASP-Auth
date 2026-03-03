using Base.Api.Dto;
using Base.Api.Dto.Response;
using Serilog.Context;

namespace Base.Api.Middleware
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IConfiguration config)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionMiddleware> _logger = logger;
        private readonly bool _isDevMode = config.GetValue<bool>("DevMode");

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                string traceId = ctx.TraceIdentifier ?? Guid.NewGuid().ToString();

                using (LogContext.PushProperty("TraceId", traceId))
                using (LogContext.PushProperty("RequestPath", ctx.Request.Path))
                using (LogContext.PushProperty("Method", ctx.Request.Method))
                {
                    _logger.LogError(ex, "Unhandled exception");
                }

                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                ctx.Response.ContentType = "application/json";

                var response = CreateErrorResponse(ex, traceId);

                await ctx.Response.WriteAsJsonAsync(response);
            }
        }

        private ResultResponse CreateErrorResponse(Exception ex, string traceId)
        {
            if (!_isDevMode)
            {
                return ResultBuilder.Fail(
                    message: "Internal server error.",
                    code: 500,
                    errorCode: "INTERNAL_ERROR",
                    traceId: traceId
                );
            }

            // Dev mode - only the originating frame
            var frames = new List<StackFrameInfo>();

            var st = new System.Diagnostics.StackTrace(ex, true);
            var originFrame = st.GetFrames()
                .FirstOrDefault(f =>
                    !string.IsNullOrEmpty(f.GetFileName()) &&
                    f.GetFileLineNumber() > 0 &&
                    !f.GetFileName().Contains("ExceptionMiddleware.cs", StringComparison.OrdinalIgnoreCase));

            if (originFrame != null)
            {
                var method = originFrame.GetMethod();
                if (method != null)
                {
                    var methodName = $"{method.DeclaringType?.FullName}.{method.Name}";
                    if (method.IsGenericMethod) methodName += "<...>";

                    frames.Add(new StackFrameInfo
                    {
                        Method = methodName + "()",
                        File = originFrame.GetFileName(),
                        Line = originFrame.GetFileLineNumber()
                    });
                }
            }

            string message = $"{ex.GetType().Name}: {ex.Message}";
            if (ex.InnerException != null)
                message += $"\nInner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";

            return ResultBuilder.Fail(
                message: message,
                code: 500,
                errorCode: ex.GetType().Name,
                traceId: traceId,
                stackTrace: frames.Count > 0 ? frames : null
            );
        }
    }
}
