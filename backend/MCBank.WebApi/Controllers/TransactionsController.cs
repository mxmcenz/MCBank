using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Application.Interfaces;
using MCBank.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MCBank.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController(ITransactionService transactionService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] TransactionRequest request)
    {
        var result = await transactionService.DepositAsync(request.AccountId, CurrentUserId, request.Amount);

        return result.ToActionResult();
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] TransactionRequest request)
    {
        var result = await transactionService.WithdrawAsync(request.AccountId, CurrentUserId, request.Amount);

        return result.ToActionResult();
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        var result =
            await transactionService.TransferAsync(request.FromAccountId, request.ToAccountId, CurrentUserId, request.Amount);

        return result.ToActionResult();
    }

    [HttpGet("{accountId:int}/transactions")]
    public async Task<IActionResult> GetAccountTransactions(int accountId)
    {
        var result = await transactionService.GetTransactionHistoryAsync(accountId, CurrentUserId);

        return result.ToActionResult();
    }
}