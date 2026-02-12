using Base.Api.Dto.Request.Users;
using Base.Api.Dto.Response;
using Base.Api.Models;

namespace Base.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result<int>> AddAsync(CreateUserRequest request, UserSession userSession);
        Task<Result<User>> GetByIdAsync(int userId);
        Task<Result<int>> UpdateUserAsync(UpdateUserRequest request, UserSession userSession);
        Task<ResultResponse> DeleteAsync(int userId);
    }
}
