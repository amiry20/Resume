using App.Application.Bases;
using App.Application.DTOs;
using App.Application.Utilities;
using App.Domain.DB.Model; 

namespace App.Application.Controls
{
    public class BankControl : BaseControl<Bank>
    {
        public BankControl(IIOC iIOC) : base(iIOC)
        {
        }

        #region Befor

        protected override async Task<BaseControlModel> ControlBeforeInsert(Bank? entity)
        {
            if (entity.Name.ToFix().IsNullEmpty())
                _errors.Add(Resource_Message.ParameterIsEmputy.ByParameters(nameof(entity.Name)));

            return new BaseControlModel(_errors);
        }
        protected override async Task<BaseControlModel> ControlBeforeUpdate(Bank? entity, List<string> updateFields)
        {
            if (updateFields.Contains(nameof(entity.Name)) &&  entity.Name.ToFix().IsNullEmpty())
                _errors.Add(Resource_Message.ParameterIsEmputy.ByParameters(nameof(entity.Name)));

            return new BaseControlModel(_errors);
        }
        #endregion

        #region After


        protected override async Task<BaseControlModel> ControlAfterInsert(Bank? entity)
        {
            #region Name

            var mdlExistName = await _iRepository.Value.Get(new BaseParameterModel<Bank>(x => x.IsDeleted == false && x.Name == entity.Name.ToFix()));

            if (!mdlExistName.Result)
                _errors.Add(Resource_Message.Error.ByParameters(mdlExistName.GetErrors()));

            if (mdlExistName.Data != null)
                _errors.Add(Resource_Message.CanNotInsertAlreadyExist.ByParameters(nameof(entity.Name)));

            #endregion
            return new BaseControlModel(_errors);
        }
        protected override async Task<BaseControlModel> ControlAfterUpdate(Bank? entity, List<string> updateFields)
        {
            #region Name
            if (updateFields.Contains(nameof(entity.Name)))
            {
                var mdlExistName = await _iRepository.Value.Get(new BaseParameterModel<Bank>(x => x.IsDeleted == false && x.Name == entity.Name.ToFix()));

                if (!mdlExistName.Result)
                    _errors.Add(Resource_Message.Error.ByParameters(mdlExistName.GetErrors()));

                if (mdlExistName.Data != null)
                    _errors.Add(Resource_Message.CanNotInsertAlreadyExist.ByParameters(nameof(entity.Name)));
            }
            #endregion 
            return new BaseControlModel(_errors);
        }


        #endregion
    }

}
