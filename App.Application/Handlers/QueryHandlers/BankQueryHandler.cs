using App.Application.Bases;
using App.Application.DTOs; 
using App.Application.Queries;
using App.Application.Utilities;
using App.Domain.DB.Model;

using MediatR;
using System.Linq.Expressions;

namespace App.Application.Handlers.QueryHandlers
{
    public class BankQueryHandler : BaseQueryHandler<Bank, BankDTO, BankQuery>, IRequestHandler<BankQuery, SelectReturn<List<BankDTO>>>
    {
        public BankQueryHandler(IIOC iIOC) : base(iIOC)
        {
        }
        protected override Expression<Func<Bank, bool>>? GetFilter(BankQuery request)
        {
            if ( request.Filter.IsNullEmpty())
                return null;

            Expression<Func<Bank, bool>> result = x => x.Name.ToString().Contains(request.Filter);
            return result;
        }
    }
}
