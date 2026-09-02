using FluentValidation.TestHelper;
using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Application.Validators;

namespace MCBank.UnitTests.Validators;

public class TransferRequestValidatorTests
{
    private readonly TransferRequestValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Should_Have_Error_When_From_Account_Id_Is_Zero_Or_Less(int invalidFromAccountId)
    {
        //Arrange
        var request = new TransferRequest(invalidFromAccountId, 1, 100);

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.FromAccountId);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    public void Should_Have_Error_When_From_Account_Id_Equal_To_Account_Id(int fromAccountId, int toAccountId)
    {
        //Arrange
        var request = new TransferRequest(fromAccountId, toAccountId, 100);

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.FromAccountId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Should_Have_Error_When_To_Account_Id_Is_Zero_Or_Less(int invalidToAccountId)
    {
        //Arrange
        var request = new TransferRequest(1, invalidToAccountId, 100);

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.ToAccountId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Should_Have_Error_When_Amount_Is_Zero_Or_Less(decimal invalidAmount)
    {
        //Arrange
        var request = new TransferRequest(1, 2, invalidAmount);

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
        var request = new TransferRequest(1, 2, invalidAmount);

        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }
}