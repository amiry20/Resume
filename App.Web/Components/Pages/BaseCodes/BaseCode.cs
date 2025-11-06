using App.Application.Interfaces;
using App.Application.Utilities;
using MediatR;
using Microsoft.AspNetCore.Components; 

namespace App.Web.Components.Pages.BaseCodes
{
    public class BaseCode : LayoutComponentBase
    {
        #region Declares 


        [Inject] protected IAppLogger? _iAppLogger { get; set; }
        [Inject] protected IMediator? _mediator { get; set; }
        [Inject] protected ILanguageHelper? _iLanguageHelper { get; set; }
        protected CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        protected bool _isLoading = true; 

        #endregion

        #region Method
       
        protected async Task Loading(bool isLoad = true)
        {
            _isLoading = isLoad;
            if (isLoad)
                await Task.Delay(1);
            else
                StateHasChanged();

        }
       
        #endregion



          
    }


}
