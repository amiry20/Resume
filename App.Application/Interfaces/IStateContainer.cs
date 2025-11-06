namespace App.Application.Interfaces
{
    public interface IStateContainer
    {
        public object? GetObject(Guid? key);
        public T? GetObject<T>(Guid? key);
        public Guid SetObject(object obj);
    }
    public class StateContainer : IStateContainer
    {
        object _stateAdd = new object();
        object _stateGet = new object();
        public object? GetObject(Guid? key)
        {
            if (key == null || key == Guid.Empty) return null;
            lock (_stateGet)
            {
                object? obj = null;
                StaticClass.AppStateContainers.TryGetValue((Guid)key, out obj);
                if (obj != null)
                    StaticClass.AppStateContainers.Remove((Guid)key);
                return obj;
            }
        }
        public T? GetObject<T>(Guid? key)
        {
            var obj = GetObject(key);
            return (T?)obj;
        }

        public Guid SetObject(object obj)
        {
            lock (_stateAdd)
            {
                var key = Guid.NewGuid();
                StaticClass.AppStateContainers.TryAdd(key, obj);
                return key;
            }
        }
    }
}
