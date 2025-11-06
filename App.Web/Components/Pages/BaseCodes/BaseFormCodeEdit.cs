using App.Application;
using App.Application.Bases; 
using App.Application.Interfaces; 
using App.Application.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace App.Web.Components.Pages.BaseCodes
{
    public class BaseFormCodeEdit<TCommandEdit, TDTO> : BaseFormCode where TCommandEdit : BaseEditCommand where TDTO : IDTO
    {
        [Parameter] public Guid _keyId { get; set; }
        [Parameter] public TDTO? _currentModel { get; set; }
        [SupplyParameterFromForm] public string? _actionType { get; set; }
        [CascadingParameter] protected EditContext? _editContext { get; set; }
        protected TCommandEdit?  _commandMain;

        protected virtual void CreateCommand()
        {
            throw new NotImplementedException("Not NotImplemented BaseFormCodeEdit.CreateCommand");
        }
        protected override async Task OnInitializedAsync()
        {
            CreateCommand();
            if (await CheckAccess(_commandMain.AccessParent, _commandMain.AccessChilds))
                if (_httpContext is null)
                {
                    await InitializedBefor();
                    await InitializedAfter(null);
                    await Loading(false);
                }
            await Loading(false);
        }

        protected async void Submit()
        {
            switch (_actionType)
            {
                case "Save": await Save(); break;
                case "SaveReturn": await SaveReturn(); break;
                case "Return": Return(); break;
                default:
                    break;
            }
        }


        protected void SetActionType(string? action)
        {
            _actionType = action;
        }
        protected async virtual Task Save(bool isReturn = false)
        {
            if (!_isAccess)
            {
                _msgErrors = Resource_Message.DoNotAccess.ToNewList();
                return;
            }
            _msgSuccess = null;
            _msgErrors = null;
            await Loading(true);
            SetDefaultValue();
            SetUpdateFields();
            AddUpdateFields();
            if (ControlCommand())
            {
                var res = (await _mediator.Send(_commandMain, _cancellationToken.Token)) as EditReturn;
                if (res.Result)
                {
                    if (isReturn)
                        Return();
                    else
                    {
                        _msgSuccess = Resource_Message.EditSuccess;
                        await InitializedAfter(null);
                    }
                }
                else
                    _msgErrors = res.Errors;
            }
            await Loading(false);
        }

        protected void Return()
        { 
            var route = _navigationManager.Uri.Replace(_navigationManager.BaseUri, "").Split('/')[0];  
            _navigationManager?.NavigateTo($"{route}s");
        }
        private async Task SaveReturn()
        {
            await Save(true);
        }
        #region Virtual   

        protected virtual void SetUpdateFields()
        {
            _commandMain.UpdateFields = App.Application.Utilities.TextHelper.GetCompaireObject(_commandMain, _currentModel);
        }
        protected virtual void AddUpdateFields()
        {
             
        }
        protected virtual bool ControlCommand()
        {
            if (!StaticClass.AppIsDebug)
            {
                if (_commandMain.UpdateFields == null || _commandMain.UpdateFields.Count == 0)
                {
                    _msgErrors = "Parameter_For_Edit_NotFound".ToNewList();
                    return false;
                }
            }
            return true;
        }

        #endregion

    }

}
