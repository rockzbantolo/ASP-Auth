using Base.Api.Dto.Request.Users;
using Base.Api.Dto.Response;
using Base.Api.Models;

namespace Base.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result<int>> AddAsync(CreateUserRequest request, SessionResponse userSession);
        Task<Result<UserModel>> GetByIdAsync(int userId);
        Task<Result<int>> UpdateUserAsync(UpdateUserRequest request, SessionResponse userSession);
        Task<ResultResponse> DeleteAsync(int userId);
    }
}
