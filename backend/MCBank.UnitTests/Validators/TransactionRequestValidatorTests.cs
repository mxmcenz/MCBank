using FluentValidation.TestHelper;
using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Application.Validators;

namespace MCBank.UnitTests.Validators;

public class TransactionRequestValidatorTests
{
    private readonly TransactionRequestValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Should_Have_Error_When_Account_Id_Is_Zero_Or_Less(int invalidId)
    {
        //Arrange
        var request = new TransactionRequest(invalidId, 100);

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Should_Have_Error_When_Amount_Is_Zero_Or_Less(decimal invalidAmount)
    {
        //Arrange
        var request = new TransactionRequest(1, invalidAmount);

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Theory]
    [InlineData(10.123)]
    [InlineData(0.003)]
    [InlineData(20.003321)]
    public void Should_Have_Error_When_Amount_Has_More_Than_Two_Decimal_Places(decimal invalidAmount)
    {
        //Arrange
        var request = new TransactionRequest(1, invalidAmount);

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        //Arrange
        var request = new TransactionRequest(1, 500);

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}