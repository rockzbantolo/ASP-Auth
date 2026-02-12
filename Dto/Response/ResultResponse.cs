using System.Text.Json.Serialization;

namespace Base.Api.Dto.Response
{
    public class ResultResponse
    {
        [JsonPropertyOrder(1)]
        public int? Code { get; set; }

        [JsonPropertyOrder(2)]
        public bool Success { get; set; }

        [JsonPropertyOrder(3)]
        public string Message { get; set; } = string.Empty;
    }

    public class Result<T> : ResultResponse
    {
        [JsonPropertyOrder(4)]
        public T? Data { get; set; }
    }
}
