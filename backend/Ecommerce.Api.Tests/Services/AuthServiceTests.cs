using Ecommerce.Api.DTOs.Auth;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Ecommerce.Api.Repositories.Interfaces;
using Ecommerce.Api.Services;
using Moq;
using Shouldly;

namespace Ecommerce.Api.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_TrimsEmailCapitalizesNamesAndStoresPasswordHash()
    {
        var userRepository = new Mock<IUserRepository>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var jwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        User? capturedUser = null;

        userRepository
            .Setup(repository => repository.EmailExistsAsync(
                "  test@example.com  ",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        userRepository
            .Setup(repository => repository.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .Returns(Task.CompletedTask);
        passwordHasher
            .Setup(hasher => hasher.HashPassword("Password123"))
            .Returns("hashed-password");

        var service = new AuthService(
            userRepository.Object,
            passwordHasher.Object,
            jwtTokenGenerator.Object);

        var response = await service.RegisterAsync(new RegisterUserRequestDto
        {
            Email = "  test@example.com  ",
            Password = "Password123",
            FirstName = "  adrian",
            LastName = "  chiriac"
        });

        capturedUser.ShouldNotBeNull();
        capturedUser.Email.ShouldBe("test@example.com");
        capturedUser.FirstName.ShouldBe("Adrian");
        capturedUser.LastName.ShouldBe("Chiriac");
        capturedUser.PasswordHash.ShouldBe("hashed-password");
        response.Email.ShouldBe("test@example.com");
        response.FirstName.ShouldBe("Adrian");
        response.LastName.ShouldBe("Chiriac");
    }

    [Fact]
    public async Task RegisterAsync_ThrowsWhenEmailAlreadyExists()
    {
        var userRepository = new Mock<IUserRepository>();
        var service = new AuthService(
            userRepository.Object,
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IJwtTokenGenerator>());

        userRepository
            .Setup(repository => repository.EmailExistsAsync(
                "test@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Should.ThrowAsync<InvalidOperationException>(
            () => service.RegisterAsync(new RegisterUserRequestDto
            {
                Email = "test@example.com",
                Password = "Password123",
                FirstName = "Adrian",
                LastName = "Chiriac"
            }));

        userRepository.Verify(
            repository => repository.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNullWhenUserDoesNotExist()
    {
        var userRepository = new Mock<IUserRepository>();
        var service = new AuthService(
            userRepository.Object,
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IJwtTokenGenerator>());

        userRepository
            .Setup(repository => repository.GetByEmailAsync(
                "test@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var response = await service.LoginAsync(new LoginUserRequestDto
        {
            Email = "  test@example.com  ",
            Password = "Password123"
        });

        response.ShouldBeNull();
    }

    [Fact]
    public async Task LoginAsync_ReturnsNullWhenPasswordIsWrong()
    {
        var user = CreateUser();
        var userRepository = new Mock<IUserRepository>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var service = new AuthService(
            userRepository.Object,
            passwordHasher.Object,
            Mock.Of<IJwtTokenGenerator>());

        userRepository
            .Setup(repository => repository.GetByEmailAsync(
                user.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        passwordHasher
            .Setup(hasher => hasher.VerifyPassword("wrong-password", user.PasswordHash))
            .Returns(false);

        var response = await service.LoginAsync(new LoginUserRequestDto
        {
            Email = user.Email,
            Password = "wrong-password"
        });

        response.ShouldBeNull();
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokenAndUserWhenCredentialsAreValid()
    {
        var user = CreateUser();
        var userRepository = new Mock<IUserRepository>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var jwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        var service = new AuthService(
            userRepository.Object,
            passwordHasher.Object,
            jwtTokenGenerator.Object);

        userRepository
            .Setup(repository => repository.GetByEmailAsync(
                user.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        passwordHasher
            .Setup(hasher => hasher.VerifyPassword("Password123", user.PasswordHash))
            .Returns(true);
        jwtTokenGenerator
            .Setup(generator => generator.GenerateToken(user))
            .Returns("jwt-token");

        var response = await service.LoginAsync(new LoginUserRequestDto
        {
            Email = user.Email,
            Password = "Password123"
        });

        response.ShouldNotBeNull();
        response.Token.ShouldBe("jwt-token");
        response.User.Id.ShouldBe(user.Id);
        response.User.Email.ShouldBe(user.Email);
    }

    private static User CreateUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashed-password",
            FirstName = "Adrian",
            LastName = "Chiriac",
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
