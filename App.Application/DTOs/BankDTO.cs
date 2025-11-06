using App.Application.Bases;
using App.Application.Interfaces; 

namespace App.Application.DTOs
{
    public class BankDTO : IDTO
    {
        [PropertyInfo(IsVisible: false)] public long? Id { get; set; }
        [PropertyInfo(Ordered: 1,   Title: "IsEnable",TextAligh:eTextAlighn.center,Width:80)] public bool? IsEnable { get; set; }
        [PropertyInfo(Ordered: 5,   Title: "Name")] public string? Name { get; set; } 
    }
}
