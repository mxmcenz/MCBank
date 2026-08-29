using MCBank.WebApi.Core.DTOs;
using MCBank.WebApi.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MCBank.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController(IBankService bankService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        var result = await bankService.GetAllAccountsAsync();

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount()
    {
        var result = await bankService.CreateAccountAsync();

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetAccounts), new { id = result.Value.Id }, result.Value);
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] TransactionRequest request)
    {
        var result = await bankService.DepositAsync(request.AccountId, request.Amount);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] TransactionRequest request)
    {
        var result = await bankService.WithdrawAsync(request.AccountId, request.Amount);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        var result = await bankService.TransferAsync(request.FromAccountId, request.ToAccountId, request.Amount);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpGet("{id:int}/transactions")]
    public async Task<IActionResult> GetAccountTransactions(int id)
    {
        var result = await bankService.GetTransactionHistoryAsync(id);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }
}