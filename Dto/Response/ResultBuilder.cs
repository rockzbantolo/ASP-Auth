using System;

namespace Base.Api.Dto.Response
{
    public static class ResultBuilder
    {
        public static Result<T> Success<T>(T data, string message = "Success", int code = 200, string? traceId = null)
        {
            return BuildResult(true, message, code, data, null, traceId, null);
        }

        public static Result<T> Fail<T>(string message = "Failed", int code = 400, string? errorCode = null,
            string? traceId = null, List<StackFrameInfo>? stackTrace = null)
        {
            return BuildResult(false, message, code, default(T), errorCode, traceId, stackTrace);
        }

        public static ResultResponse Success(string message = "Success", int code = 200, string? traceId = null)
        {
            return BuildResultBase(true, message, code, null, traceId, null);
        }

        public static ResultResponse Fail(string message = "Failed", int code = 400, string? errorCode = null,
            string? traceId = null, List<StackFrameInfo>? stackTrace = null)
        {
            return BuildResultBase(false, message, code, errorCode, traceId, stackTrace);
        }

        private static Result<T> BuildResult<T>(bool success, string message, int code, T? data,
            string? errorCode, string? traceId, List<StackFrameInfo>? stackTrace)
        {
            return new Result<T>
            {
                Success = success,
                Message = message,
                Code = code,
                Data = data,
                ErrorCode = errorCode,
                TraceId = traceId ?? Guid.NewGuid().ToString(),
                TimestampUtc = DateTime.UtcNow,
                StackTrace = stackTrace
            };
        }

        private static ResultResponse BuildResultBase(bool success, string message, int code,
            string? errorCode, string? traceId, List<StackFrameInfo>? stackTrace)
        {
            return new ResultResponse
            {
                Success = success,
                Message = message,
                Code = code,
                ErrorCode = errorCode,
                TraceId = traceId ?? Guid.NewGuid().ToString(),
                TimestampUtc = DateTime.UtcNow,
                StackTrace = stackTrace
            };
        }
    }
}
