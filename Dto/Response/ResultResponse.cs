using System.Text.Json.Serialization;

namespace Base.Api.Dto.Response
{
    public class ResultResponse
    {
        [JsonPropertyOrder(1)]
        public int Code { get; set; }

        [JsonPropertyOrder(2)]
        public bool Success { get; set; }

        [JsonPropertyOrder(3)]
        public string Message { get; set; } = default!;

        [JsonPropertyOrder(4)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ErrorCode { get; set; }

        [JsonPropertyOrder(5)]
        public string TraceId { get; set; } = default!;

        [JsonPropertyOrder(6)]
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        [JsonPropertyOrder(8)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<StackFrameInfo>? StackTrace { get; set; }
    }

    public class Result<T> : ResultResponse
    {
        [JsonPropertyOrder(7)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }
    }
}
