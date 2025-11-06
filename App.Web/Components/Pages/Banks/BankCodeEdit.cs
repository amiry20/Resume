using App.Application.Bases;
using App.Application.Commands;
using App.Application.DTOs;

using App.Application.Utilities;
using App.Web.Components.Pages.BaseCodes; 
using Microsoft.AspNetCore.Components.Forms; 

namespace App.Web.Components.Pages.Banks
{
    public class BankCodeEdit : BaseFormCodeEdit<BankEditCommand, BankDTO>
    {  
        protected override void CreateCommand()
        {
            this._commandMain = new BankEditCommand();
        }
        protected override void Clear()
        {
        } 
        protected async override Task InitializedAfter(object? afterSaveData)
        { 
            _currentModel = _iStateContainer?.GetObject(_keyId) as BankDTO;

            if (_currentModel != null)
            {
                _commandMain = new BankEditCommand()
                {
                    UpdateFields = null,
                    Id = _currentModel.Id, 
                    Name = _currentModel.Name,
                    IsEnable = _currentModel.IsEnable.ToBool(), 
                }; 
            }
            if (_commandMain != null)
                _editContext = new EditContext(_commandMain);
        } 
    }
}