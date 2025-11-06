using App.Application.Bases;
using App.Application.DTOs;
using App.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace App.Application.Interfaces
{
    public interface IBaseService<T> where T : IDTO
    {
        // Task<List<T>> GetEnables1();
        //Task<List<T>> GetEnable2(Func<T, object> select, Func<T, object> update, Expression<Func<T, bool>> where);
        // Task<List<T>> Gets2(Expression<Func<T, bool>>? where = null, params string[] includs);
        // Task<List<TResult>> Gets3<TResult>(Expression<Func<T, bool>> where, params string[] includs);

        Task<ResponseRepository> Gets(BaseParameterModel<T>? parameterModel = null);
        Task<ResponseRepository> Get(BaseParameterModel<T>? parameterModel = null);
        Task<List<AccessModel>> GetAccess();

        //Task<T> Get(Expression<Func<T, bool>> where, params string[] includs);
        Task<(bool, string)> AddUpdate(T mdl, bool isUpdate, Func<T, object> updateFields, Expression<Func<T, bool>> where);
        Task<(bool, string)> Add(T mdl, bool isCommit = false);
        Task<(bool, string)> Adds(List<T> mdls, bool isCommit = false);
        Task<ResponseRepository> Update(T mdl, Expression<Func<T, object>> updateFields, bool isCommit = false);
        Task<ResponseRepository> Delete(T mdl, bool isCommit = false);
        Task<(int, string)> Deletes(Expression<Func<T, bool>> where);
        Task<ResponseRepository> IsCanDelete(string tableName, long id);
        Task<int> Commit();
    }

}
