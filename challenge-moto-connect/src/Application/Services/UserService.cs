using System;
using challenge_moto_connect.Application.DTOs;
using challenge_moto_connect.Application.DTOs.Pagination;
using challenge_moto_connect.Domain.Entity;
using challenge_moto_connect.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using challenge_moto_connect.Domain.ValueObjects;

namespace challenge_moto_connect.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;

        public UserService(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDTO> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            return MapToDto(user);
        }

        public async Task<UserDTO> CreateUserAsync(UserDTO userDto)
        {
            if (string.IsNullOrWhiteSpace(userDto.Password))
                throw new ArgumentException("Senha é obrigatória para cadastro.");

            var password = Password.FromPlainText(userDto.Password);
            var user = new User(Guid.NewGuid(), userDto.Name, new Email(userDto.Email), password, (UserType)userDto.Type);
            await _userRepository.AddAsync(user);
            return MapToDto(user);
        }

        public async Task UpdateUserAsync(Guid id, UserDTO userDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new KeyNotFoundException("User not found.");

            user.Name = userDto.Name;
            user.UpdateEmail(new Email(userDto.Email));

            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                user.UpdatePassword(Password.FromPlainText(userDto.Password));
            }

            user.Type = (UserType)userDto.Type;

            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new KeyNotFoundException("User not found.");

            await _userRepository.DeleteAsync(user.UserID);
        }

        public async Task<PagedListDto<UserDTO>> GetPagedUsersAsync(PaginationParams paginationParams)
        {
            var users = _userRepository.GetAllAsQueryable();
            var pagedUsers = PagedListDto<User>.ToPagedList(users, paginationParams.PageNumber, paginationParams.PageSize);

            var userDtos = pagedUsers.Items.Select(MapToDto).ToList();

            return new PagedListDto<UserDTO>(userDtos, pagedUsers.TotalCount, pagedUsers.CurrentPage, pagedUsers.PageSize);
        }

        public async Task<UserDTO> GetUserByEmailAsync(string email)
        {
            var emailValue = new Email(email);
            var user = await _userRepository.GetByCondition(u => u.Email == emailValue).FirstOrDefaultAsync();

            if (user == null) return null;

            return MapToDto(user);
        }

        public async Task<UserDTO> AuthenticateAsync(string email, string password)
        {
            var emailValue = new Email(email);
            var user = await _userRepository.GetByCondition(u => u.Email == emailValue).FirstOrDefaultAsync();
            if (user == null) return null;
            if (!user.Password.Verify(password)) return null;
            return MapToDto(user);
        }

        private static UserDTO MapToDto(User user)
        {
            return new UserDTO
            {
                UserID = user.UserID,
                Name = user.Name,
                Email = user.Email.Address,
                Type = (int)user.Type
            };
        }
    }
}

