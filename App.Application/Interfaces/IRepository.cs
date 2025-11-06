using System.Linq.Expressions;
using App.Application.Bases;
using App.Application.Classes;
using App.Domain.Bases;

namespace App.Application.Interfaces
{
    public interface IRepository<TEntity> : IDisposable where TEntity : BaseEntity
    {

        #region Add-Update-Delete-Commit
        void AddEntity(object model);
        void AddEntitys(List<object> model);

        void UpdateEntity(object model, List<string> fields);
        void ExistEntity(TEntity model);
        void DeleteEntity(object model);
        void DeleteEntitys(List<object>? models);
        Task<int> DeleteEntitys(Expression<Func<TEntity, bool>> where);
        Task<bool> AddUpdateEntity(TEntity entity, bool isUpdate, List<string> fields, Expression<Func<TEntity, bool>> where);
        Task<ResponseRepository> Commit(CommitConfig commitConfig, CancellationToken cancellationToken);
        #endregion

        #region Read 
        Task<ResponseRepository> Gets(BaseParameterModel<TEntity>? parameterModel, CancellationToken cancellationToken);
        Task<ResponseRepository> Get(BaseParameterModel<TEntity>? parameterModel);

        Task<ResponseRepository> RunSP(string query, params string[] parameters);

        #endregion

        #region Other 
        Task<ResponseRepository> IsCanDelete(string tableName, long id);

        #endregion
    }

}
