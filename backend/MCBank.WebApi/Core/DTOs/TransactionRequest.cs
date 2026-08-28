namespace MCBank.WebApi.Core.DTOs;

public record TransactionRequest(Guid AccountId, decimal Amount);