using challenge_moto_connect.Application.DTOs;
using challenge_moto_connect.Application.Services;
using challenge_moto_connect.Domain.Entity;
using challenge_moto_connect.Domain.Interfaces;
using challenge_moto_connect.Domain.ValueObjects;
using Moq;
using Xunit;

namespace challenge_moto_connect.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IRepository<User>> _mockRepository;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockRepository = new Mock<IRepository<User>>();
            _userService = new UserService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ReturnsUserDTO()
        {
            var userId = Guid.NewGuid();
            var user = new User(
                userId,
                "Test User",
                new Email("test@example.com"),
                new Password("password123"),
                UserType.ADMIN
            );

            _mockRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var result = await _userService.GetUserByIdAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(userId, result.UserID);
            Assert.Equal("Test User", result.Name);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
        {
            var userId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            var result = await _userService.GetUserByIdAsync(userId);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUserAsync_WithValidData_ReturnsCreatedUser()
        {
            var userDto = new UserDTO
            {
                UserID = Guid.NewGuid(),
                Name = "New User",
                Email = "newuser@example.com",
                Password = "password123",
                Type = (int)UserType.MECHANIC
            };

            _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var result = await _userService.CreateUserAsync(userDto);

            Assert.NotNull(result);
            Assert.Equal(userDto.Name, result.Name);
            Assert.Equal(userDto.Email, result.Email);
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_WithValidId_CallsRepositoryDelete()
        {
            var userId = Guid.NewGuid();
            var user = new User(
                userId,
                "Test User",
                new Email("test@example.com"),
                new Password("password123"),
                UserType.ADMIN
            );

            _mockRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _mockRepository.Setup(repo => repo.DeleteAsync(userId))
                .Returns(Task.CompletedTask);

            await _userService.DeleteUserAsync(userId);

            _mockRepository.Verify(repo => repo.DeleteAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithValidEmail_ReturnsUser()
        {
            var email = "test@example.com";
            var user = new User(
                Guid.NewGuid(),
                "Test User",
                new Email(email),
                new Password("password123"),
                UserType.ADMIN
            );

            _mockRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User> { user });

            var result = await _userService.GetUserByEmailAsync(email);

            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithInvalidEmail_ReturnsNull()
        {
            _mockRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User>());

            var result = await _userService.GetUserByEmailAsync("nonexistent@example.com");

            Assert.Null(result);
        }
    }
}

