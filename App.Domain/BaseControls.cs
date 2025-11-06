using App.Application.DTOs;
using App.Helper.Helpers; 
using System.Reflection; 

namespace App.Domain
{ 
    public interface IControl<T>
    {
        public Task<BaseControlModel> ControlBefore(T model, EntityActionMode entityActionMode, List<string> updateFields);
        public Task<BaseControlModel> ControlAfter(T model, EntityActionMode entityActionMode, List<string> updateFields);
        public Task<BaseControlModel> ControlFields(List<string> updateFields, EntityActionMode entityActionMode);
    }
    public static class BaseControl
    {
        private static Assembly assembly;

        private static Dictionary<string, object> controlClass = new Dictionary<string, object>();
        private static Assembly GetAssembly()
        {
            if (assembly == null)
                assembly = typeof(IControl<>).Assembly;
            return assembly;
        }
        private static object? GetClass(Type type)
        {
            var name = $"{type.Name.Replace("Model", "")}Control";
            object? c = null;

            controlClass.TryGetValue(name, out c);
            if (c != null) return c;

            var t = GetAssembly().GetTypes().Where(x => x.Name == name).FirstOrDefault();

            if (t == null) return null;

            c = Activator.CreateInstance(t);

            if (t == null) return null;
            controlClass.TryAdd(name, c);
            return c;
        }
        public static async Task<BaseControlModel> Control<T>(T model, EntityActionMode entityActionMode, List<string> updateFields,bool isBefore)
        {
            var c = GetClass(typeof(T));
            if (c == null) return new BaseControlModel(new List<string>() { $"Error:Not Found {typeof(T).Name} Class Type" });

            var methodName = isBefore ? "ControlBefore" : "ControlAfter";

            var methodControl = c.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance,
            null, CallingConventions.Any, new Type[] { typeof(T), typeof(EntityActionMode), typeof(List<string>) }, null);
             
            if (methodControl == null)
                return new BaseControlModel(new List<string>() { $"Error:Not Found {typeof(T).Name} Method" });

            return await (methodControl.Invoke(c, new object[] { model, entityActionMode, updateFields }) as Task<BaseControlModel>);
        }
        public static async Task<BaseControlModel> ControlFields<T>(List<string> updateFields, EntityActionMode entityActionMode)
        {
            var c = GetClass(typeof(T));
            if (c == null) return new BaseControlModel(new List<string>() { $"Error:Not Found {typeof(T).Name} Class Type" });

            var methodControl = c.GetType().GetMethod("ControlFields", BindingFlags.Public | BindingFlags.Instance,
            null, CallingConventions.Any, new Type[] { typeof(List<string>), typeof(EntityActionMode) }, null);

            if (methodControl == null)
                return new BaseControlModel(new List<string>() { $"Error:Not Found {typeof(T).Name} Method" });

            return await (methodControl.Invoke(c, new object[] { updateFields, entityActionMode }) as Task<BaseControlModel>);
        }
    }

}
