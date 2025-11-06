



namespace App.Application.Bases
{
    public class NotMapAttribute : Attribute
    {
    }
    public class PropertyInfoAttribute : Attribute
    {
        public PropertyInfoAttribute()
        {

        }

        public PropertyInfoAttribute(bool IsVisible)
        {
            this.IsVisible = IsVisible;
        }
        public PropertyInfoAttribute(int Ordered, string Title)
        {
            this.Title = Title;
            this.Ordered = Ordered;
        }
        public PropertyInfoAttribute(int Ordered, string Title, string OrderField, int Width = 100)
        {
            this.Title = Title;
            this.Ordered = Ordered;
            this.Width = Width;
            this.OrderField = OrderField;
        }
        public PropertyInfoAttribute(int Ordered, string Title, string OrderField, eTextAlighn TextAligh, int Width = 100)
        {
            this.Title = Title;
            this.Ordered = Ordered;
            this.OrderField = OrderField;
            this.Width = Width;
            this.TextAligh = TextAligh.ToString();
        }
        public PropertyInfoAttribute(int Ordered, string Title, string OrderField, eTextAlighn TextAligh, bool IsSeprator)
        {
            this.Title = Title;
            this.Ordered = Ordered;
            this.OrderField = OrderField;
            this.IsSeprator = IsSeprator;
            this.TextAligh = TextAligh.ToString();
        }
        public PropertyInfoAttribute(int Ordered, string Title, eTextAlighn TextAligh, int Width = 100, bool IsSeprator = false, bool IsOnlyDate = false)
        {
            this.Title = Title;
            this.Ordered = Ordered;
            this.Width = Width;
            this.IsSeprator = IsSeprator;
            this.IsOnlyDate = IsOnlyDate;
            this.TextAligh = TextAligh.ToString();
        }
        public PropertyInfoAttribute(int Ordered, string Title, bool IsSeprator)
        {
            this.Title = Title;
            this.Ordered = Ordered;
            this.IsSeprator = IsSeprator;
        }
        public PropertyInfoAttribute(int Ordered, string Title, int Width)
        {
            this.Title = Title;
            this.Ordered = Ordered;
            this.Width = Width;
        }
        public PropertyInfoAttribute(int Ordered, bool IsOnlyDate, string Title)
        {
            this.Title = Title;
            this.Ordered = Ordered;
            this.IsOnlyDate = IsOnlyDate;
        }

        public string? Title { get; private set; } = null;
        public string? OrderField { get; private set; } = null;
        public string? TextAligh { get; private set; } = null;
        public int Width { get; private set; } = 100;
        public int Ordered { get; private set; } = 1;
        public bool IsVisible { get; private set; } = true;
        public bool IsSeprator { get; private set; } = false;
        public bool IsOnlyDate { get; private set; } = false;
    }
}
