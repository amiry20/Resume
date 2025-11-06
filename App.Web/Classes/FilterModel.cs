
namespace App.Web.Classes
{
    public class FilterModel
    {
        public int? Id { get; set; }
        public bool IsSelect { get; set; }
        public string? Name { get; set; }
        public string? Value { get; set; }

        public FilterModel(int? id, bool isSelect, string? name)
        {
            Id = id;
            IsSelect = isSelect;
            Name = name;
            Value = null;

        }
        public FilterModel(int? id, bool isSelect, string? name, string? value)
        {
            Id = id;
            IsSelect = isSelect;
            Name = name;
            Value = value;
        }
    }
}
