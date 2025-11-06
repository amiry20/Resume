using App.Application.Bases;
using App.Application.Commands;
using App.Application.DTOs;
using App.Application.Queries;
using App.Web.Components.Pages.BaseCodes;

namespace App.Web.Components.Pages.Banks
{
    public class BankCodeIndex : BaseFormCodeIndex<BankDTO, BankQuery>
    { 
        protected override string[]? GetDeleteQuestion(BankDTO? dto)
        {
            var result = new List<string?>();
            result.Add(dto?.Name.ToString()); 
            return result.ToArray();
        }
        
        protected override BaseDeleteCommand GetDeleteCommand(BankDTO? dto)
        {
            return new BankDeleteCommand(dto?.Id);
        }

    }
}