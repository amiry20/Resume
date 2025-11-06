 

namespace App.Domain.Bases
{
    public class EntityAttribute : Attribute
    {
        public string? DBName { get; set; }  
        public Type[]? AddSubEntitys { get; set;} 
        public Type[]? EncodeProperties{ get; set;} 
        public Type[]? HashProperties{ get; set;} 
    }
    public class EntityPropertyAttribute : Attribute
    {
        public string? AppSettingName { get; set; }
        public bool IsUpdate { get; set; } = true;
        public bool IsEncode { get; set; } = false;
        public bool IsHashCode { get; set; } = false;
    }
}
