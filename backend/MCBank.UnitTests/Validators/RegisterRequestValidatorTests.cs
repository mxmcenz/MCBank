using FluentValidation.TestHelper;
using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Application.Validators;

namespace MCBank.UnitTests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Have_Error_When_Username_Is_Null_Or_Empty(string invalidUsername)
    {
        //Arrange
        var request = new RegisterRequest()
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
    public void Should_Have_Error_When_Username_Length_Less_Than_Three(string invalidUsername)
    {
        //Arrange
        var request = new RegisterRequest
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
        var request = new RegisterRequest
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
    public void Should_Have_Error_When_Password_Length_Less_Than_Five(string invalidPassword)
    {
        //Arrange
        var request = new RegisterRequest
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