using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters; 

namespace App.Web.Base
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NoSessionInfoAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionInfoAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                var isDefined = controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true)
                    .Any(a => a.GetType().Equals(typeof(NoSessionInfoAttribute)));
                if (isDefined)
                    return;
            }
            throw new NotImplementedException("Not NotImplemented SessionInfoAttribute.OnActionExecuted");
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            throw new NotImplementedException("Not NotImplemented SessionInfoAttribute.OnActionExecuting");
        }
    }
    //public class ContainsSubstringAttribute : ValidationAttribute
    //{
    //    private readonly string _substring;

    //    public ContainsSubstringAttribute(string substring)
    //    {
    //        _substring = substring;
    //    }

    //    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    //    {
    //        if (value is string stringValue)
    //        {
    //            if (stringValue.Contains(_substring))
    //            {
    //                return ValidationResult.Success;
    //            }
    //            else
    //            {
    //                return new ValidationResult($"{validationContext.DisplayName} must contain the substring '{_substring}'.");
    //            }
    //        }

    //        return new ValidationResult($"{validationContext.DisplayName} must be a string.");
    //    }
    //}


    //[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    //public class HttpContextAttribute : Attribute, IActionFilter
    //{
    //    private IHttpContext Accessor _httpContex tAccessor;

    //    public void OnActionExecuting(ActionExecutingContext context)
    //    {
    //        _httpConte xtAccessor = (IHttpCon textAccessor)context.HttpContext.RequestServices.GetService(typeof(IHttpContextAccessor));
    //        // Access the current HttpContext using _httpContextAccessor.HttpContext
    //    }

    //    public void OnActionExecuted(ActionExecutedContext context)
    //    {
    //        // Implement if needed
    //    }
    //}
}
