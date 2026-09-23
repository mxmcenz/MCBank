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
            new object[] { AccountType.SavingsFixed, 1000m, true },
            new object[] { AccountType.SavingsFlexible, 1000m, true },
            new object[] { AccountType.SavingsReplenishable, 1000m, true }
        };

    [Theory]
    [MemberData(nameof(GetDepositData))]
    public void CanDeposit_ShouldReturnExpectedResult(AccountType type, decimal amount, bool expectedSuccess)
    {
        //Arrange
        var strategy = new AccountStrategyFactory().GetStrategy(type);
        var account = new Account { Balance = 0, Type = type };

        //Act
        var result = strategy.CanDeposit(amount, account);

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

    [Theory]
    [InlineData(AccountType.SavingsFlexible, 1000, 14.5)]
    public void CalculateInterest_ShouldReturnExpectedValue(AccountType type, decimal balance, decimal rate)
    {
        // Arrange
        var strategy = new AccountStrategyFactory().GetStrategy(type);
        var expected = balance * (rate / 100) / 365;

        // Act
        var result = strategy.CalculateInterest(balance, rate);

        // Assert
        result.Should().BeApproximately(expected, 0.000001m);
    }

    public static IEnumerable<object[]> GetInterestDueTestData() =>
        new List<object[]>
        {
            new object[] { new SavingsFlexibleAccountStrategy(), null!, "2026-09-23", true },
            new object[] { new SavingsFlexibleAccountStrategy(), "2026-09-22", "2026-09-23", true },
            new object[] { new SavingsFlexibleAccountStrategy(), "2026-09-23", "2026-09-23", false },

            new object[] { new SavingsFixedAccountStrategy(), null!, "2026-09-23", true },
            new object[] { new SavingsFixedAccountStrategy(), "2026-08-23", "2026-09-23", true },
            new object[] { new SavingsFixedAccountStrategy(), "2026-09-22", "2026-09-23", false },
            new object[] { new SavingsFixedAccountStrategy(), "2026-02-28", "2026-03-28", true },

            new object[] { new SavingsReplenishableAccountStrategy(), null!, "2026-09-23", true },
            new object[] { new SavingsReplenishableAccountStrategy(), "2026-08-23", "2026-09-23", true },
            new object[] { new SavingsReplenishableAccountStrategy(), "2026-09-20", "2026-09-23", false },

            new object[] { new CurrentAccountStrategy(), null!, "2026-09-23", false },
            new object[] { new CurrentAccountStrategy(), "2026-01-01", "2026-09-23", false }
        };

    [Theory]
    [MemberData(nameof(GetInterestDueTestData))]
    public void IsInterestDue_ShouldReturnExpectedResult(IAccountStrategy strategy, string? lastAppliedStr,
        string nowStr, bool expected)
    {
        DateTime? lastApplied = lastAppliedStr != null ? DateTime.Parse(lastAppliedStr) : null;
        var now = DateTime.Parse(nowStr);

        var result = strategy.IsInterestDue(lastApplied, now);

        result.Should().Be(expected);
    }
}