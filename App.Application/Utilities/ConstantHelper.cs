 

namespace App.Application.Utilities
{
    public static class ConstantHelper
    {  
        public static int PageSize { get { return 10; } }
        public static List<int> PageSizes
        {
            get
            {
                var items = new List<int>();
                items.Add(10);
                items.Add(20);
                items.Add(50);
                items.Add(100);
                items.Add(500);
                items.Add(1000);
                return items;
            }
        } 
    }
}
