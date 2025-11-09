using challenge_moto_connect.Application.DTOs;
using challenge_moto_connect.Application.DTOs.Pagination;
using challenge_moto_connect.Domain.Entity;

namespace challenge_moto_connect.Application.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO> GetUserByIdAsync(Guid id);
        Task<UserDTO> CreateUserAsync(UserDTO userDto);
        Task UpdateUserAsync(Guid id, UserDTO userDto);
        Task DeleteUserAsync(Guid id);
        Task<PagedListDto<UserDTO>> GetPagedUsersAsync(PaginationParams paginationParams);
        Task<UserDTO> GetUserByEmailAsync(string email);
    }
}

