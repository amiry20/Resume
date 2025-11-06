using App.Application.Interfaces;
using App.Config;
using App.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Serilog.Sinks.MSSqlServer;
using Serilog;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace App.Web.Base
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWebServices(this IServiceCollection services)
        {



            #region HttpContextAccessor 

            services.AddHttpContextAccessor();
            services.AddScoped<HttpContextAccessor>();

            #endregion

            #region Scoped 
            services.AddScoped(typeof(ISessionInfo), typeof(SessionInfo));
            services.AddScoped(typeof(IAppLogger), typeof(AppLogger));

            #endregion

            //            services.AddDatabaseDeveloperPageExceptionFilter();

            //            services.AddScoped<IUser, CurrentUser>();

            //            services.AddHttpContextAccessor();

            //            services.AddHealthChecks()
            //                .AddDbContextCheck<ApplicationDbContext>();

            //            services.AddExceptionHandler<CustomExceptionHandler>();

            //            services.AddRazorPages();

            //            // Customise default API behaviour
            //            services.Configure<ApiBehaviorOptions>(options =>
            //                options.SuppressModelStateInvalidFilter = true);

            //            services.AddEndpointsApiExplorer();

            //            services.AddOpenApiDocument((configure, sp) =>
            //            {
            //                configure.Title = "CleanArchitecture API";

            //#if (UseApiOnly)
            //            // Add JWT
            //            configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            //            {
            //                Type = OpenApiSecuritySchemeType.ApiKey,
            //                Name = "Authorization",
            //                In = OpenApiSecurityApiKeyLocation.Header,
            //                Description = "Type into the textbox: Bearer {your JWT token}."
            //            });

            //            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            //#endif
            //            });


            #region Razor Component
            services.AddRazorComponents()
                 .AddInteractiveServerComponents();

            #endregion

            #region Serilog 

            var columnpption = new ColumnOptions();

            //columnpption.Store.Add( StandardColumn.LogEvent);
            columnpption.Id.DataType = System.Data.SqlDbType.BigInt;
            columnpption.AdditionalColumns = new Collection<SqlColumn>
{
    new SqlColumn
    {
        ColumnName = "IsDeleted",
        AllowNull = false,
        DataType = System.Data.SqlDbType.Bit,
        PropertyName = "IsDeleted",

    }
};
            //columnpption.AdditionalColumns = new Collection<SqlColumn>
            //{
            //    new SqlColumn
            //    {
            //        ColumnName = "ClientIp",
            //        AllowNull = false,
            //        DataLength = 50,
            //        DataType = System.Data.SqlDbType.NVarChar,
            //        PropertyName = "Numbers",
            //    }
            //}; 


            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(ConfigHelper.GetConnectionStringLogDB()
                , sinkOptions: new MSSqlServerSinkOptions()
                {
                    TableName = "AppLogs",
                    AutoCreateSqlDatabase = true,
                    AutoCreateSqlTable = true,

                },
                columnOptions: columnpption
                )
                 //.MinimumLevel.Information()
                 //.MinimumLevel.Error()
                 .Enrich.WithProperty("ServerName", $"{Environment.MachineName}_{System.Net.Dns.GetHostName()}")
                 .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
                .CreateLogger();
            Log.Information("Start App");
            services.AddSingleton(Log.Logger);

            #endregion



            #region Authentication 

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/Login";
                        options.LogoutPath = "/Logout";
                    });
            services.AddAuthorization();
            services.AddCascadingAuthenticationState();
            #endregion


            #region SideBlazor  Error 
            if (App.Application.StaticClass.AppIsDebug)
                services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

            #endregion



            return services;
        }
    }
}
