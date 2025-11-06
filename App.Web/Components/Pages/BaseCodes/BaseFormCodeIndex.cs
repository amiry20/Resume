using App.Application.Bases;
using App.Application.Utilities;
using Microsoft.AspNetCore.Components;
using App.Application.Interfaces;

using Microsoft.JSInterop;
using App.Web.Base;
using DocumentFormat.OpenXml.Packaging;
namespace App.Web.Components.Pages.BaseCodes
{
    public class BaseFormCodeIndex<TDTO, TQuery> : BaseFormCode where TDTO : IDTO where TQuery : BaseQuery
    {
        #region Declares

        protected SelectReturn<List<TDTO>> mdlReturn = new SelectReturn<List<TDTO>>();
        protected TQuery? mdlQuery; 
        protected List<EventCallModel>? _mdlEventCalls { get; set; }
         


        #endregion

        #region Export

        protected void ExportPDF()
        {

        }

        protected void ExportEcxel()
        {
            var destination = @"F:\122\ws1.xlsx";
            using (var workbook = SpreadsheetDocument.Create(destination, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();

                workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
                workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();
                var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                uint sheetId = 1;
                if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                {
                    sheetId =
                        sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }

                DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = typeof(TDTO).Name };
                sheets.Append(sheet);

                DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                foreach (var item in mdlReturn.Structs.Where(x => x.IsVisible))
                {
                    DocumentFormat.OpenXml.Spreadsheet.Cell cell1 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                    cell1.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                    cell1.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(item.Title);
                    headerRow.AppendChild(cell1);
                }

                sheetData.AppendChild(headerRow);
                if (mdlReturn.Data != null)
                {
                    var props = typeof(TDTO).GetProperties();
                    foreach (var dsrow in mdlReturn.Data)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                        foreach (var item in mdlReturn.Structs.Where(x => x.IsVisible))
                        {
                            var prop = props.Where(x => x.Name.Equals(item.Name)).FirstOrDefault();
                            if (prop == null) continue;
                            var propValue = prop.GetValue(dsrow)?.ToString();

                            if (propValue == null) propValue = string.Empty;
                            if (! propValue.IsNullEmpty() && prop.PropertyType.IsNullableEnum())
                                propValue = _iLanguageHelper.GetText(propValue);
                            else if (prop.PropertyType == typeof(DateTime?) && ! propValue.IsNullEmpty())
                            {
                                if (item.IsOnlyDate)
                                    propValue = _iAppInfo.GetDate(DateTime.Parse(propValue));
                                else
                                    propValue = _iAppInfo.GetDateTime(DateTime.Parse(propValue)) ;
                            }
                            else if (item.IsSeprator)
                                propValue = propValue.ToLongString(0, "#,#");

                            DocumentFormat.OpenXml.Spreadsheet.Cell cell14 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell14.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cell14.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(propValue);
                            newRow.AppendChild(cell14);
                        }
                        sheetData.AppendChild(newRow);
                    }
                }
            }
        }


        #endregion

        #region Method

        private void CreateQuery()
        {
            if (mdlQuery == null)
            {
                var mdlQuery1 = Activator.CreateInstance(typeof(TQuery));
                mdlQuery = mdlQuery1 as TQuery;

                mdlQuery.Paging = new PageModel();
                mdlQuery.Order = GetOrderQuery();
                mdlQuery.Filter = null;
                mdlQuery.Filters = null;
                mdlQuery.IsStruct = true;
                //  TxtFilter = null;
            }
        }
        protected async Task Reload()
        {
            await Loading();
            CreateQuery();
            await GetList();
        }
        protected async Task GetList()
        {
            if (!_isAccess)
            {
                _msgErrors = Resource_Message.DoNotAccess.ToNewList();
                return;
            }
            _msgErrors = null;
            try
            {
                if (_mediator == null)
                {
                    _msgErrors = _iLanguageHelper.GetText("ServiceNotFound", nameof(_mediator)).ToNewList();
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
            if (!sorted.Equals("None"))
            {
                mdlQuery.Order = new OrderModel(sorted, asc);
                mdlQuery.Paging = new PageModel();
                await GetList();
            }
        }
        protected async void Filter(string filtettext)
        {
            if (mdlQuery != null)
            {
                if (filtettext.IsNullEmpty())
                    mdlQuery.Filter = null;
                else
                    mdlQuery.Filter = filtettext;
                await GetList();
            }
        }
        protected async void SearchColumn(string search)
        {
            var arr = search.Split(';');
            if (arr != null && arr.Length == 2)
            {
                await GetList();
            }
        }
        protected EventCallModel GetDeleteEventCall()
        {
            return new EventCallModel() { Ordered = 1, Title = "Delete", Class = "btn btn-danger", EventCallback = EventCallback.Factory.Create<object>(this, DeleteQuestion) };
        }
        protected EventCallModel GetEditEventCall()
        {
            return new EventCallModel() { Ordered = 2, Title = "Edit", Class = "btn btn-primary", EventCallback = EventCallback.Factory.Create<object>(this, Edit) };
        }
        protected EventCallModel GetDataLogEventCall()
        {
            return new EventCallModel() { Ordered = 3, Title = "DataLogs", Class = "btn btn-info", EventCallback = EventCallback.Factory.Create<object>(this, DataLogs) };
        }
        protected EventCallModel GetDetailEventCall()
        {
            return new EventCallModel() { Ordered = 4, Title = "Detail", Class = "btn btn-warning", EventCallback = EventCallback.Factory.Create<object>(this, Detail) };
        }

        protected async Task<List<EventCallModel>?> SetEventCallDefault()
        {
            var mdls = new List<EventCallModel>();
            if (await IsAccess(mdlQuery.AccessParent, eAccessChild.Delete))
                mdls.Add(GetDeleteEventCall());
            if (await IsAccess(mdlQuery.AccessParent, eAccessChild.Edit))
                mdls.Add(GetEditEventCall());
            if (await IsAccess(mdlQuery.AccessParent, eAccessChild.DataLog))
                mdls.Add(GetDataLogEventCall());
            return mdls;
        }
        #endregion


        #region Events


        protected override async Task OnInitializedAsync()
        {
            CreateQuery();
            if (await CheckAccess(mdlQuery.AccessParent, mdlQuery.AccessChilds))
            {
                _currentRoute = Path.GetFileName(_navigationManager.Uri.ToString());
                _currentDomin = Path.GetFileName(_navigationManager.BaseUri.ToString());
                if (_httpContext is null)
                {
                    await InitializedBefor();
                    await Reload();
                    await InitializedAfter(null);
                }
                SetEventCallModels();
            }
            await Loading(false);
        }
        protected async void Add()
        {
            if (await CheckAccess(mdlQuery.AccessParent, eAccessChild.Add))
            {
                _navigationManager.NavigateTo($"{_currentRoute}/Add".Replace("s/Add", "/Add"));
            }
        }
        protected async void Edit(object mdl)
        {
            if (CheckEditAccess((TDTO)mdl) && await CheckAccess(mdlQuery.AccessParent, eAccessChild.Edit))
            {
                var key = _iStateContainer.SetObject(mdl);
                _navigationManager.NavigateTo($"{_currentRoute}/Edit/{key}".Replace("s/Edit", "/Edit"));
            }
        }
        protected async void DataLogs(object mdl)
        {
            //string myurl = "http://localhost:5106/DataLogs/Places/f3a45c8c-80eb-4b2f-9c0c-32e6e855fd5d"; // ensure this is a valid URL
            //await _iJSRuntime.InvokeVoidAsync("open", myurl, "_blank");
            if (await CheckAccess(mdlQuery.AccessParent, eAccessChild.DataLog))
            {
                var key = _iStateContainer.SetObject(mdl);
                string url = $"{_currentDomin}DataLogs/{_currentRoute}/{key}";
                await _iJSRuntime.InvokeAsync<object>("open", url, "_blank");
            }
        }
    
        protected async void Delete(object? obj)
        {
            var dto = (TDTO)obj;
            if (dto == null)
            {
                _msgErrors = Resource_Message.NotFoundInfo.ToNewList();
                return;
            }
            if (CheckDeleteAccess(dto) && await CheckAccess(mdlQuery.AccessParent, eAccessChild.Delete))
            {
                await Loading();
                _msgErrors = null;
                try
                {
                    var command = GetDeleteCommand(dto);
                    var res = (await _mediator.Send(command, _cancellationToken.Token)) as DeleteReturn;
                    if (!res.Result)
                    {
                        _msgErrors = res.Errors;
                        await Loading(false);
                        return;
                    }
                    await GetList();
                    _alertDialog.ShowSuccess("DeleteSuccess");
                }
                catch (Exception ex)
                {
                    _iAppLogger.Error(ex, this.GetType().Name);
                    _msgErrors = ex.Message.ToNewList();
                }
                await Loading(false);
            }
        }


        protected async void DeleteQuestion(object? mdl)
        {
            var dto = (TDTO)mdl;
            if (CheckDeleteAccess(dto) && await CheckAccess(mdlQuery.AccessParent, eAccessChild.Delete))
                _deleteDialog?.Show(mdl, GetDeleteQuestion(dto));
        }

        protected async void Detail(object? mdl)
        {
            if (await CheckAccess(mdlQuery.AccessParent, eAccessChild.Detail))
            {
                var dto = (TDTO)mdl;
                var lst = await GetDetail(dto);
                _detailDialog?.Show(lst);
            }
        }

        #endregion

        #region Virtual
        protected virtual bool CheckEditAccess(TDTO? dto)
        {
            return true;
        }
        protected virtual bool CheckDeleteAccess(TDTO? dto)
        {
            return true;
        }

        protected virtual string[]? GetDeleteQuestion(TDTO? dto)
        {
            throw new NotImplementedException("Not NotImplemented BaseFormCodeIndex.GetDeleteQuestion");
        }

        protected virtual Task<string[]?> GetDetail(TDTO? dto)
        {
            throw new NotImplementedException("Not NotImplemented BaseFormCodeIndex.GetDetail");
        }

        protected virtual BaseDeleteCommand GetDeleteCommand(TDTO? dto)
        { 
            throw new NotImplementedException("Not NotImplemented BaseFormCodeIndex.GetDeleteCommand");
        }
        protected virtual async void SetEventCallModels()
        {
            _mdlEventCalls = await SetEventCallDefault();
        }

        #endregion

    }
}
