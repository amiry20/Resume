using Microsoft.AspNetCore.Components;
using App.Application.Utilities;
using App.Application.Interfaces;
using App.Application.Queries;
using App.Application.DTOs.Searchs;

using App.Application.Bases;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Authorization;
using App.Web.Components.Pages.BaseRazors; 

namespace App.Web.Components.Pages.BaseCodes
{
    public class BaseFormCode : BaseCode, IDisposable
    {
        #region Declares  
        [Inject] protected IJSRuntime? _iJSRuntime { get; set; }
        [Inject] protected ILanguageHelper? _iLanguageHelper { get; set; }
        [Inject] protected IAppInfo? _iAppInfo { get; set; }
        [Inject] protected IAccess? _iAccess { get; set; }
        [Inject] protected ISessionInfo? _iSessionInfo { get; set; }
        [Inject] protected IStateContainer? _iStateContainer { get; set; }
        [Inject] protected NavigationManager? _navigationManager { get; set; }
        [Inject] protected AuthenticationStateProvider? GetAuthenticationStateAsync { get; set; }
        [CascadingParameter] public HttpContext? _httpContext { get; set; }
        [CascadingParameter(Name = "RouteData")] public Microsoft.AspNetCore.Components.RouteData? _routeData { get; set; }
        protected string _imageSource { get; set; } = "/images/sites/noimage.png";

        protected AlertDialog? _alertDialog { get; set; }
        protected DeleteDialog? _deleteDialog { get; set; }
        protected DetailDialog? _detailDialog { get; set; }
        protected QuestionDialog? _questionDialog { get; set; }
        //public List<InsuranceCostSearchDTO>? _mdlInsuranceCostSearchs { get; set; }

        protected bool _isAccess = false;
        protected string _currentRoute = "NoSet";
        protected string _currentDomin = "NoSet";
        protected List<string>? _msgErrors;
        protected string? _msgSuccess = string.Empty;
        protected InputText? _TextForFocus;
        protected InputNumber<long?>? _NumberForFocus;

        #endregion

        #region Method

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (_TextForFocus != null && _TextForFocus.Element != null)
                {
                    await _TextForFocus.Element.Value.FocusAsync();
                }
                if (_NumberForFocus != null && _NumberForFocus.Element != null)
                {
                    await _NumberForFocus.Element.Value.FocusAsync();
                }
            }
        }

        /// <summary>
        /// _imageSource = "/images/sites/noimage.png";
        /// </summary>
        protected void SetDefaultImageSource()
        {
            _imageSource = "/images/sites/noimage.png";
        }
        protected virtual string? GetModelName()
        {
            return _iLanguageHelper.GetText(_currentRoute);
        }

        protected async Task<bool> CheckAccess(eAccessParent accessParent, params eAccessChild[] accessChild)
        {
            var checkAccess = await _iAccess.CheckAccess(accessParent, accessChild);
            if (!checkAccess.Result)
                _msgErrors = checkAccess.Errors;
            _isAccess = checkAccess.Result;
            return checkAccess.Result;
        }

        protected async Task<bool> IsAccess(eAccessParent accessParent, params eAccessChild[] accessChild)
        {
            var checkAccess = await _iAccess.CheckAccess(accessParent, accessChild);
            return checkAccess.Result;
        }


        //public static byte[] GenerateExcelWorkbook1(List<object> list)
        //{
        //    var wb = new XLWorkbook();
        //    wb.Properties.Author = "the Author";
        //    wb.Properties.Title = "the Title";
        //    wb.Properties.Subject = "the Subject";

        //    var ws = wb.Worksheets.Add("Weather Forecast");

        //    ws.Cell(1, 1).Value = "Temp. (C)";
        //    ws.Cell(1, 2).Value = "Temp. (F)";
        //    ws.Cell(1, 3).Value = "Summary";

        //    for (int row = 0; row < data.Length; row++)
        //    {
        //        ws.Cell(row + 1, 1).Value = data[row].TemperatureC;
        //        ws.Cell(row + 1, 2).Value = data[row].TemperatureF;
        //        ws.Cell(row + 1, 3).Value = data[row].Summary;
        //    }

        //    MemoryStream XLSStream = new();
        //    wb.SaveAs(XLSStream);
        //}
        //public static byte[] GenerateExcelWorkbook(List<object> list)
        //{

        //    var stream = new MemoryStream();

        //    // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    using (var package = new ExcelPackage(stream))
        //    {
        //        var workSheet = package.Workbook.Worksheets.Add("Sheet1");

        //        // simple way
        //        workSheet.Cells.LoadFromCollection(list, true);

        //        ////// mutual
        //        ////workSheet.Row(1).Height = 20;
        //        ////workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        ////workSheet.Row(1).Style.Font.Bold = true;
        //        ////workSheet.Cells[1, 1].Value = "No";
        //        ////workSheet.Cells[1, 2].Value = "Name";
        //        ////workSheet.Cells[1, 3].Value = "Age";

        //        ////int recordIndex = 2;
        //        ////foreach (var item in list)
        //        ////{
        //        ////    workSheet.Cells[recordIndex, 1].Value = (recordIndex - 1).ToString();
        //        ////    workSheet.Cells[recordIndex, 2].Value = item.UserName;
        //        ////    workSheet.Cells[recordIndex, 3].Value = item.Age;
        //        ////    recordIndex++;
        //        ////}

        //        return package.GetAsByteArray();
        //    }
        //}
   
  

        #endregion



        #region Virtual 
        protected virtual void SetDefaultValue()
        {

        }
        protected virtual async Task InitializedBefor()
        {
        }
        protected virtual async Task InitializedAfter(object? afterSaveData)
        {
        }
        protected virtual OrderModel? GetOrderQuery()
        {
            return null;
        }
        protected virtual void Clear()
        {
        }

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
