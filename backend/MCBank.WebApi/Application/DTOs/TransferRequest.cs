namespace MCBank.WebApi.Application.DTOs;

public record TransferRequest(int FromAccountId, int ToAccountId, decimal Amount);