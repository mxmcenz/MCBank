using FluentValidation.TestHelper;
using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Application.Validators;

namespace MCBank.UnitTests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Have_Error_When_Username_Is_Null_Or_Empty(string invalidUsername)
    {
        //Arrange
        var request = new LoginRequest
        {
            Username = invalidUsername,
            Password = "password"
        };
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("u")]
    [InlineData("us")]
    public void Should_Have_Error_When_Username_Length_Is_Three_Or_Less(string invalidUsername)
    {
        //Arrange
        var request = new LoginRequest
        {
            Username = invalidUsername,
            Password = "password"
        };
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Have_Error_When_Password_Is_Null_Or_Empty(string invalidPassword)
    {
        //Arrange
        var request = new LoginRequest
        {
            Username = "username",
            Password = invalidPassword
        };
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
    
    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("123")]
    [InlineData("1234")]
    public void Should_Have_Error_When_Password_Length_Is_Five_Or_Less(string invalidPassword)
    {
        //Arrange
        var request = new LoginRequest
        {
            Username = "username",
            Password = invalidPassword
        };
        
        //Act
        var result = _validator.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}