using MCBank.WebApi.Core.Enums;

namespace MCBank.WebApi.Application.DTOs;

public sealed record CreateAccountRequest(AccountType AccountType);