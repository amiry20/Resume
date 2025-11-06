using App.Application.Bases;
using App.Application.Interfaces;
using App.Application.Models;
using App.Application.Utilities;
using MediatR;
using Microsoft.AspNetCore.Components;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using App.Application;
using App.Application.DTOs;
using App.Application.Classes;

namespace App.Web.Base
{

    public class SessionInfo : ISessionInfo
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly IMediator _mediator;
        readonly IIOC iIoc;
        public SessionInfo(IHttpContextAccessor httpcontextAccessor, IIOC ioc)
        {
            iIoc = ioc;
            _httpContextAccessor = httpcontextAccessor;
            //_mediator = mediator;
            SetUserInfo();
            SetUserMenuInfo();
        }

        private async void SetUserInfo()
        {
            if (UserInfo == null)
            {
                var keyId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!keyId.IsNullEmpty())
                {
                    var languageId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.HomePhone);
                    LanguageId = languageId.ToInt();

                    UserInfo? mdl;
                    StaticClass.AppUsers.TryGetValue(keyId, out mdl);
                    UserInfo = mdl;

                    if (StaticClass.AppIsDebug && UserInfo == null)
                    {
                        var iAppInit = iIoc.CreateObject<IAppInit>();
                        await iAppInit.SetSessionInfo(Guid.Parse(keyId));
                    }
                }
            }
        }
        private void SetUserMenuInfo()
        {
            if (UserInfo != null)
            {
                if (UserInfo?.KeyId != null)
                {
                    List<MenuDTO>? mdl;
                    StaticClass.AppUserMenus.TryGetValue(UserInfo.KeyId.ToString(), out mdl);
                    UserMenuInfo = mdl;
                }
            }
        }
        public RequestInfo? GetRequestInfo()
        {
            if (_httpContextAccessor != null && RequestInfo == null)
            {
                RequestInfo = new RequestInfo()
                {
                    UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToSerialize(),
                    RemoteIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                    RemotePort = _httpContextAccessor.HttpContext?.Connection.RemotePort,
                    LocalIp = _httpContextAccessor.HttpContext?.Connection.LocalIpAddress?.ToString(),
                    LocalPort = _httpContextAccessor.HttpContext?.Connection.LocalPort,
                    Host = _httpContextAccessor.HttpContext?.Request.Host.ToString(),
                    Headers = _httpContextAccessor.HttpContext?.Request.Headers.ToSerialize(),
                    Browser = _httpContextAccessor.HttpContext?.Request.Headers["sec-ch-ua"],
                    Mobile = _httpContextAccessor.HttpContext?.Request.Headers["sec-ch-ua-mobile"],
                    Platform = _httpContextAccessor.HttpContext?.Request.Headers["sec-ch-ua-platform"],
                    Scheme = _httpContextAccessor.HttpContext?.Request.Scheme,
                };
            }
            return RequestInfo;
        }

        public List<MenuDTO>? UserMenuInfo { get; set; }
        public UserInfo? UserInfo { get; set; }
        public RequestInfo? RequestInfo { get; set; }
        public YearInfo? YearInfo { get; set; }
        public int LanguageId { get; set; }
    }

    public class AppLogger : IAppLogger
    {
        private readonly IIOC iOC;
        string mess = string.Empty;
        public AppLogger(IIOC ioc)
        {
            this.iOC = ioc;
        }
        private void SetMessage()
        {
            var sessioninfo = iOC.CreateObject<ISessionInfo>();
            if (sessioninfo != null)
                mess = $"CompanyId:{sessioninfo.UserInfo?.CompanyId}_UserId:{sessioninfo.UserInfo?.Id}_UserName:{sessioninfo.UserInfo?.Name}";
        }
        public void Error(Exception exp, string description)
        {
            SetMessage();
            string stack = "StackTrace:";
            var st = new StackTrace(exp, true);
            for (int i = 0; i < 20; i++)
            {
                var frame0 = st.GetFrame(i);
                if (frame0 == null)
                    break;

                var line0 = frame0.GetFileLineNumber();
                if (!frame0.GetFileName().IsNullEmpty())
                    stack += $"{i}:L_{line0}_F:{frame0.GetFileName()}";
            }
            Log.Error(exp, "{desc} {stack} {mess}", description, stack, mess);
        }

        public void Error(string message, string description)
        {
            SetMessage();
            Log.Error(message, "Note: {desc} {mess}", description, mess);
        }

        public void Information(string message)
        {
            SetMessage();
            Log.Information(message, "Note:{mess}", mess);
        }

        public void Warning(string message)
        {
            SetMessage();
            Log.Warning(message, "Note:{mess}", mess);
        }
    }


    public class AppLoginModel
    {
        [Required(ErrorMessage = "{0}_IsRequired")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "{0}_IsRequired")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        [Required(ErrorMessage = "{0}_IsRequired")]
        public int? LanguageId { get; set; } = (int)eLanguage.Fa;
        public string? Redirect { get; set; }
        public string? Message { get; set; }
    }

    public class EventCallModel
    {
        public int Ordered { get; set; }
        public string? Title { get; set; }
        public string? Class { get; set; }
        public EventCallback<object> EventCallback { get; set; }
    }
}
