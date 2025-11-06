using App.Application.Bases;
using App.Application.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace App.Web.Components.Pages.BaseCodes
{
    public class BaseFormCodeAdd<TCommandAdd> : BaseFormCode where TCommandAdd : BaseAddCommand
    {
        [SupplyParameterFromForm] public string? _actionType { get; set; }
        [CascadingParameter] protected EditContext? _editContext { get; set; }
        protected TCommandAdd? _commandMain;

        protected virtual void CreateCommand()
        {
            throw new NotImplementedException("Not NotImplemented  BaseFormCodeAdd.CreateCommand");
        }
        protected override async Task OnInitializedAsync()
        {
            CreateCommand();
            if (await CheckAccess(_commandMain.AccessParent, _commandMain.AccessChilds))
                if (_httpContext is null)
                {
                    await InitializedBefor();
                    Clear();
                    await InitializedAfter(null);
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
        protected virtual async Task Save(bool isReturn = false)
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
            if (ControlCommand())
            {
                var res = (await _mediator.Send(_commandMain, _cancellationToken.Token)) as AddReturn;
                if (res.Result)
                {
                    if (isReturn)
                        Return();
                    else
                    {
                        _msgSuccess = Resource_Message.InsertSuccess;
                        await InitializedAfter(res.Data);
                    }
                    AfterSave(_commandMain);
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

        /// <summary>
        /// Control Field of Command in Presentation Layer
        /// </summary>
        /// <returns></returns>
        protected virtual bool ControlCommand()
        {
            return true;
        }
        protected virtual void AfterSave(TCommandAdd? command)
        {
        }

        #endregion
    }

}
