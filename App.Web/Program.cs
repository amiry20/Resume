using App.Application.Classes;
using App.Web.Base;
using App.Web.Components;
using App.Application;
using App.Infrastructure;

try
{

#if DEBUG
    App.Application.StaticClass.AppIsDebug = true;
#endif
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices();


#region AppInit 
var sp = builder.Services.BuildServiceProvider();
var iAppInit = sp.GetRequiredService<IAppInit>();
iAppInit.Init();
#endregion

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}


app.UseStatusCodePagesWithRedirects("/NotFound");
app.UseStaticFiles();

    #region Authentication 
    app.UseAuthentication();
    app.UseAuthorization();
    #endregion

    app.UseAntiforgery();
    app.UseHttpsRedirection();


    app.MapRazorComponents<Main>().AddInteractiveServerRenderMode();
    app.Run();

}
catch (Exception)
{ 
    throw;
}