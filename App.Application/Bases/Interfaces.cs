using App.Application.DTOs;

namespace App.Application.Bases
{

    public interface IBaseCommand
    {
        public List<string>? DisabledControls { get; set; }
        public List<string>? DisabledWarnings { get; set; }
    }
    public interface IAddCommand : IDisposable
    {
        public bool? ReturnInsertedRow { get; set; }
    }
    public interface IImageCommand
    {
        public byte[]? ImageByte { get; set; }
        public string? ImagePath { get; set; }
    }
    public interface IIOC
    {

        public T CreateObject<T>();
        public object CreateObject(Type type);
    }
    public interface IAccess
    {
        public Task<BaseControlModel> CheckAccess(eAccessParent eAccessParent, params eAccessChild[]? access);
        public Task<List<long>?> CheckAccess(List<long>? ids);
        public List<MenuDTO>? GetUserMenus();
    }

}
