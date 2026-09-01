using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Application.Interfaces;
using MCBank.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MCBank.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController(IBankService bankService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("{accountId:int}")]
    public async Task<IActionResult> GetAccountById(int accountId)
    {
        var result = await bankService.GetAccountByIdAsync(accountId, CurrentUserId);

        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        var result = await bankService.GetAllAccountsAsync(CurrentUserId);

        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount()
    {
        var result = await bankService.CreateAccountAsync(CurrentUserId);

        if (result.IsFailure)
            return result.ToActionResult();

        return CreatedAtAction(nameof(GetAccountById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] TransactionRequest request)
    {
        var result = await bankService.DepositAsync(request.AccountId, CurrentUserId, request.Amount);

        return result.ToActionResult();
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] TransactionRequest request)
    {
        var result = await bankService.WithdrawAsync(request.AccountId, CurrentUserId, request.Amount);

        return result.ToActionResult();
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        var result =
            await bankService.TransferAsync(request.FromAccountId, request.ToAccountId, CurrentUserId, request.Amount);

        return result.ToActionResult();
    }

    [HttpGet("{accountId:int}/transactions")]
    public async Task<IActionResult> GetAccountTransactions(int accountId)
    {
        var result = await bankService.GetTransactionHistoryAsync(accountId, CurrentUserId);

        return result.ToActionResult();
    }

    [HttpDelete("{accountId:int}")]
    public async Task<IActionResult> DeleteAccountById(int accountId)
    {
        var result = await bankService.DeleteAccount(accountId, CurrentUserId);

        return result.ToActionResult();
    }
}