using System.ComponentModel.DataAnnotations;

namespace App.Domain.Bases
{
    public interface IEntity
    {
    }
    public class BaseEntity : IEntity
    {
        [Key]
        public long Id { get; set; }
        public bool IsDeleted { get; set; }

    }

}
