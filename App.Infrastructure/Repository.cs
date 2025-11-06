using App.Application.Bases;
using App.Application.Classes;
using App.Application.DTOs;
using App.Application.Interfaces;
using App.Application.Utilities;
using App.Domain.Bases;
using App.Domain.DB.Model;
using App.Infrastructure.Contexts;
using App.Infrastructure.Helpers; 
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace App.Infrastructure
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        #region Heade
        private List<object>? mdlEntityActionModels;

        private readonly IIOC _iIOC;

        public Repository(IIOC iIOC)
        {
            _iIOC = iIOC;
            mdlEntityActionModels = new List<object>();
        }


        #endregion

        #region Read   
        public async Task<ResponseRepository> Get(BaseParameterModel<TEntity>? parameterModel)
        {
            using (var cx = ContextHelper.CreateContext<TEntity>(null))
            {
                var q = cx.Set<TEntity>().AsQueryable();

                if (parameterModel != null && parameterModel.Where != null)
                    q = q.Where(parameterModel.Where);
                if (parameterModel != null && parameterModel.Includs != null)
                    foreach (var item in parameterModel.Includs)
                    {
                        q = q.Include(item);
                    }
                if (parameterModel != null && parameterModel.Select != null)
                    q.Select(parameterModel.Select);

                var mdlResult = q.FirstOrDefault();
                return new ResponseRepository(mdlResult, mdlResult == null ? 0 : 1);
            }
        }
        public async Task<ResponseRepository> Gets(BaseParameterModel<TEntity>? parameterModel, CancellationToken cancellationToken)
        {
            using (var cx = ContextHelper.CreateContext<TEntity>(null))
            {
                var q = cx.Set<TEntity>().AsQueryable();

                if (parameterModel != null && parameterModel.Where != null)
                    q = q.Where(parameterModel.Where);

                int? totalcnt = null;
                if (parameterModel?.Paging != null)
                    totalcnt = await q.CountAsync(cancellationToken);

                IQueryable<object> query = q;
                IQueryable<object> query2 = q;
                IQueryable<TEntity> query1 = q;
                if (parameterModel.Order != null)
                    query1 = q.OrderByField(parameterModel.Order.ColName, parameterModel.Order.Asc);

                if (parameterModel != null && parameterModel.Select != null)
                    query2 = query1.Select(parameterModel.Select);
                else
                    query2 = query1;

                if (parameterModel != null && parameterModel.Paging != null)
                    query = query2.Skip(parameterModel.Paging.GetSkip).Take(parameterModel.Paging.PageSize);
                else
                    query = query2;

                var mdlResult = await query.ToListAsync(cancellationToken);
                return new ResponseRepository(mdlResult, totalcnt == null ? mdlResult.Count : totalcnt.ToInt());
            }
        }


        public async Task<ResponseRepository> RunSP(string query, params string[] parameters)
        {
            using (var cx = ContextHelper.CreateContext<TEntity>(null))
            {
                SqlParameter[] param;
                if (parameters != null && parameters.Length > 0)
                {
                    param = new SqlParameter[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        param[i] = new SqlParameter($"@param{(i + 1)}", parameters[i]);
                    }
                }
                else
                    param = new SqlParameter[] { };
                var d = query + param.ToString();
                var message = cx.Database.SqlQueryRaw<string>(query, param).AsEnumerable().FirstOrDefault();

                return new ResponseRepository(new List<string>() { message });
            }
        }

        #endregion


        #region Other 

        async Task<BaseControlModel> NotifyControls(List<UOWModel> mdlUOWModels, bool isBefore)
        {
            foreach (var item in mdlUOWModels)
            {
                string typeName = item.Entity.GetType().Name;
                var resultControl = await ControlHelper.Control(typeName, _iIOC, item.Entity, item.Mode, item.UpdateFields, isBefore);
                if (!resultControl.Result)
                    return resultControl;
            }
            return new BaseControlModel();
        }


        public async Task<ResponseRepository> IsCanDelete(string tableName, long id)
        {
            using (var cx = ContextHelper.CreateContext<TEntity>(null))
            {
                var mdls = cx.TableDTOs.FromSqlRaw(CommandHelper.GetCanDeleteCommand(tableName, id)).AsEnumerable().ToList();

                if (mdls == null || mdls.Count == 0 || mdls.Where(x => x.Count > 0).Count() == 0)
                    return new ResponseRepository();
                var dtos = mdls.Select(x => $"{x.Title.ToFaString()}({x.Count})").ToList();
                return new ResponseRepository(dtos);
            }
        }


        #endregion


        #region Add-Update-Delete-Commit
        public void AddEntity(object model)
        {
            mdlEntityActionModels?.Add(new UOWModel(model, EntityActionMode.Insert));
        }
        public void AddEntitys(List<object> models)
        {
            foreach (var mdl in models)
            {
                AddEntity(mdl);
            }
        }
        public void UpdateEntity(object model, List<string> fields)
        {
            mdlEntityActionModels?.Add(new UOWModel(model, EntityActionMode.Update, fields));
        }


        public void DeleteEntity(object model)
        {
            ((BaseEntity)model).IsDeleted = true;
            UpdateEntity(model, new List<string>() { nameof(BaseEntity.IsDeleted) });
        }
        public void DeleteEntitys(List<object>? models)
        {
            if (models != null)
                foreach (var mdl in models)
                {
                    DeleteEntity(mdl);
                }
        }
        public async Task<int> DeleteEntitys(Expression<Func<TEntity, bool>> where)
        {
            using (var cx = ContextHelper.CreateContext<TEntity>(null))
            {
                var q = cx.Set<TEntity>();

                using (var transaction = cx.Database.BeginTransaction())
                {
                    cx.RemoveRange(where != null ? q.Where(where) : q);
                    var cnt = await cx.SaveChangesAsync();
                    transaction.Commit();
                    return cnt;
                }
            }
        }
        public async Task<ResponseRepository> Commit(CommitConfig commitConfig, CancellationToken cancellationToken)
        {
            IDbContextTransaction? transaction = null;
            try
            {
                if (mdlEntityActionModels != null && mdlEntityActionModels.Count != 0)
                {


                    var resControlBefore = await NotifyControls(mdlEntityActionModels.Select(x => (UOWModel)x).ToList(), true);
                    if (!resControlBefore.Result)
                        return new ResponseRepository(resControlBefore.Errors);


                    using (var cx = ContextHelper.CreateContext<TEntity>(commitConfig.Transaction))
                    {
                        int cnt = 0;

                        if (commitConfig.Transaction != null)
                            cx.Database.UseTransaction(commitConfig.Transaction.GetDbTransaction());
                        else
                            transaction = cx.Database.BeginTransaction();

                        foreach (var item in mdlEntityActionModels.Where(x => ((UOWModel)x).Mode != 0).Select(s => (UOWModel)s).ToList())
                        {
                            switch (item.Mode)
                            {
                                case EntityActionMode.Insert:
                                    cx.Entry(item.Entity).State = EntityState.Added;

                                    var attr = item.Entity.GetType().GetCustomAttribute(typeof(EntityAttribute)) as EntityAttribute;
                                    if (attr != null && attr.AddSubEntitys != null)
                                    {
                                        var prps = item.Entity.GetType().GetProperties();

                                        foreach (var prp in prps)
                                        {
                                            #region Add Sub Entitys

                                            var val = prp.GetValue(item.Entity, null);
                                            if (val == null) continue;
                                            var type = val.GetType();
                                            if (type.IsGenericType)
                                                type = type.GetGenericArguments().FirstOrDefault();

                                            foreach (var subEntity in attr.AddSubEntitys.Where(x => x == type))
                                            {
                                                if (val.GetType().IsGenericType)
                                                {
                                                    foreach (object v in ((IEnumerable<object>)val).Cast<object>().ToList())
                                                    {
                                                        cx.Entry(v).State = EntityState.Added;
                                                        cnt++;
                                                    }
                                                }
                                                else
                                                {
                                                    cx.Entry(val).State = EntityState.Added;
                                                    cnt++;
                                                }
                                            }
                                            #endregion
                                        }
                                    }

                                    break;
                                case EntityActionMode.Update:
                                    cx.Entry(item.Entity).State = EntityState.Unchanged;
                                    if (item.UpdateFields != null && item.UpdateFields.Count != 0)
                                        item.UpdateFields.ForEach(a => cx.Entry(item.Entity).Property(a).IsModified = true);
                                    else
                                        return new ResponseRepository(Resource_Message.NotFoundInfo.ByParameters($"List_Update_{item.Entity.GetType().Name}"));
                                    break;
                                case EntityActionMode.Delete:
                                    cx.Entry(item.Entity).State = EntityState.Deleted;

                                    break;
                                default:
                                    break;
                            }
                            cnt++;
                        }

                        var resControlAfter = await NotifyControls(mdlEntityActionModels.Select(x => (UOWModel)x).ToList(), false);
                        if (!resControlAfter.Result)
                            return new ResponseRepository(resControlAfter.Errors);


                        var savecnt = await cx.SaveChangesAsync(cancellationToken);
                        cx.ChangeTracker.Clear();

                        #region  DataLog

                        var iSessionInfo = _iIOC.CreateObject<ISessionInfo>();

                        foreach (var model in mdlEntityActionModels.Select(x => (UOWModel)x).ToList())
                        {
                            var id = ((BaseEntity)model.Entity).Id;
                            var name = model.Entity.GetType().Name;
                            var sessionstr = iSessionInfo.ToSerialize();
                            var entity = new AppDataLog
                            {
                                Id = 0,
                                CreateDateTime = DateTime.Now,
                                EntityId = id,
                                EntityName = name,
                                IsDeleted = false,
                                LogMode = (int)model.Mode,
                                SessionInfoJson = sessionstr,
                                UserId = iSessionInfo.UserInfo?.Id,
                                DataJson = model.Entity.ToSerialize(),
                                UpdateFields = model.UpdateFields.ListToString(),
                            };
                            cx.Entry(entity).State = EntityState.Added;
                        }
                        await cx.SaveChangesAsync(cancellationToken);
                        cx.ChangeTracker.Clear();

                        #endregion

                        if (savecnt == cnt)
                            transaction?.CommitAsync(cancellationToken);
                        else
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return new ResponseRepository(Resource_Message.Rollback.ByParameters((cnt - savecnt).ToString()));
                        }
                        if (commitConfig.IsReturnData.ToBool())
                        {
                            List<object> returndata = new List<object>();
                            foreach (UOWModel item in mdlEntityActionModels)
                            {
                                returndata.Add(item.Entity);
                            }
                            return new ResponseRepository(returndata, returndata.Count);
                        }
                        return new ResponseRepository();
                    }
                }
                return new ResponseRepository(Resource_Message.NotFoundInfo.ByParameters("List_Entity"));
            }
            catch (Exception exp)
            {
                if (commitConfig.IsDisposeTransaction.ToBool() && transaction != null)
                    await transaction.RollbackAsync(cancellationToken);
                throw exp;
            }
            finally
            {
                if (commitConfig.IsDisposeTransaction.ToBool() && transaction != null)
                    transaction.Dispose();
                mdlEntityActionModels?.Clear();
            }
        }

        public async Task<bool> AddUpdateEntity(TEntity entity, bool isUpdate, List<string> fields, Expression<Func<TEntity, bool>> where)
        {
            using (var cx = ContextHelper.CreateContext<TEntity>(null))
            {
                var q = cx.Set<TEntity>();

                var mdlResult = where != null ? q.Where(where).ToList() : q.ToList();

                if (mdlResult != null && mdlResult.Count > 1)
                {
                    return false;
                }
                else if (mdlResult != null && mdlResult.Count == 1)
                {
                    if (isUpdate)
                    {
                        var propertyInfo = mdlResult.First().GetType().GetProperty("Id");
                        var valueId = propertyInfo.GetValue(mdlResult.First());
                        propertyInfo.SetValue(entity, Convert.ChangeType(valueId, propertyInfo.PropertyType), null);
                        mdlEntityActionModels?.Add(new UOWModel(entity, EntityActionMode.Update, fields));
                    }
                }
                else
                {
                    mdlEntityActionModels?.Add(new UOWModel(entity, EntityActionMode.Insert));
                }
                return true;
            }
        }

        public void ExistEntity(TEntity model)
        {
            mdlEntityActionModels?.Add(new UOWModel(model, EntityActionMode.Exist));
        }

        public void Dispose()
        {
            if (mdlEntityActionModels != null)
                mdlEntityActionModels.Clear();
            GC.SuppressFinalize(this);
        }

        #endregion




    }

}
