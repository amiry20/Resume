using App.Application.Commands; 
using App.Web.Components.Pages.BaseCodes;  
using Microsoft.AspNetCore.Components.Forms;

namespace App.Web.Components.Pages.Banks
{
    public class BankFormCodeAdd : BaseFormCodeAdd<BankAddCommand>
    {   
        protected override void CreateCommand()
        {
            _commandMain = new BankAddCommand();
        }
        protected override void Clear()
        { 
            _commandMain.IsEnable = true;  
        }
     
        protected async override Task InitializedAfter(object? afterSaveData)
        {
            _editContext = new EditContext(_commandMain);
        } 
    }
}