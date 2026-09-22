using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Application.Interfaces;
using MCBank.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MCBank.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController(IAccountService accountService, ISavingsPlanService savingsPlanService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("{accountId:int}")]
    public async Task<IActionResult> GetAccountById(int accountId)
    {
        var result = await accountService.GetAccountByIdAsync(accountId, CurrentUserId);

        return result.ToActionResult();
    }

    [HttpGet("iban/{iban}")]
    public async Task<IActionResult> GetAccountIdByIban(string iban)
    {
        var result = await accountService.GetAccountIdByIbanAsync(iban);
        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        var result = await accountService.GetAllAccountsAsync(CurrentUserId);

        return result.ToActionResult();
    }

    [HttpGet("plans")]
    public async Task<IActionResult> GetSavingsPlans()
    {
        var result = await savingsPlanService.GetSavingsPlansAsync();

        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        var result = await accountService.CreateAccountAsync(CurrentUserId, request.AccountType, request.TermMonths);

        if (result.IsFailure)
            return result.ToActionResult();

        return CreatedAtAction(nameof(GetAccountById), new { accountId = result.Value.Id }, result.Value);
    }

    [HttpDelete("{accountId:int}")]
    public async Task<IActionResult> DeleteAccountById(int accountId)
    {
        var result = await accountService.DeleteAccount(accountId, CurrentUserId);

        return result.ToActionResult();
    }
}