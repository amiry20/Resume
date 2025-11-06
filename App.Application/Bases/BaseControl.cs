using App.Application.DTOs;
using App.Application.Interfaces;
using App.Application.Utilities;
using App.Domain.Bases;
using System.Reflection;

namespace App.Application.Bases
{
    public abstract class BaseControl<TEntity> where TEntity : BaseEntity
    {
        readonly protected IIOC _iIOC;
        protected List<string> _errors;
        protected Lazy<IRepository<TEntity>> _iRepository;
        public BaseControl(IIOC iIOC)
        {
            _iIOC = iIOC;
            _errors = new List<string>();
            _iRepository = new Lazy<IRepository<TEntity>>(() => _iIOC.CreateObject<IRepository<TEntity>>());
        }
        protected virtual async Task<BaseControlModel> ControlBeforeUpdate(TEntity? entity, List<string> updateFields)
        {
            return new BaseControlModel();
        }
        protected virtual async Task<BaseControlModel> ControlBeforeInsert(TEntity? entity)
        {
            return new BaseControlModel();
        }
        protected virtual async Task<BaseControlModel> ControlAfterUpdate(TEntity? entity, List<string> updateFields)
        {
            return new BaseControlModel();
        }
        protected virtual async Task<BaseControlModel> ControlAfterInsert(TEntity? entity)
        {
            return new BaseControlModel();
        }
    }
    public static class ControlHelper
    {
        private static Assembly? assembly;

        private static Dictionary<string, List<MethodInfo>> controlMethods = new Dictionary<string, List<MethodInfo>>();
        private static Assembly GetAssembly()
        {
            if (assembly == null)
                assembly = typeof(ControlHelper).Assembly;
            return assembly;
        }
        private static object? GetClass(string name, IIOC iIOC)
        {
            name = $"{name}Control";

            var tControl = GetAssembly().GetTypes().Where(x => x.Namespace == "App.Application.Controls" && x.Name == name).FirstOrDefault();

            if (tControl == null) return null;

            return Activator.CreateInstance(tControl, iIOC);
        }
        private static List<MethodInfo>? GetMethods(Type type, string nameMethod)
        {
            List<MethodInfo>? results = null;

            var key = ($"{type.Name}_{nameMethod}").ToLower();
            controlMethods.TryGetValue(key, out results);

            if (results != null) return results;

            results = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.Name == nameMethod).ToList();
            if (results == null || results.Count() == 0) return null;

            controlMethods.TryAdd(key, results);
            return results;
        }
        public static async Task<BaseControlModel> Control(string typeName, IIOC iIOC, object? entity, EntityActionMode entityActionMode, List<string> updateFields, bool isBefore)
        {
            if (entity == null)
                return new BaseControlModel(Resource_Message.ModelIsEmpty);

            var classType = GetClass(typeName, iIOC);
            if (classType == null)
                return new BaseControlModel(Resource_Message.NotFoundService.ByParameters($"{typeName}_Type_Control"));

            var nameMethod = $"Control{(isBefore ? "Before" : "After")}{entityActionMode}";

            var methods = GetMethods(classType.GetType(), nameMethod);

            if (methods == null || methods.Count == 0)
                return new BaseControlModel(Resource_Message.NotFoundMethod.ByParameters($"{typeName}_Type_Control"));

            var errors = new List<string>();
            foreach (var method in methods)
            {
                BaseControlModel result = null;
                if (entityActionMode == EntityActionMode.Insert)
                    result = await (method.Invoke(classType, new object[] { entity }) as Task<BaseControlModel>);
                else if (entityActionMode == EntityActionMode.Update)
                    result = await (method.Invoke(classType, new object[] { entity, updateFields }) as Task<BaseControlModel>);

                if (result == null)
                    errors.Add(Resource_Message.NotFoundInfo.ByParameters($"{typeName}_Result_Control"));
                else if (!result.Result)
                    errors.AddRange(result.Errors);
            }
            return new BaseControlModel(errors);
        }
    }
}
