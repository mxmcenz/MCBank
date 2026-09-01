namespace MCBank.WebApi.Application.DTOs;

public sealed record AccountResponse(int Id, string Iban, decimal Balance);