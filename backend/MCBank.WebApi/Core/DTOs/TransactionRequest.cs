namespace MCBank.WebApi.Core.DTOs;

public record TransactionRequest(int AccountId, decimal Amount);