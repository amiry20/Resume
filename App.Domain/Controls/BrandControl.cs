using App.Domain.DB.Model;
using App.Domain.Repositories;
using App.Application.Interfaces;
using App.Application.DTOs;
using App.Helper.Helpers;
using App.Helper.Resources;

namespace App.Domain.Controls
{
    public class BrandControl : IControl<Brand>
    {
        readonly List<string> allowFields = new List<string>() { nameof(Brand.Name), nameof(Brand.Code), nameof(Brand.Ordered), nameof(Brand.IsEnable) };

        public BrandControl()
        {
        }
        public async Task<BaseControlModel> ControlFields(List<string> updateFields, EntityActionMode entityActionMode)
        {
            if (entityActionMode == EntityActionMode.Update)
            {
                var errors = updateFields.Where(x => !allowFields.Contains(x))
                    .Select(c => string.Format(Resource_Fa.ParameterNotAllow, "ویرایش", c))
                    .ToList();
                if (errors != null && errors.Count != 0)
                    return new BaseControlModel(errors);
            }
            return new BaseControlModel();
        }
        public async Task<BaseControlModel> ControlBefore(Brand model, EntityActionMode entityActionMode, List<string> updateFields)
        {
            if (entityActionMode == EntityActionMode.Delete)
                return new BaseControlModel();

            List<string> errors = new List<string>();

            if (model == null)
                errors.Add(string.Format(Resource_Fa.ModelIsEmpty, typeof(Brand).GetClassDisplayName()));

            if (model != null)
            {
                if (string.IsNullOrEmpty(model.Name.ToFix()))
                    errors.Add(string.Format(Resource_Fa.ParameterIsEmputy, typeof(BrandModel).GetPropertyDisplayName(nameof(BrandModel.Name))));
                if (string.IsNullOrEmpty(model.Code.ToFix()))
                    errors.Add(string.Format(Resource_Fa.ParameterIsEmputy, typeof(BrandModel).GetPropertyDisplayName(nameof(BrandModel.Code))));
                if (model.Code < 1000)
                    errors.Add(string.Format(Resource_Fa.CanNotInsertLessThanForItem, 1000, typeof(BrandModel).GetPropertyDisplayName(nameof(BrandModel.Code))));
                if (model.Code == 0)
                    errors.Add(string.Format(Resource_Fa.ParameterNotAllow, model.Code.ToString(), typeof(BrandModel).GetPropertyDisplayName(nameof(BrandModel.Code))));
            }

            if (errors.Any())
                return new BaseControlModel(errors);
            else return new BaseControlModel();
        }

        public async Task<BaseControlModel> ControlAfter(Brand model, EntityActionMode entityActionMode, List<string> updateFields)
        {
            if (entityActionMode == EntityActionMode.Delete)
                return new BaseControlModel();

            List<string> errors = new List<string>();

            if (model == null)
                errors.Add(string.Format(Resource_Fa.ModelIsEmpty, typeof(Brand).GetClassDisplayName()));

            if (model != null)
            {
                if (entityActionMode == EntityActionMode.Delete)
                {

                }
                else
                {
                    var repository = new Repository<Brand>();
                    var mdlExistName = await repository.Get(x => x.Name == model.Name && (entityActionMode == EntityActionMode.Update ? x.Id != model.Id : true));

                    if (!mdlExistName.Result)
                        errors.Add(string.Format(Resource_Fa.Error, mdlExistName.GetErrors()));

                    if (mdlExistName.Data != null)
                        errors.Add(string.Format(Resource_Fa.CanNotInsertAlreadyExist, $"{typeof(BrandModel).GetPropertyDisplayName(nameof(BrandModel.Name))}({model.Name})"));

                    //-------------------------------------------------------
                    var mdlExistCode = await repository.Get(x => x.Code == model.Code && (entityActionMode == EntityActionMode.Update ? x.Id != model.Id : true));

                    if (!mdlExistCode.Result)
                        errors.Add(string.Format(Resource_Fa.Error, mdlExistCode.GetErrors()));

                    if (mdlExistCode.Data != null)
                        errors.Add(string.Format(Resource_Fa.CanNotInsertAlreadyExist, $"{typeof(BrandModel).GetPropertyDisplayName(nameof(BrandModel.Code))}({model.Code})"));
                }
            }

            if (errors.Any())
                return new BaseControlModel(errors);
            else return new BaseControlModel();
        }
    }
}
