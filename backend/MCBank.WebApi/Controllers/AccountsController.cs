using MCBank.WebApi.Application;
using MCBank.WebApi.Core;
using MCBank.WebApi.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MCBank.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController(BankService bankService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAccounts()
    {
        return Ok(bankService.GetAccounts());
    }

    [HttpPost]
    public IActionResult CreateAccount()
    {
        var account = new BankAccount();
        bankService.AddAccount(account);
        return CreatedAtAction(nameof(GetAccounts), new { id = account.Guid }, account);
    }

    [HttpPost("deposit")]
    public IActionResult Deposit([FromBody] TransactionRequest request)
    {
        var account = bankService.GetAccounts().FirstOrDefault(acc => acc.Guid == request.AccountId);
        if (account == null)
        {
            return NotFound();
        }

        var result = bankService.Deposit(account, request.Amount);

        if (!result)
        {
            return BadRequest("Ошибка: Сумма меньше или равна 0");
        }

        return Ok(account);
    }

    [HttpPost("withdraw")]
    public IActionResult Withdraw([FromBody] TransactionRequest request)
    {
        var account = bankService.GetAccounts().FirstOrDefault(acc => acc.Guid == request.AccountId);
        if (account == null)
        {
            return NotFound("");
        }

        var result = bankService.Withdraw(account, request.Amount);

        if (!result)
        {
            return BadRequest("Ошибка: Сумма меньше или равна 0 или недостаточно средств");
        }

        return Ok(account);
    }
}