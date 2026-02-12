using Base.Api.Dto.Request;
using Base.Api.Dto.Response;

namespace Base.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Result<UserSession>> LoginAsync(AuthRequest request);
    }
}
