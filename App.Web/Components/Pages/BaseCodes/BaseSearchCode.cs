using App.Application.Bases;
using App.Application.Utilities;
using Microsoft.AspNetCore.Components;
using App.Application.Interfaces;


namespace App.Web.Components.Pages.BaseCodes
{
    public class BaseSearchCode<TDTO, TQuery> : BaseCode, IDisposable where TDTO : IDTO where TQuery : BaseQuery
    {
        #region Declares 

        protected List<string>? _msgErrors;
        protected SelectReturn<List<TDTO>> mdlReturn = new SelectReturn<List<TDTO>>();
        protected TQuery? mdlQuery;
        protected TDTO? mdlCurrent;

        protected string? TxtFilter { get; set; }


        #endregion

        #region Method

        protected async Task GetList()
        {
            _msgErrors = null;
            try
            {
                if (_mediator == null)
                {
                    _msgErrors =  _iLanguageHelper.GetText("ServiceNotFound", nameof(_mediator)).ToNewList();
                    await Loading(false);
                    return;
                }
                if (mdlQuery == null)
                {
                    _msgErrors = _iLanguageHelper.GetText("ServiceNotFound", nameof(mdlQuery)).ToNewList();
                    await Loading(false);
                    return;
                }
                var res = await _mediator.Send(mdlQuery, _cancellationToken.Token);
                if (res != null)
                    mdlReturn = (SelectReturn<List<TDTO>>)res;

                if (mdlReturn == null || !mdlReturn.Result)
                {
                    _msgErrors = mdlReturn?.Errors;
                    await Loading(false);
                }
            }
            catch (Exception ex)
            {
                _iAppLogger.Error(ex, this.GetType().Name);
                _msgErrors = ex.Message.ToNewList();
            }
            await Loading(false);
        }

        protected async void GetPage(int page)
        {
            mdlQuery.Paging.Page = page;
            await GetList();
        }

        protected async void SelectChange(ChangeEventArgs e)
        {
            if (mdlQuery.Paging == null)
                mdlQuery.Paging = new PageModel();
            mdlQuery.Paging.Page = 1;
            mdlQuery.Paging.PageSize = e.Value.ToInt(ConstantHelper.PageSize);
            await GetList();
        }

        protected void SetOrder(string value)
        {
            var array = value.Split(',');
            if (array.Length != 2) return;
            Sorteded(array[0], array[1].ToBool(false));
        }

        protected async void Sorteded(string sorted, bool asc)
        {
            mdlQuery.Order = new OrderModel(sorted, asc);
            mdlQuery.Paging = new PageModel();
            await GetList();
        }
        protected async void Filter()
        {
            if (mdlQuery != null)
            {
                if (TxtFilter.IsNullEmpty())
                    mdlQuery.Filter = null;
                else
                    mdlQuery.Filter = TxtFilter ;
                await GetList();
            }
        }

        #endregion



        #region Virtual
       

        #endregion

        public void Dispose()
        {
            if (_cancellationToken != null)
            {
                _cancellationToken.Cancel();
                _cancellationToken.Dispose();
            }
        }
    }
}
