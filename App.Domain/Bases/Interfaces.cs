 

namespace App.Domain.Bases
{

    public interface IHasCreateDateTime
    { 
    }
    public interface IHasCreateUser
    { 
    }
    public interface IHasEncode
    { 
    }
    public interface IHasImage
    {
        public string? ImagePath { get; set; }
    }
    public interface IHasEnable
    {
        public bool? IsEnable { get; set; }
    } 
}
