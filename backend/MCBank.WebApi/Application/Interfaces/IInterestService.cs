using MCBank.WebApi.Core.Common;

namespace MCBank.WebApi.Application.Interfaces;

public interface IInterestService
{
    Task ApplyInterestAsync();
}