using App.Application.Bases;
using App.Application.DTOs;
using MediatR; 

namespace App.Application.Queries
{ 
    public class BankQuery : BaseQuery, IRequest<SelectReturn<List<BankDTO>>>
    {
        public override eAccessParent AccessParent => eAccessParent.Bank;
    }
}
