
using App.Application.Bases;
using App.Application.Interfaces;
using App.Application.Utilities;
using Microsoft.EntityFrameworkCore.Storage;

namespace App.Application.Classes
{

    public class CommitConfig
    {
        public bool? IsReturnData { get; set; }
        public bool? IsDisposeTransaction { get; set; }
        public IDbContextTransaction? Transaction { get; set; }
        public CommitConfig(bool isdisposetransaction = true, bool isReturnData = false)
        {
            IsDisposeTransaction = isdisposetransaction;
            IsReturnData = isReturnData;
        }
        public CommitConfig(bool isdisposetransaction, IDbContextTransaction? transaction)
        {
            Transaction = transaction;
            IsDisposeTransaction = isdisposetransaction;
        }
    }

    public class AppInfo : IAppInfo
    {
        readonly int languageId = 1;
        public AppInfo(IIOC iOC)
        {
            var iSessionInfo = iOC.CreateObject<ISessionInfo>();
            if (iSessionInfo != null && iSessionInfo.UserInfo != null)
                languageId = iSessionInfo.LanguageId;
        }
        public string GetCurrentDateTime()
        {
            if (languageId == 1)
                return DateTime.Now.ToShamsiDateTime();
            return DateTime.Now.ToString("yyy/MM/dd HH:mm:ss");
        }

        public string GetCurrentDate()
        {
            if (languageId == 1)
                return DateTime.Now.ToShamsiDate();
            return DateTime.Now.ToString("yyy/MM/dd");
        }
        public List<string>? GetVersions()
        {
            var result = new List<string>();
            result.Add($"App Version : {App.Config.ConfigHelper.Version}");
            result.Add($"App Mode : {(StaticClass.AppIsDebug ? "Debug" : "Relase")}");
            result.Add($"MachineName : {Environment.MachineName}");
            result.Add($"User Count : {StaticClass.AppUsers.Count.ToString("#,0")}");
            result.Add($"Dictionary Count : {StaticClass.AppLanguages.Count.ToString("#,0")}");
            result.Add($"Language : {(eLanguage)languageId}");
            result.Add($"DB Name: {App.Config.ConfigHelper.GetConnectionVersion("DB")}");
            result.Add($"Log DB : {App.Config.ConfigHelper.GetConnectionVersion("LogDB")}"); 

            return result;
        }

        public string? GetDateTime(DateTime? dateTime)
        {
            if (languageId == 1)
                return dateTime.ToShamsiDateTimeNull();
            return dateTime?.ToString("yyy/MM/dd HH:mm:ss");
        }
        public string? GetDate(DateTime? dateTime)
        {
            if (languageId == 1)
                return dateTime.ToShamsiDateNull();
            return dateTime?.ToString("yyy/MM/dd");
        }

    }
    public class AppSetting : IAppSetting
    {
        public AppSetting()
        {
        }
        public string? GetValue(string name)
        {
            if (StaticClass.AppSettingDTOs == null || StaticClass.AppSettingDTOs.Count == 0)
                return null;

            var mdl = StaticClass.AppSettingDTOs.Where(x => x.Name == name).FirstOrDefault();
            return mdl?.Value;
        }
    }

    public class PersianCalendarModel
    {
        public DateTime CurentDate { get; set; }
        public string? PersianDate { get; set; }
        public string? PersianYear { get; set; }
        public string? PersianMounth { get; set; }
        public string? PersianDay { get; set; }
        public bool? IsCurrent { get; set; }
        public bool? IsFriday { get; set; }
        public bool? IsClose { get; set; }
    }
}
