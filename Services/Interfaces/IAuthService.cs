using Base.Api.Dto.Request;
using Base.Api.Dto.Response;

namespace Base.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Result<SessionResponse>> LoginAsync(AuthRequest request);
        Task<ResultResponse> LogoutAsync(Guid userGuid);
    }
}
