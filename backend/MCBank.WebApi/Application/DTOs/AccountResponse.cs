using MCBank.WebApi.Core.Enums;

namespace MCBank.WebApi.Application.DTOs;

public sealed record AccountResponse(int Id, string Iban, decimal Balance, AccountType Type);