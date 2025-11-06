using App.Application.Bases;

using MediatR;
using System.ComponentModel.DataAnnotations;
namespace App.Application.Commands
{
    public class BankAddCommand : BaseAddCommand, IRequest<AddReturn>
    {
        public override eAccessParent AccessParent => eAccessParent.Bank; 

        [Required(ErrorMessage = "{0}_IsRequired")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "{0}_IsRequired")] 
        public bool  IsEnable { get; set; }  

    }
    public class BankEditCommand : BaseEditCommand, IRequest<EditReturn>
    {

        public override eAccessParent AccessParent => eAccessParent.Bank;

        [Required(ErrorMessage = "{0}_IsRequired")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "{0}_IsRequired")] 
        public bool IsEnable { get; set; }


    }
    public class BankDeleteCommand : BaseDeleteCommand, IRequest<DeleteReturn>
    {
        public override eAccessParent AccessParent => eAccessParent.Bank;
        public BankDeleteCommand(long? id) : base(id)
        {
        }
    }
}
