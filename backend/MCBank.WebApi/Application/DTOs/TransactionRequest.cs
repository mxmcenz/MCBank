namespace MCBank.WebApi.Application.DTOs;

public record TransactionRequest(int AccountId, decimal Amount);