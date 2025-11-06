using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using App.Application.Interfaces;
using App.Application.Utilities;

namespace App.Application.Bases
{
    public abstract class BaseQuery
    {
        public abstract eAccessParent AccessParent { get; }
        public virtual eAccessChild[] AccessChilds
        {
            get { return new[] { eAccessChild.View }; }
        }
        public bool IsStruct { get; set; } = false;
        public bool OnlyEnable { get; set; } = false;
        public PageModel? Paging { get; set; }
        public OrderModel? Order { get; set; }
        public List<string>? Filters { get; set; }
        public string? Filter { get; set; }
    }

    public abstract class BaseEditCommand
    {
        [NotMap] public abstract eAccessParent AccessParent { get; }
        public virtual eAccessChild[] AccessChilds
        {
            get { return new[] { eAccessChild.Edit }; }
        }
        [Required(ErrorMessage = "{0}_IsRequired")]
        [NotMap] public long? Id { get; set; }
        [NotMap] public List<string>? UpdateFields { get; set; }
    }
    public abstract class BaseAddCommand
    {
        public bool IsRetutnData { get; set; } = false;
        public abstract eAccessParent AccessParent { get; }
        public virtual eAccessChild[] AccessChilds
        {
            get { return new[] { eAccessChild.Add }; }
        }
    }
    public abstract class BaseDeleteCommand
    {
        public abstract eAccessParent AccessParent { get; }
        public virtual eAccessChild[] AccessChilds
        {
            get { return new[] { eAccessChild.Delete }; }
        }
        public BaseDeleteCommand()
        {

        }
        public BaseDeleteCommand(long? id)
        {
            Ids = new List<long>();
            Ids.Add((long)id);

        }
        public List<long> Ids { get; set; }
    }
    public class BaseNameValueModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class BaseParameterModel<T>
    {
        public object Value { get; set; }
        public List<Expression<Func<T, object>>>? Includs1 { get; set; }
        public Expression<Func<T, bool>>? Where { get; set; }
        public Expression<Func<T, object>>? Select { get; set; }
        public OrderModel? Order { get; set; }
        public string[]? Includs { get; set; }
        public PageModel? Paging { get; set; }

        public BaseParameterModel()
        {
        }
        public BaseParameterModel(int page, int pageSize)
        {
            Paging = new PageModel(page, pageSize);
        }
        public BaseParameterModel(Expression<Func<T, bool>>? where)
        {
            Where = where;
        }
        public BaseParameterModel(PageModel pageModel)
        {
            Paging = pageModel;
        }
    }

    public class StructModel
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? OrderField { get; set; }
        public string? TextAligh { get; set; } = string.Empty;

        public int Width { get; set; }
        public int Ordered { get; set; }
        public bool IsVisible { get; set; }
        public bool IsSeprator { get; set; }
        public bool IsOnlyDate { get; set; }
    }
    public class PageModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int GetSkip
        {
            get
            {
                var skip = Page - 1;
                if (skip < 0)
                    skip = 0;
                return (skip * PageSize);
            }
        }
        public int GetLastPage
        {
            get
            {
                if (TotalCount == 0 || PageSize == 0)
                    return TotalCount;
                if (TotalCount % PageSize == 0)
                    return TotalCount / PageSize;
                return (TotalCount / PageSize) + 1;
            }
        }
        public int GetRowNo
        {
            get
            {
                if (PageSize > 0 && Page > 1)
                    return ((Page - 1) * PageSize) + 1;
                return 1;
            }
        }

        public PageModel(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
        }
        public PageModel(int totalCount)
        {
            Page = 1;
            PageSize = ConstantHelper.PageSize;
            TotalCount = totalCount;
        }
        public PageModel()
        {
            Page = 1;
            PageSize = ConstantHelper.PageSize;
        }

    }

    public record OrderModel
    {
        public string ColName { get; set; }
        public bool Asc { get; set; } = false;
        public OrderModel()
        {

        }
        public OrderModel(string colName, bool asc)
        {
            ColName = colName; Asc = asc;
        }
    }
    public record BaseFilter
    {
        public string? ColumnName { get; set; }
        public eFilterType? FilterType { get; set; }
        public string? Value { get; set; }
        public object? Obj { get; set; }
    }

    public class BaseReturn
    {
        public bool Result { get; set; }
        public List<string>? Errors { get; set; }
        public string? GetErrors()
        {
            return Errors.ListToString('\n');
        }

        /// <summary>
        /// Result = true; 
        /// </summary> 
        public BaseReturn()
        {
            Result = true;
        }
        /// <summary>
        /// Result = true; 
        /// </summary>
        /// <param name="error"></param>
        public BaseReturn(string error)
        {
            Result = false;
            Errors = new List<string>() { error };
        }


        /// <summary>
        /// Result = false; Set Errors = ?;  
        /// </summary>
        /// <param name="errors"></param>
        public BaseReturn(List<string>? errors)
        {
            Result = false;
            Errors = errors;
        }

        /// <summary>
        /// Result = false; Exception = ?;
        /// </summary>
        /// <param name="exception"></param>
        public BaseReturn(Exception exception, bool isInnerExeption)
        {
            Result = false;
            Errors = exception.GetExceptionMessage(isInnerExeption);
        }

    }

    public class EditReturn : BaseReturn
    {
        public EditReturn()
        {
            Result = true;
            Errors = null;
        }
        public EditReturn(Exception exception)
        {
            Result = false;
            Errors = new List<string>() { exception.Message };
        }
        public EditReturn(List<string>? errors)
        {
            Result = false;
            Errors = errors;
        }
        public EditReturn(string error)
        {
            Result = false;
            Errors = new List<string>() { error };
        }

    }

    public class AddReturn : BaseReturn
    {
        public object? Data { get; set; }
        public AddReturn()
        {
            Result = true;
            Errors = null;
        }

        public AddReturn(object? data)
        {
            Result = true;
            Data = data;
            Errors = null;
        }
        public AddReturn(Exception exception)
        {
            Result = false;
            Errors = new List<string>() { exception.Message };
        }
        public AddReturn(string error)
        {
            Result = false;
            Errors = new List<string>() { error };
        }
        public AddReturn(List<string>? errors)
        {
            Result = false;
            Errors = errors;
        }

    }
    public class DeleteReturn : BaseReturn
    {
        public DeleteReturn()
        {
            Result = true;
            Errors = null;
        }
        public DeleteReturn(List<string>? errors)
        {
            Result = false;
            Errors = errors;
        }
        public DeleteReturn(string error)
        {
            Result = false;
            Errors = new List<string>() { error };
        }

    }
    public class SelectReturn<T> : BaseReturn
    {
        public OrderModel? Order { get; set; }
        public PageModel? Paging { get; set; }
        public List<StructModel>? Structs { get; set; }
        public T? Data { get; set; }
        public SelectReturn()
        {
        }
        /// <summary>
        ///  Result = false; Set Data = ?; Set Errors = ?; 
        /// </summary>
        /// <param name="data"></param> 
        /// <param name="errors"></param>
        public SelectReturn(T? data, List<string>? errors)
        {
            Result = false;
            Data = data;
            Errors = errors;
        }
        /// <summary>
        ///  Result = true; Set Data = ?; Set Errors = null; 
        /// </summary>
        /// <param name="data"></param>
        public SelectReturn(T? data)
        {
            Result = true;
            Data = data;
            Errors = null;
        }
        /// <summary>
        ///  Result = false; Set Data = ?; Set Errors = ?; 
        /// </summary>
        /// <param name="data"></param> 
        /// <param name="errors"></param>
        public SelectReturn(T? data, PageModel paging)
        {
            Paging = paging;
            Result = true;
            Data = data;
        }
    }

    public class Access : IAccess
    {
        private readonly IIOC iIOc;
        public Access(IIOC iIOc)
        {
            this.iIOc = iIOc;
        }
        public async Task<BaseControlModel> CheckAccess(eAccessParent accessParent, params eAccessChild[]? access)
        {
            if (access == null)
                return new BaseControlModel(Resource_Message.ParameterIsEmputy.ByParameters("Access"));

            if (accessParent == eAccessParent.None)
                return new BaseControlModel();

            var iSessionInfo = iIOc.CreateObject<ISessionInfo>();

            //if (iSessionInfo.UserInfo == null)
            //    await iSessionInfo.GetUserInfoAsync();

            if (iSessionInfo.UserInfo == null)
                return new BaseControlModel(Resource_Message.DoNotAccess.ByParameters(Resource_Message.NotFoundUser));

            if (iSessionInfo.UserInfo.IsAdmin.ToBool())
                return new BaseControlModel();


            var lst = access.Select(x => x.ToString()).ToList();
            if (StaticClass.AppSectionAccess != null)
            {
                var name = accessParent.ToString();

                var section = StaticClass.AppSectionAccess.Where(x => x.Name == name).SingleOrDefault();
                if (section != null)
                {
                    var chiled = StaticClass.AppSectionAccess.Where(x => x.ParentId == section.Id).ToList();
                    foreach (var item in chiled.Where(x => lst.Contains(x.Name)))
                    {
                        if (item.Accesss != null && item.Accesss.Any(x => iSessionInfo.UserInfo.RoleIds.Contains((long)x.RoleId)))
                            return new BaseControlModel();
                    }
                }
            }
            var array = lst.ListToString(joinChar: '_');
            array = $"{array}_{accessParent}";
            return new BaseControlModel(Resource_Message.DoNotAccess.ByParameters(array));
        }

        public async Task<List<long>?> CheckAccess(List<long>? ids)
        {
            if (ids == null || ids.Count == 0)
                return null;

            var iSessionInfo = iIOc.CreateObject<ISessionInfo>();
            //if (iSessionInfo.UserInfo == null)
            //    await iSessionInfo.GetUserInfoAsync();

            if (iSessionInfo.UserInfo == null) return null;

            if (iSessionInfo.UserInfo.IsAdmin.ToBool()) return ids;


            if (StaticClass.AppAccess != null)
            {
                var result = StaticClass.AppAccess
                  .Where(x => iSessionInfo.UserInfo.RoleIds.Contains(x.RoleId ?? 0) && ids.Contains(x.RoleSectionId ?? 0))
                  .Select(s => s.RoleSectionId ?? 0).ToList();

                return result;
            }
            return null;
        }

        public List<MenuDTO>? GetUserMenus()
        {
            var iSessionInfo = iIOc.CreateObject<ISessionInfo>();
            if (iSessionInfo.UserMenuInfo != null) return iSessionInfo.UserMenuInfo;

            if (iSessionInfo.UserInfo == null || (!iSessionInfo.UserInfo.IsAdmin.ToBool() && iSessionInfo.UserInfo.AccessIds == null)) return null;

            var userMenus = new List<MenuDTO>();

            if (StaticClass.AppMenus != null && StaticClass.AppMenus.Count > 0)
            {
                if (iSessionInfo.UserInfo.IsAdmin.ToBool())
                {
                    userMenus = StaticClass.AppMenus.OrderBy(o => o.Ordered).Select(c => new MenuDTO()
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Link = c.Link,
                        IconOne = c.IconOne,
                        IconTwo = c.IconTwo,
                        Description = c.Description,
                        ParentId = c.ParentId,
                        Ordered = c.Ordered,
                        SubMenus = c.SubMenus,
                    }).ToList();
                }
                else
                {
                    foreach (var item in StaticClass.AppMenus.OrderBy(o => o.Ordered).ToList())
                    {
                        if (item.AccessIds.Any(x => iSessionInfo.UserInfo.AccessIds.Contains(x)))
                        {
                            var mdl = new MenuDTO()
                            {
                                Id = item.Id,
                                Title = item.Title,
                                Link = item.Link,
                                IconOne = item.IconOne,
                                IconTwo = item.IconTwo,
                                Description = item.Description,
                                ParentId = item.ParentId,
                                Ordered = item.Ordered,
                                SubMenus = null,
                            };
                            mdl.SubMenus = GetUserSubMenus(item.SubMenus.OrderBy(o => o.Ordered).ToList(), iSessionInfo.UserInfo.AccessIds);
                            userMenus.Add(mdl);
                        }
                    }
                }
            }

            StaticClass.AppUserMenus.Remove(iSessionInfo.UserInfo.KeyId.ToString());
            StaticClass.AppUserMenus.TryAdd(iSessionInfo.UserInfo.KeyId.ToString(), userMenus);
            return userMenus;
        }
        private List<MenuDTO>? GetUserSubMenus(List<MenuDTO> mdls, List<long>? acsessIds)
        {
            var userMenus = new List<MenuDTO>();

            foreach (var item in mdls)
            {
                if (item.AccessIds.Any(x => acsessIds.Contains(x) || x == 0))
                {
                    var mdl = new MenuDTO()
                    {
                        Id = item.Id,
                        Title = item.Title,
                        Link = item.Link,
                        IconOne = item.IconOne,
                        IconTwo = item.IconTwo,
                        Description = item.Description,
                        ParentId = item.ParentId,
                        Ordered = item.Ordered,
                        SubMenus = null,
                    };
                    mdl.SubMenus = GetUserSubMenus(item.SubMenus, acsessIds);
                    userMenus.Add(mdl);
                }
            }
            return userMenus;
        }
    }

}