using BasketService.Application.Users.Commands;
using FluentAssertions;

namespace BasketService.Tests.Application.Users.Commands;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_WhenCommandIsValid_ReturnsNoErrors()
    {
        var command = new LoginCommand
        {
            Email = "valid.user@example.com",
            Name = "Valid User"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WhenEmailAndNameAreMissing_ReturnsExpectedErrors()
    {
        var command = new LoginCommand();

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(LoginCommand.Email) && error.ErrorMessage == "Email is required");
        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(LoginCommand.Name) && error.ErrorMessage == "Name is required");
    }

    [Fact]
    public async Task ValidateAsync_WhenEmailIsInvalid_ReturnsEmailAddressError()
    {
        var command = new LoginCommand
        {
            Email = "not-an-email",
            Name = "Valid User"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(LoginCommand.Email) && error.ErrorMessage == "Email must be a valid email address");
    }
}