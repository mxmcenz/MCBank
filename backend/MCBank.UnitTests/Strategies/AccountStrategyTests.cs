using FluentAssertions;
using MCBank.WebApi.Application.Strategies;
using MCBank.WebApi.Core.Entities;
using MCBank.WebApi.Core.Enums;

namespace MCBank.UnitTests.Strategies;

public class AccountStrategyTests
{
    public static IEnumerable<object[]> GetWithdrawData() =>
        new List<object[]>
        {
            new object[] { AccountType.Current, 1000m, 500m, true },
            new object[] { AccountType.Current, 1000m, 1500m, false },
            new object[] { AccountType.SavingsFlexible, 1000m, 500m, true },
            new object[] { AccountType.SavingsFixed, 1000m, 500m, false },
            new object[] { AccountType.SavingsReplenishable, 1000m, 500m, false }
        };

    [Theory]
    [MemberData(nameof(GetWithdrawData))]
    public void CanWithdraw_ShouldReturnExpectedResult(AccountType type, decimal balance, decimal amount,
        bool expectedSuccess)
    {
        //Arrange
        var strategy = new AccountStrategyFactory().GetStrategy(type);
        var account = new Account { Balance = balance, Type = type };

        //Act
        var result = strategy.CanWithdraw(amount, account);

        //Assert
        result.IsSuccess.Should().Be(expectedSuccess);
    }

    public static IEnumerable<object[]> GetDepositData() =>
        new List<object[]>
        {
            new object[] { AccountType.Current, 1000m, true },
            new object[] { AccountType.SavingsFixed, 1000m, false },
            new object[] { AccountType.SavingsFlexible, 1000m, true },
            new object[] { AccountType.SavingsReplenishable, 1000m, true }
        };

    [Theory]
    [MemberData(nameof(GetDepositData))]
    public void CanDeposit_ShouldReturnExpectedResult(AccountType type, decimal amount, bool expectedSuccess)
    {
        //Arrange
        var strategy = new AccountStrategyFactory().GetStrategy(type);

        //Act
        var result = strategy.CanDeposit(amount);

        //Assert
        result.IsSuccess.Should().Be(expectedSuccess);
    }

    public static IEnumerable<object[]> GetTransferData() =>
        new List<object[]>
        {
            new object[] { AccountType.Current, 1000m, 500m, true },
            new object[] { AccountType.Current, 1000m, 1500m, false },
            new object[] { AccountType.SavingsFlexible, 1000m, 500m, true },
            new object[] { AccountType.SavingsFixed, 1000m, 500m, false },
            new object[] { AccountType.SavingsReplenishable, 1000m, 500m, false }
        };

    [Theory]
    [MemberData(nameof(GetTransferData))]
    public void CanTransfer_ShouldReturnExpectedResult(AccountType type, decimal balance, decimal amount,
        bool expectedSuccess)
    {
        //Arrange
        var strategy = new AccountStrategyFactory().GetStrategy(type);
        var account = new Account { Balance = balance, Type = type };

        //Act
        var result = strategy.CanTransfer(amount, account);

        //Assert
        result.IsSuccess.Should().Be(expectedSuccess);
    }
}