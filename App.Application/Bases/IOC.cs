using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Bases
{
    public class IOC : IIOC
    {
        private IServiceProvider _serviceProvider; 

        public IOC(IServiceProvider scope)
        {
            _serviceProvider = scope;
        } 
        public T CreateObject<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }
        public object CreateObject(Type type)
        {
            return _serviceProvider.GetRequiredService(type);
        } 
    }


}
