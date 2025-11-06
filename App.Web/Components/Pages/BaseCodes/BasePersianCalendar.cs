using App.Application.Bases;    


namespace App.Web.Components.Pages.BaseCodes
{
    public class BasePersianCalendar :  IDisposable  
    {
        #region Declares 
          
        protected List<string>? _msgErrors;  
         
        protected string? TxtFilter { get; set; }


        #endregion

        #region Method
 
     
       
        #endregion

         

        #region Virtual
      
     

        protected virtual BaseFilter GetFilter(string value)
        {
            throw new NotImplementedException("Not NotImplemented BasePersianCalendar.GetFilter");
        }

        #endregion

        public void Dispose()
        { 
        }
    }
}
