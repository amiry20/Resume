using System.Reflection;
using App.Application.Bases;
using App.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            #region Scoped  
            services.AddScoped(typeof(IStateContainer), typeof(StateContainer));
            services.AddScoped(typeof(IIOC), typeof(IOC));

            #endregion

            #region Singleton  

            #endregion


            #region MediatR

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            #endregion




            return services;
        }

    }
}
