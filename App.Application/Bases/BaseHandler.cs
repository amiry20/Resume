using App.Application.Interfaces;
using App.Application.Utilities; 
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using App.Domain.Bases;
using App.Application.Classes;
using System.Reflection; 
using AutoMapper; 

namespace App.Application.Bases
{
    #region QueryHandler
    public abstract class BaseQueryHandler<TEntity, TDTO, TQuery> where TEntity : BaseEntity where TDTO : IDTO where TQuery : BaseQuery
    {

        protected readonly IRepository<TEntity> _iRepository;
        public readonly IAccess _iAccess;
        public readonly IMapper _iMapper;
        //public readonly ISessionInfo _iSessionInfo; چون ایجاد حلقه می شد غیر فعال شد
        public readonly IAppLogger _iAppLogger;
        public readonly IAppSetting _iAppSetting;
        public readonly ILanguageHelper _iLanguageHelper;
        public readonly IIOC _iIOC;
        public BaseQueryHandler(IIOC iIOC)
        {
            _iIOC = iIOC;
            //_iRepository = iRepository;
            _iRepository = iIOC.CreateObject<IRepository<TEntity>>();
           _iMapper = MapperConfig.InitializeAutomapper();
            // _iMapper = iIOC.CreateObject<IMapper>();
            _iAccess = iIOC.CreateObject<IAccess>();
            _iAppLogger = iIOC.CreateObject<IAppLogger>();
            _iAppSetting = iIOC.CreateObject<IAppSetting>();
            _iLanguageHelper = iIOC.CreateObject<ILanguageHelper>();
        }
        protected virtual Expression<Func<TEntity, object>>? EndHandler(TQuery request)
        {
            return null;
        }
        protected virtual Expression<Func<TEntity, object>>? GetSelect(TQuery request)
        {
            return null;
        }
        protected virtual Task HandleEnd(SelectReturn<List<TDTO>> selectReturn, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Expression<Func<TEntity, bool>>? GetFilter(TQuery request)
        {
            throw new NotImplementedException("GetFilter");
        }

        private Expression<Func<TEntity, bool>>? AddFilters(TQuery query, Expression<Func<TEntity, bool>> expression)
        {
            Expression<Func<TEntity, bool>> exp = x => x.IsDeleted == false;
            if (query.OnlyEnable)
            {
                Expression<Func<TEntity, bool>> expEnable = ExpressionHelper.ToExpressionWhere<TEntity>(typeof(bool?), true, "IsEnable");
                exp = ExpressionHelper.AndAlsoExpression(exp, expEnable);
            }
            if (expression == null)
                return exp;

            return ExpressionHelper.AndAlsoExpression(expression, exp);
        }

        public async Task<SelectReturn<List<TDTO>>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            var result = new SelectReturn<List<TDTO>>();
            try
            {
                var checkAccess = await _iAccess.CheckAccess(query.AccessParent, query.AccessChilds);
                if (!checkAccess.Result)
                {
                    result.Result = false;
                    result.Errors = checkAccess.Errors;
                    return result;
                }
                var baseParameterModel = new BaseParameterModel<TEntity>();
                baseParameterModel.Paging = query.Paging;
                baseParameterModel.Order = query.Order;
                baseParameterModel.Select = GetSelect(query);
                baseParameterModel.Where = GetFilter(query);
                baseParameterModel.Where = AddFilters(query, baseParameterModel.Where);

                var resultRepo = await _iRepository.Gets(baseParameterModel, cancellationToken);

                if (query.Paging == null)
                    query.Paging = new PageModel(resultRepo.TotalCount);
                query.Paging.TotalCount = resultRepo.TotalCount;

                result.Errors = resultRepo.Errors;
                result.Result = resultRepo.Result;
                result.Paging = query.Paging;
                result.Order = query.Order;
                if (query.IsStruct)
                    result.Structs = GetStructs(typeof(TDTO));
                result.Data = _iMapper.Map<List<TDTO>>(resultRepo.Data);

                #region IHasImage
                if (result.Data != null && result.Data.Count > 0)
                {
                    var iHasImage = typeof(TEntity).GetInterfaces().Where(x => x.Name.Equals(nameof(IHasImage))).ToList();

                    if (iHasImage != null && iHasImage.Count == 1)
                    {
                        var prop = typeof(TEntity).GetProperty(nameof(IHasImage.ImagePath));
                        var attrEntity = prop.GetCustomAttribute(typeof(EntityPropertyAttribute)) as EntityPropertyAttribute;
                        if (attrEntity == null ||  attrEntity.AppSettingName.IsNullEmpty())
                            throw new Exception("File Property AppSetting Attribute Is Empty");

                        var savePath = _iAppSetting.GetValue(attrEntity.AppSettingName);
                        foreach (var item in result.Data)
                        {
                            var prp = item.GetType().GetProperty(nameof(IHasImage.ImagePath));
                            var prpValue = prp?.GetValue(item);
                            if (prpValue != null)
                            {
                                string newPath = Path.Combine(savePath!, prpValue.ToString()!);
                                prp.SetValue(item, newPath, null);
                            }
                        }
                    }
                }

                #endregion

                await HandleEnd(result, cancellationToken);
            }
            catch (Exception ex)
            {
                _iAppLogger.Error(ex, this.GetType().Name);
                result.Errors = new List<string>() { ex.Message };
                result.Result = false;
                result.Data = null;
                result.Paging = null;
            }
            return result;
        }
        protected List<StructModel>? GetStructs(Type type)
        {
            var mdls = new List<StructModel>();
            foreach (var item in type.GetProperties().ToList())
            {
                PropertyInfoAttribute attr = (PropertyInfoAttribute)item.GetCustomAttributes(typeof(PropertyInfoAttribute), true).SingleOrDefault();
                if (attr == null)
                {
                    MetadataTypeAttribute metadataType = (MetadataTypeAttribute)type.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
                    if (metadataType != null)
                    {
                        var property = metadataType.MetadataClassType.GetProperty(item.Name);
                        if (property != null)
                        {
                            attr = (PropertyInfoAttribute)property.GetCustomAttributes(typeof(PropertyInfoAttribute), true).SingleOrDefault();
                        }
                    }
                }

                mdls.Add(new StructModel()
                {
                    Title = attr == null ? _iLanguageHelper.GetText(item.Name) : _iLanguageHelper.GetText(attr.Title),
                    Name = item.Name,
                    TextAligh = attr.TextAligh,
                    OrderField = attr?.OrderField ?? item.Name,
                    IsVisible = attr == null ? false : attr.IsVisible,
                    Ordered = attr == null ? 0 : attr.Ordered,
                    Width = attr == null ? 100 : attr.Width,
                    IsSeprator = attr == null ? false : attr.IsSeprator,
                    IsOnlyDate = attr == null ? false : attr.IsOnlyDate,
                    //TypeId = TextHelper.GetTypeMode(item.PropertyType)
                });
            }
            return mdls.OrderBy(o => o.Ordered).ToList();

        }
    }

    #endregion

    #region AddHandler
    public abstract class BaseAddHandler<TEntity, TDTO, TCommandAdd> where TEntity : BaseEntity where TDTO :
        IDTO where TCommandAdd : BaseAddCommand
    {
        protected readonly IRepository<TEntity> _iRepository;
        public readonly IAccess _iAccess;
        public readonly IMapper _iMapper;
        public readonly IAppLogger _iAppLogger;
        public readonly IIOC _iIOC;
        public readonly IAppSetting _iAppSetting;
        public readonly ISessionInfo _isessionInfo;
        public BaseAddHandler(IIOC iIOC )
        {
            _iIOC = iIOC;
            _iRepository = iIOC.CreateObject<IRepository<TEntity>>();
           _iMapper = MapperConfig.InitializeAutomapper();
             //_iMapper = iIOC.CreateObject<IMapper>();
            _iAccess = iIOC.CreateObject<IAccess>();
            _isessionInfo = iIOC.CreateObject<ISessionInfo>();
            _iAppLogger = iIOC.CreateObject<IAppLogger>();
            _iAppSetting = iIOC.CreateObject<IAppSetting>();
        }
        protected virtual List<TEntity>? SetDefault(List<TEntity> entity)
        {
            return entity;
        }

        private async Task<string> SaveFile(byte[] byteArray, string? path, string name)
        {
            if (byteArray == null) return null;
            string storagePath = Path.Combine(Environment.CurrentDirectory, path);

            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            string filePath = Path.Combine(storagePath, name);
            await File.WriteAllBytesAsync(filePath, byteArray);

            return filePath;
        }
        protected virtual List<TEntity> MapEntities(TCommandAdd commandAdd)
        {
            var entities = new List<TEntity>();
            entities.Add(_iMapper.Map<TEntity>(commandAdd));
            return entities;
        }
        protected virtual void RepositoryActions(List<TEntity> entities, TCommandAdd commandAdd)
        {

        }

        protected virtual AddReturn ControlCommand(TCommandAdd commandAdd)
        {
            return new AddReturn();
        }
        public async Task<AddReturn> Handle(TCommandAdd commandAdd, CancellationToken cancellationToken)
        {
            try
            {
                var resControlCommand = ControlCommand(commandAdd);
                if (!resControlCommand.Result)
                    return new AddReturn(resControlCommand.Errors);

                var checkAccess = await _iAccess.CheckAccess(commandAdd.AccessParent, commandAdd.AccessChilds);
                if (!checkAccess.Result)
                    return new AddReturn(checkAccess.Errors);

                var entities = MapEntities(commandAdd);

                entities = SetDefault(entities); 

                if (!entities.AnyCount())
                    return new AddReturn("NotFoundEntityForAcion");

                foreach (var entity in entities)
                {
                    #region IHasCreateDateTime
                    var iHasCreateDateTime = typeof(TEntity).GetInterfaces().Where(x => x.Name.Equals(nameof(IHasCreateDateTime))).ToList();

                    if (iHasCreateDateTime != null && iHasCreateDateTime.Count == 1)
                    {
                        var prop = typeof(TEntity).GetProperty("CreateDateTime");
                        if (prop == null)
                            throw new Exception("Handle Entity Has CreateDateTime Not Found Property");

                        prop.SetValue(entity, DateTime.Now);
                    }
                    #endregion
                    #region IHasCreateUser
                    var iHasCreateUser = typeof(TEntity).GetInterfaces().Where(x => x.Name.Equals(nameof(IHasCreateUser))).ToList();

                    if (iHasCreateUser != null && iHasCreateUser.Count == 1)
                    {
                        var prop = typeof(TEntity).GetProperty("CreateUser");
                        if (prop == null)
                            throw new Exception("Handle Entity Has CreateUser Not Found Property");

                        prop.SetValue(entity, _isessionInfo.UserInfo.Id);
                    }

                    #endregion

                    #region IHasImage
                    var iHasImage = typeof(TEntity).GetInterfaces().Where(x => x.Name.Equals(nameof(IHasImage))).ToList();

                    if (iHasImage != null && iHasImage.Count == 1)
                    {
                        var bytearray = ((IImageCommand)commandAdd).ImageByte;
                        if (bytearray != null)
                        {
                            var prop = typeof(TEntity).GetProperty(nameof(IHasImage.ImagePath));
                            var attrEntity = prop.GetCustomAttribute(typeof(EntityPropertyAttribute)) as EntityPropertyAttribute;
                            if (attrEntity == null ||  attrEntity.AppSettingName.IsNullEmpty())
                                throw new Exception("File Property AppSetting Attribute Is Empty");

                            var savePath = _iAppSetting.GetValue(attrEntity.AppSettingName);
                            var saveName = $"{Guid.NewGuid().ToString().Replace("-", "")}.png";
                            var path = await SaveFile(bytearray, $"wwwroot\\{savePath}", saveName);
                            ((IHasImage)entity).ImagePath = saveName;
                        }
                    }

                    #endregion

                    #region IHasEncode
                    var iHasEncode = typeof(TEntity).GetInterfaces().Where(x => x.Name.Equals(nameof(IHasEncode))).ToList();
                    if (iHasEncode != null && iHasEncode.Count == 1)
                    {
                        var props = typeof(TEntity).GetProperties();

                        if (props != null)
                        {
                            foreach (var prp in props)
                            {
                                var attrEntity = prp.GetCustomAttribute(typeof(EntityPropertyAttribute)) as EntityPropertyAttribute;
                                if (attrEntity != null && (attrEntity.IsEncode || attrEntity.IsHashCode))
                                {
                                    if (attrEntity.IsEncode)
                                        prp.SetValue(entity, prp.GetValue(entity, null), null);

                                    else if (attrEntity.IsHashCode)
                                        prp.SetValue(entity, prp.GetValue(entity, null), null);
                                }
                            }
                        }
                    }

                    #endregion 
                    _iRepository.AddEntity(entity);
                }

                RepositoryActions(entities, commandAdd);

                var resultCommit = await _iRepository.Commit(new CommitConfig(true, commandAdd.IsRetutnData), cancellationToken);
                if (!resultCommit.Result)
                    return new AddReturn(resultCommit.Errors);

                return new AddReturn(commandAdd.IsRetutnData ? _iMapper.Map<List<TDTO>>(resultCommit.Data) : null);
            }
            catch (Exception ex)
            {
                _iAppLogger.Error(ex, $"{this.GetType().Name}");
                return new AddReturn(ex);
            }
        }
    }

    #endregion


    #region EditHandler
    public abstract class BaseEditHandler<TEntity, TDTO, TCommandEdit> where TEntity : BaseEntity where TDTO :
        IDTO where TCommandEdit : BaseEditCommand
    {
        protected readonly IRepository<TEntity> _iRepository;
        public readonly IAccess _iAccess;
        public readonly IMapper _iMapper;
        public readonly IAppLogger _iAppLogger;
        public readonly IAppSetting _iAppSetting;
        public readonly ISessionInfo _isessionInfo;
        public readonly IIOC _iIOC;
        public BaseEditHandler(IIOC iIOC)
        {
            _iIOC = iIOC;
            _iRepository = iIOC.CreateObject<IRepository<TEntity>>();
            _iMapper = MapperConfig.InitializeAutomapper();
           // _iMapper = iIOC.CreateObject<IMapper>();
            _iAccess = iIOC.CreateObject<IAccess>();
            _isessionInfo = iIOC.CreateObject<ISessionInfo>();
            _iAppLogger = iIOC.CreateObject<IAppLogger>();
            _iAppSetting = iIOC.CreateObject<IAppSetting>();
        }
        public async Task<EditReturn> Handle(TCommandEdit commandEdit, CancellationToken cancellationToken)
        {
            try
            {
                var resControlCommand = ControlCommand(commandEdit);
                if (!resControlCommand.Result)
                    return new EditReturn(resControlCommand.Errors);

                var resUpdateFields = CheckUpdateFields(commandEdit);
                if (!resUpdateFields.Result)
                    return new EditReturn(resUpdateFields.Errors);

                var checkAccess = await _iAccess.CheckAccess(commandEdit.AccessParent, commandEdit.AccessChilds);
                if (!checkAccess.Result)
                    return new EditReturn(checkAccess.Errors);

                #region CheckNoUpdate 
                if (commandEdit.UpdateFields != null && commandEdit.UpdateFields.Count > 0)
                {
                    var props = typeof(TEntity).GetProperties();
                    if (props != null)
                    {
                        foreach (var prp in props)
                        {
                            var attrEntity = prp.GetCustomAttribute(typeof(EntityPropertyAttribute)) as EntityPropertyAttribute;
                            if (attrEntity != null && !attrEntity.IsUpdate)
                            {
                                if (commandEdit.UpdateFields.Contains(prp.Name))
                                    return new EditReturn($"CanNtot_Edit_Parameter_{prp.Name}");
                            }
                        }
                    }
                }
                #endregion

                var entities = MapEntities(commandEdit);

                entities = SetDefault(entities, commandEdit.UpdateFields);
                
                if (!entities.AnyCount())
                    return new EditReturn("NotFoundEntityForAcion");

                foreach (var entity in entities)
                {
                    if (entity.Id == null || entity.Id == 0)
                        return new EditReturn(Resource_Message.ParameterIsEmputy.ByParameters(nameof(entity.Id)));

                    #region IHasImage
                    if (commandEdit.UpdateFields.Contains(nameof(IHasImage.ImagePath)))
                    {
                        var iHasImage = typeof(TEntity).GetInterfaces().Where(x => x.Name.Equals(nameof(IHasImage))).ToList();

                        if (iHasImage != null && iHasImage.Count == 1)
                        {
                            var bytearray = ((IImageCommand)commandEdit).ImageByte;
                            if (bytearray != null)
                            {
                                var prop = typeof(TEntity).GetProperty(nameof(IHasImage.ImagePath));
                                var attrEntity = prop.GetCustomAttribute(typeof(EntityPropertyAttribute)) as EntityPropertyAttribute;
                                if (attrEntity == null ||  attrEntity.AppSettingName.IsNullEmpty())
                                    throw new Exception("File Property AppSetting Attribute Is Empty");

                                var savePath = _iAppSetting.GetValue(attrEntity.AppSettingName);
                                var saveName = $"{Guid.NewGuid().ToString().Replace("-", "")}.png";
                                var path = await SaveFile(bytearray, $"wwwroot\\{savePath}", saveName);
                                ((IHasImage)entity).ImagePath = saveName;
                            }
                        }
                    }
                    #endregion


                    _iRepository.UpdateEntity(entity, commandEdit.UpdateFields);
                }

                RepositoryActions(entities, commandEdit);

                var result = await _iRepository.Commit(new CommitConfig(true), cancellationToken);
                if (!result.Result)
                    return new EditReturn(result.Errors);
            }
            catch (Exception ex)
            {
                _iAppLogger.Error(ex, this.GetType().Name);
                return new EditReturn(ex);
            }
            return new EditReturn();
        }
        private async Task<string> SaveFile(byte[] byteArray, string? path, string name)
        {
            if (byteArray == null) return null;
            string storagePath = Path.Combine(Environment.CurrentDirectory, path);

            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            string filePath = Path.Combine(storagePath, name);
            await File.WriteAllBytesAsync(filePath, byteArray);

            return filePath;
        }

        #region virtual
        protected virtual EditReturn ControlCommand(TCommandEdit commandEdit)
        {
            return new EditReturn();
        }
        protected virtual List<TEntity>? SetDefault(List<TEntity> entity, List<string> updateFields)
        {
            return entity;
        }

        protected virtual void RepositoryActions(List<TEntity> entities, TCommandEdit commandEdit)
        {

        }
        protected virtual List<TEntity> MapEntities(TCommandEdit commandEdit)
        {
            var entities = new List<TEntity>();
            entities.Add(_iMapper.Map<TEntity>(commandEdit));
            return entities;
        }

        protected virtual EditReturn CheckUpdateFields(TCommandEdit commandEdit)
        {
            if (commandEdit.UpdateFields == null || commandEdit.UpdateFields.Count == 0)
                return new EditReturn("Parameter_For_Update_NotFound");
            return new EditReturn();
        }
        #endregion
    }

    #endregion

    #region DeleteHandler
    public abstract class BaseDeleteHandler<TEntity, TDTO, TCommandDelete> where TEntity : BaseEntity where TDTO :
        IDTO where TCommandDelete : BaseDeleteCommand
    {
        protected readonly IRepository<TEntity> _iRepository;
        public readonly IMapper _iMapper;
        public readonly IAppLogger _iAppLogger;
        public readonly ISessionInfo _isessionInfo;
        public readonly IIOC _iIOC;
        public readonly IAccess _iAccess;
        public BaseDeleteHandler(IIOC iIOC)
        {
            _iIOC = iIOC;
            _iRepository = iIOC.CreateObject<IRepository<TEntity>>();
            _iMapper = MapperConfig.InitializeAutomapper();
            //_iMapper = iIOC.CreateObject<IMapper>();
            _iAccess = iIOC.CreateObject<IAccess>();
            _isessionInfo = iIOC.CreateObject<ISessionInfo>();
            _iAppLogger = iIOC.CreateObject<IAppLogger>();
        }

        public async Task<DeleteReturn> Handle(TCommandDelete commandDelete, CancellationToken cancellationToken)
        {
            try
            {
                if (commandDelete.Ids == null || commandDelete.Ids.Count == 0)
                    return new DeleteReturn("Parameter_For_Delete_NotFound");

                var checkAccess = await _iAccess.CheckAccess(commandDelete.AccessParent, commandDelete.AccessChilds);
                if (!checkAccess.Result)
                    return new DeleteReturn(checkAccess.Errors);

                foreach (var id in commandDelete.Ids)
                {
                    var entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
                    entity.Id = id;
                    _iRepository.DeleteEntity(entity);
                }
                var result = await _iRepository.Commit(new CommitConfig(true), cancellationToken);
                if (!result.Result)
                    return new DeleteReturn(result.Errors);
            }
            catch (Exception ex)
            {
                _iAppLogger.Error(ex, this.GetType().Name);
                return new DeleteReturn(ex.Message);
            }
            finally
            {
            }
            return new DeleteReturn();
        }
    }

    #endregion
}
