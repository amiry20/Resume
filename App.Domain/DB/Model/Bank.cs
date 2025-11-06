using App.Domain.Bases; 
namespace App.Domain.DB.Model
{

    [EntityAttribute(DBName = "DB")]
    public partial class Bank : BaseEntity
    { 
        public string? Name { get; set; } = null!;

        public bool? IsEnable { get; set; } 
        public virtual ICollection<PersonAccount> PersonAccounts { get; set; } = new List<PersonAccount>();
    } 
}
