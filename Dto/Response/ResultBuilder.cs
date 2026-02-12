namespace Base.Api.Dto.Response
{
    public class ResultBuilder
    {
        // Success with data
        public static Result<T> Success<T>(T data, string message = "Success", int code = 200)
        {
            return new Result<T>
            {
                Success = true,
                Message = message,
                Code = code,
                Data = data
            };
        }

        // Failure with generic type
        public static Result<T> Fail<T>(string message = "Failed", int code = 400)
        {
            return new Result<T>
            {
                Success = false,
                Message = message,
                Code = code,
                Data = default
            };
        }

        // Success without data
        public static ResultResponse Success(string message = "Success", int code = 200)
        {
            return new ResultResponse
            {
                Success = true,
                Message = message,
                Code = code
            };
        }

        // Failure without data
        public static ResultResponse Fail(string message = "Failed", int code = 400)
        {
            return new ResultResponse
            {
                Success = false,
                Message = message,
                Code = code
            };
        }
    }
}
