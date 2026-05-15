using Ecommerce.Api.Controllers;
using Ecommerce.Api.DTOs.Auth;
using Ecommerce.Api.DTOs.Errors;
using Ecommerce.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace Ecommerce.Api.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ReturnsCreatedUserWhenRegistrationSucceeds()
    {
        var request = new RegisterUserRequestDto
        {
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Adrian",
            LastName = "Chiriac"
        };
        var user = new UserResponseDto(Guid.NewGuid(), request.Email, request.FirstName, request.LastName);
        var authService = new Mock<IAuthService>();
        var controller = new AuthController(authService.Object);

        authService
            .Setup(service => service.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await controller.Register(request, CancellationToken.None);

        var statusResult = result.Result.ShouldBeOfType<ObjectResult>();
        statusResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
        statusResult.Value.ShouldBe(user);
    }

    [Fact]
    public async Task Register_ReturnsConflictWhenEmailAlreadyExists()
    {
        var request = new RegisterUserRequestDto
        {
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Adrian",
            LastName = "Chiriac"
        };
        var authService = new Mock<IAuthService>();
        var controller = new AuthController(authService.Object);

        authService
            .Setup(service => service.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("An account with this email already exists."));

        var result = await controller.Register(request, CancellationToken.None);

        var conflictResult = result.Result.ShouldBeOfType<ConflictObjectResult>();
        var error = conflictResult.Value.ShouldBeOfType<ApiErrorResponseDto>();
        error.Message.ShouldBe("An account with this email already exists.");
    }

    [Fact]
    public async Task Login_ReturnsOkWhenCredentialsAreValid()
    {
        var request = new LoginUserRequestDto
        {
            Email = "test@example.com",
            Password = "Password123"
        };
        var response = new LoginUserResponseDto(
            "jwt-token",
            new UserResponseDto(Guid.NewGuid(), request.Email, "Adrian", "Chiriac"));
        var authService = new Mock<IAuthService>();
        var controller = new AuthController(authService.Object);

        authService
            .Setup(service => service.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await controller.Login(request, CancellationToken.None);

        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(response);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorizedWhenCredentialsAreInvalid()
    {
        var request = new LoginUserRequestDto
        {
            Email = "test@example.com",
            Password = "wrong-password"
        };
        var authService = new Mock<IAuthService>();
        var controller = new AuthController(authService.Object);

        authService
            .Setup(service => service.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoginUserResponseDto?)null);

        var result = await controller.Login(request, CancellationToken.None);

        var unauthorizedResult = result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
        var error = unauthorizedResult.Value.ShouldBeOfType<ApiErrorResponseDto>();
        error.Message.ShouldBe("Invalid email or password.");
    }
}
