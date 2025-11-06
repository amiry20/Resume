

using App.Application.Bases;
using App.Application.Interfaces;

namespace App.Application.Utilities
{
    public interface ILanguageHelper
    {
        public string? GetText(string? code, char spliter = '_', params string[]? values);

        public string? GetText(string? code, params string[]? values);
        public string? GetAppName();
    }
    public class LanguageHelper : ILanguageHelper
    {
        int languageId = 1;
        public LanguageHelper(IIOC iOC)
        {
            var session = iOC.CreateObject<ISessionInfo>();
            if (session != null && session.UserInfo != null)
                languageId = session.LanguageId;
        }


        public string? GetText(string? code, params string[]? values)
        {
            return GetText(code, '_', values);
        }
        public string? GetText(string? code, char spliter = '_', params string[]? values)
        {
            if (code.IsNullEmpty()) return string.Empty;

            if (StaticClass.AppLanguages == null || StaticClass.AppLanguages.Count == 0)
                return code;

            var result = string.Empty;
            var txts = code.Split(spliter);
            foreach (var txt in txts)
            {
                var res = StaticClass.AppLanguages
                    .Where(x => x.Name == txt.ToLower() && x.LanguageId == languageId)
                    .Select(s => s.Title).FirstOrDefault();

                if (res.IsNullEmpty())
                {
                    if (txts.Length < 2)
                        res = $"NF({code})";
                    else res = txt;
                }

                if (!result.IsNullEmpty())
                    result += " ";
                result += res;
            }

            if (values != null)
                result += string.Join(',', values);
            return result;
        }

        public string? GetAppName()
        {
            return GetText("AppName");
        }

    }
}
