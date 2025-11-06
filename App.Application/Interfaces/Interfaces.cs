using App.Application.DTOs;
using App.Application.Models;

namespace App.Application.Interfaces
{
    public interface ISessionInfo
    {
        public UserInfo? UserInfo { get; set; }
        public List<MenuDTO>? UserMenuInfo { get; set; }
        public YearInfo? YearInfo { get; set; }
        public RequestInfo? RequestInfo { get; set; }
        public int LanguageId { get; set; }
        public RequestInfo? GetRequestInfo();
    }
    public interface IAppLogger
    {
        public void Error(Exception exp, string description);
        public void Error(string message, string description);
        public void Information(string message);
        public void Warning(string message);
    }

    public interface IAppInfo
    {
        public List<string>? GetVersions();
        public string GetCurrentDate();
        public string GetCurrentDateTime();
        public string? GetDateTime(DateTime? dateTime);
        public string? GetDate(DateTime? dateTime);
    }
    public interface IAppSetting
    {
        public string? GetValue(string name);
    }


    public interface IDTO
    {
    }
}
