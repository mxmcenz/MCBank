namespace MCBank.WebApi.Core.DTOs;

public record TransferRequest(int FromAccountId, int ToAccountId, decimal Amount);