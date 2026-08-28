namespace MCBank.WebApi.Core.DTOs;

public record TransferRequest(Guid FromAccountId, Guid ToAccountId, decimal Amount);