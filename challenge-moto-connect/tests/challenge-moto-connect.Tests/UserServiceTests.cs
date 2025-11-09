using challenge_moto_connect.Application.DTOs;
using challenge_moto_connect.Application.Services;
using challenge_moto_connect.Domain.Entity;
using challenge_moto_connect.Domain.Interfaces;
using challenge_moto_connect.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
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
                Password.FromHash("100000:c2FsdA==:aGFzaGVkUGFzc3dvcmQxMjM="),
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
                Password = "Senha123!@#",
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
                Password.FromHash("100000:c2FsdA==:aGFzaGVkUGFzc3dvcmQxMjM="),
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
                Password.FromHash("100000:c2FsdA==:aGFzaGVkUGFzc3dvcmQxMjM="),
                UserType.ADMIN
            );

            var users = new List<User> { user };
            var queryable = users.AsQueryable().BuildMock();
            
            _mockRepository.Setup(repo => repo.GetByCondition(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .Returns(queryable);

            var result = await _userService.GetUserByEmailAsync(email);

            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithInvalidEmail_ReturnsNull()
        {
            var users = new List<User>();
            var queryable = users.AsQueryable().BuildMock();
            
            _mockRepository.Setup(repo => repo.GetByCondition(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .Returns(queryable);

            var result = await _userService.GetUserByEmailAsync("nonexistent@example.com");

            Assert.Null(result);
        }
    }

    public static class QueryableExtensions
    {
        public static IQueryable<T> BuildMock<T>(this IQueryable<T> source) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(source.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(source.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(source.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(source.GetEnumerator());
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(source.GetEnumerator()));
            return mockSet.Object;
        }
    }

    public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executeMethod = typeof(IQueryProvider)
                .GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(System.Linq.Expressions.Expression) })
                .MakeGenericMethod(resultType);
            var result = executeMethod.Invoke(_inner, new object[] { expression });
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { result });
        }
    }

    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
            : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return default;
        }
    }
}
