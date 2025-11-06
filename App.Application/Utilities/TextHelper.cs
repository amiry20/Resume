using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using App.Application.Bases;
using App.Application.Models;
using Newtonsoft.Json;

namespace App.Application.Utilities
{
    public static class TextHelper
    {
        readonly static System.Text.Json.JsonSerializerOptions optionSystemText = new System.Text.Json.JsonSerializerOptions()
        {
            MaxDepth = 0,
#pragma warning disable SYSLIB0020 // Type or member is obsolete
            IgnoreNullValues = true,
#pragma warning restore SYSLIB0020 // Type or member is obsolete
            IgnoreReadOnlyProperties = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        readonly static JsonSerializerSettings optionNewtonsoft = new JsonSerializerSettings()
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
        };

        public static string ToLongString(this object? value, long defualtValue = -1, string format = "")
        {
            if (value == null)
                return defualtValue.ToString(format);
            string? val = value.ToString()?.Replace(",", "");
            long outvalue = -1;
            if (!long.TryParse(val, out outvalue))
            {
                return defualtValue.ToString(format);
            }
            return outvalue.ToString(format);
        }
        public static string ToPrice(this object? value)
        {
            return value.ToLongString(0, "#,0");
        }
        public static string? ToSerialize(this object? value, bool isNewtonsoft = false)
        {
            if (value == null) return null;
            if (isNewtonsoft) return JsonConvert.SerializeObject(value, optionNewtonsoft);
            return System.Text.Json.JsonSerializer.Serialize(value, optionSystemText);
        }

        public static bool IsNullEmpty(this object? value, int? len = null)
        {
            if (value == null) return true;
            return IsNullEmpty(value.ToString(), len);
        }
        public static bool IsNullEmpty(this string? value, int? len = null)
        {
            if (len != null)
                return string.IsNullOrEmpty(value) && value?.Length != len;

            return string.IsNullOrEmpty(value);
        }
        public static bool AnyCount(this IList? value, int count = 0)
        {
            if (value == null) return false;

            return value.Count > count;
        }
        public static List<string>? ToNewList(this string? value, params string[]? valueRows)
        {
            if (value == null) return null;
            var result = new List<string>() { value };
            if (valueRows != null)
                result.AddRange(valueRows);
            return result;
        }
        public static void AddNewList(ref List<string?>? value, params string[]? valueRows)
        {
            if (value == null)
                value = new List<string?>();
            if (valueRows != null)
                value.AddRange(valueRows);
        }
        public static List<SelectItemModel> GetDisplayNames(this Type type)
        {
            var mdls = new List<SelectItemModel>();
            foreach (var item in type.GetProperties().ToList())
            {
                DisplayAttribute attr = (DisplayAttribute)item.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();
                if (attr == null)
                {
                    MetadataTypeAttribute metadataType = (MetadataTypeAttribute)type.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
                    if (metadataType != null)
                    {
                        var property = metadataType.MetadataClassType.GetProperty(item.Name);
                        if (property != null)
                        {
                            attr = (DisplayAttribute)property.GetCustomAttributes(typeof(DisplayNameAttribute), true).SingleOrDefault();
                        }
                    }
                }
                if (attr == null || attr.Name.Equals("Hiden")) continue;

                mdls.Add(new SelectItemModel()
                {
                    Ordered = mdls.Count + 1,
                    Title = (attr != null) ? attr.Name : "NoDisplay",
                    Name = item.Name,
                });
            }
            return mdls.OrderBy(o => o.Ordered).ToList();
        }

        public static T? ToDeserialize<T>(this string? value, bool isNewtonsoft = false)
        {
            if (value.IsNullEmpty()) return default(T);
            if (isNewtonsoft) return JsonConvert.DeserializeObject<T>(value, optionNewtonsoft);
            return System.Text.Json.JsonSerializer.Deserialize<T>(value, optionSystemText);
        }
        //public static List<T> ToDeserializes<T>(this string value)
        //{
        //    return System.Text.Json.JsonSerializer.Deserialize<List<T>>(value, options);
        //}

        //public static void ToDarukadeProductSearch(this string value)
        //{
        //    if (value != null && value.Length > 0)
        //        System.Diagnostics.Process.Start(@"https://www.darukade.com/products?w=" + value);
        //}
        //public static void ToDarukadeProduct(this string value)
        //{
        //    if (value != null && value.Length > 0)
        //        System.Diagnostics.Process.Start(@"https://www.darukade.com/find/0/" + value);
        //}
        //public static string CleanText(this string value)
        //{
        //    return value.Trim().Replace("با کلید Enter انتخاب نمایید", "");
        //}
        //public static string FixText1(this string value)
        //{
        //    return value.Trim().ToLower();
        //}

        public static string? ToFix(this string? value)
        {
            if (value.IsNullEmpty()) return value;

            return value.Trim()
                .Replace("ي", "ی")
                .Replace("ك", "ک")
                .Replace("٠", "0")
                .Replace("١", "1")
                .Replace("٢", "2")
                .Replace("٣", "3")
                .Replace("٤", "4")
                .Replace("٥", "5")
                .Replace("٦", "6")
                .Replace("٧", "7")
                .Replace("٨", "8")
                .Replace("٩", "9");
        }
        public static string ToFix(this object value, string def = "")
        {
            if (value == null) return def;

            return value.ToString().ToFix();
        }


        private static string hrf(int lenght, int e, string str)
        {
            int s = 0;
            string str2 = string.Empty, retstr = string.Empty;
            if (e == 3)
            {
                try
                {
                    str2 = str.Substring(s, 1);
                    switch (str2)
                    {
                        case "0": break;
                        case "1": retstr = retstr + "صد"; break;
                        case "2": retstr = retstr + "دویست"; break;
                        case "3": retstr = retstr + "سیصد"; break;
                        case "4": retstr = retstr + "چهارصد"; break;
                        case "5": retstr = retstr + "پانصد"; break;
                        case "6": retstr = retstr + "ششصد"; break;
                        case "7": retstr = retstr + "هفتصد"; break;
                        case "8": retstr = retstr + "هشتصد"; break;
                        case "9": retstr = retstr + "نه صد"; break;
                    }
                    s++;
                }
                catch
                {

                }
            }
            if (e >= 2)
            {
                if (e == 3)
                    str2 = str.Substring(1, 2);
                else
                    str2 = str;

                if ((Convert.ToInt32(str2) > 9 && Convert.ToInt32(str2) < 20))
                {
                    try
                    {
                        if (e == 3 && str.Substring(0, 1) != "0")
                            retstr = retstr + " و ";


                        switch (str2)
                        {
                            case "10": retstr = retstr + "ده"; break;
                            case "11": retstr = retstr + "یازده"; break;
                            case "12": retstr = retstr + "دوازده"; break;
                            case "13": retstr = retstr + "سیزده"; break;
                            case "14": retstr = retstr + "چهارده"; break;
                            case "15": retstr = retstr + "پانرده"; break;
                            case "16": retstr = retstr + "شانزده"; break;
                            case "17": retstr = retstr + "هفتده"; break;
                            case "18": retstr = retstr + "هجده"; break;
                            case "19": retstr = retstr + "نوزده"; break;
                        }
                        e = 0;
                    }
                    catch
                    {

                    }
                }
                else
                {
                    try
                    {
                        str2 = str.Substring(s, 1);
                        if (e == 3 && str2 != "0" && str.Substring(0, 1) != "0")
                            retstr = retstr + " و ";
                        switch (str2)
                        {
                            case "0": break;
                            case "1": retstr = retstr + "ده"; break;
                            case "2": retstr = retstr + "بیست"; break;
                            case "3": retstr = retstr + "سی"; break;
                            case "4": retstr = retstr + "چهل"; break;
                            case "5": retstr = retstr + "پنجاه"; break;
                            case "6": retstr = retstr + "شصت"; break;
                            case "7": retstr = retstr + "هفتاد"; break;
                            case "8": retstr = retstr + "هشتاد"; break;
                            case "9": retstr = retstr + "نود"; break;
                        }
                        s++;
                    }
                    catch
                    {

                    }
                }

            }
            if (e >= 1)
            {
                try
                {
                    if (str.Substring(s, 1) != "0")
                    {
                        try
                        {
                            str2 = str.Substring(0, 2);
                            if ((e == 3 || e == 2) && Convert.ToInt32(str2) != 0)
                                retstr = retstr + " و ";
                        }
                        catch
                        {
                        }
                        str2 = str.Substring(s, 1);
                        switch (str2)
                        {
                            case "0": break;
                            case "1": retstr = retstr + "یک"; break;
                            case "2": retstr = retstr + "دو"; break;
                            case "3": retstr = retstr + "سه"; break;
                            case "4": retstr = retstr + "چهار"; break;
                            case "5": retstr = retstr + "پنج"; break;
                            case "6": retstr = retstr + "شش"; break;
                            case "7": retstr = retstr + "هفت"; break;
                            case "8": retstr = retstr + "هشت"; break;
                            case "9": retstr = retstr + "نه"; break;
                        }
                    }
                }
                catch
                {

                }
            }
            return retstr;
        }
        private static string NumberToString(this string numberstr)
        {
            string retsult = string.Empty, str2 = string.Empty;
            numberstr = numberstr.Replace(",", "");
            if (numberstr != "")
            {
                try
                {
                    int lngh = numberstr.Length, start = 0, l = 0;
                    if (lngh > 12)
                    {

                        str2 = numberstr.Substring(start, lngh - 12);
                        if (Convert.ToInt32(str2) != 0)
                        {
                            retsult = retsult + hrf(l, lngh - 12, str2);
                        }
                        start += lngh - 12;
                        lngh -= lngh - 12;

                        retsult = retsult + " تیلیارد";
                    }
                    if (lngh > 9)
                    {

                        str2 = numberstr.Substring(start, lngh - 9);
                        if (Convert.ToInt32(str2) != 0)
                        {
                            if (l > 0)
                                retsult = retsult + " و ";
                            retsult = retsult + hrf(l, lngh - 9, str2);
                            l++;
                            retsult = retsult + " میلیارد";
                        }
                        start += lngh - 9;
                        lngh -= lngh - 9;
                    }
                    if (lngh > 6)
                    {
                        str2 = numberstr.Substring(start, lngh - 6);
                        if (Convert.ToInt32(str2) != 0)
                        {
                            if (l > 0)
                                retsult = retsult + " و ";
                            retsult = retsult + hrf(l, lngh - 6, str2);
                            retsult = retsult + " میلیون";
                            l++;
                        }
                        start += lngh - 6;
                        lngh -= lngh - 6;
                    }
                    if (lngh > 3)
                    {
                        str2 = numberstr.Substring(start, lngh - 3);
                        if (Convert.ToInt32(str2) != 0)
                        {
                            if (l > 0)
                                retsult = retsult + " و ";
                            retsult = retsult + hrf(l, lngh - 3, str2);
                            retsult = retsult + " هزار";
                            l++;
                        }
                        start += lngh - 3;
                        lngh -= lngh - 3;
                    }
                    if (lngh > 0)
                    {
                        str2 = numberstr.Substring(start, lngh);
                        if (Convert.ToInt32(str2) != 0)
                        {
                            if (l > 0)
                                retsult = retsult + " و ";
                            retsult = retsult + hrf(l, lngh, str2);
                        }
                    }
                }
                catch
                {
                    retsult = "خطا";
                }
            }
            return retsult;
        }

        public static string NumberToCharacter(this long number)
        {
            return number.ToString().NumberToString();
        }


        public static bool IsNumber(this string number)
        {
            if (number == null)
                return false;
            number = number.Replace(",", "");
            long outValue = 0;
            return long.TryParse(number, out outValue);
        }



        public static string? ToStringNull(this object? value, string defualtValue = null)
        {
            if (value == null) return defualtValue;
            return value.ToString();
        }
        public static string? ListToString(this List<string>? value, char joinChar = ',')
        {
            if (value == null || value.Count == 0) return null;
            return string.Join(joinChar, value);
        }
        public static string? ArrayToString(this string[]? value, char joinChar = ',')
        {
            if (value == null || value.Count() == 0) return null;
            return string.Join(joinChar, value);
        }
        public static bool ToBoolNull(this string? value, bool def = false)
        {
            return ToBool(value.ToString(), def);
        }
        public static bool ToBool(this string value, bool? def = false)
        {
            if (value.IsNullEmpty()) return def.ToBool();
            return bool.Parse(value);
        }
        public static bool ToBool(this bool? value, bool defualtValue = false)
        {
            if (value == null) return defualtValue;
            return (bool)value;
        }

        public static decimal ToDecimal(this object value, decimal defualtValue = 0)
        {
            if (value == null)
                return defualtValue;
            decimal.TryParse(value.ToString(), out defualtValue);
            return defualtValue;
        }

        public static List<T>? ToList<T>(this object? value)
        {
            if (value == null) return null;
            return ((IEnumerable)value).Cast<T>().ToList();
        }
        public static long? ToLongNull(this object? value, int defualtValue = 0)
        {
            if (value == null) return defualtValue;
            return value.ToString().ToLong(defualtValue);
        }
        public static long ToLong(this string? value, long defualtValue = 0)
        {
            if (value == null)
                return defualtValue;
            value = value.Replace(",", "");
            long.TryParse(value, out defualtValue);
            return defualtValue;
        }
        public static long ToLongObject(this object? value, long defualtValue = 0)
        {
            if (value == null) return defualtValue;
            return value.ToString().ToLong(defualtValue);
        }

        public static object? GetPropertyValue(object? obj, string propName)
        {
            if (obj == null) return null;
            var p = obj.GetType().GetProperty(propName);
            if (p == null) return null;
            return p.GetValue(obj, null);
        }
        public static List<string>? GetCompaireObject(object? objOne, object? objTwo)
        {
            if (objOne == null || objTwo == null)
                return null;

            var result = new List<string>();

            var propertis = objOne.GetType().GetProperties();
            if (propertis != null)
            {
                foreach (var prop in propertis)
                {
                    var notmapOne = prop.GetCustomAttribute(typeof(NotMapAttribute));
                    if (notmapOne != null)
                        continue;
                    var oneValue = prop.GetValue(objOne);

                    var propTwo = objTwo.GetType().GetProperty(prop.Name);
                    if (propTwo != null)
                    {
                        var notmapTwo = propTwo.GetCustomAttribute(typeof(NotMapAttribute));
                        if (notmapTwo != null)
                            continue;
                        var twoValue = propTwo.GetValue(objTwo);
                        if (twoValue?.ToString() != oneValue?.ToString())
                            result.Add(prop.Name);
                    }
                }
            }
            return result;
        }
        public static string? ToSeprator(this long value)
        {
            return value.ToString("#,0");
        }
        public static string? ToSeprator(this int value)
        {
            return value.ToString("#,0");
        }
        public static int ToInt(this object? value, int defualtValue = 0)
        {
            if (value == null) return defualtValue;
            return value.ToString().ToInt(defualtValue);
        }
        public static int ToInt(this string? value, int defualtValue = 0)
        {
            if (value.IsNullEmpty())
                return defualtValue;
            value = value.Replace(",", "");
            int.TryParse(value, out defualtValue);
            return defualtValue;
        }
        public static int ToInt(this int? value, int defualtValue = 0)
        {
            if (value == null)
                return defualtValue;
            return (int)value;
        }
        public static string ByParameters(this string value, params string[] parameters)
        {
            return string.Format(value, string.Join(" ", parameters));
        }



        public static List<string> GetSelectFields<T>(this Expression<Func<T, object>> expre)
        {
            if (expre == null) return null;
            if (expre.Body is NewExpression)
            {
                var q = (expre.Body as NewExpression).Arguments.OfType<Expression>().ToList();
                return q.Select(a => a.ToString().Split('.')[1]).ToList();
            }
            else if (expre.Body is UnaryExpression)
            {
                var q = (expre.Body as UnaryExpression).Operand.ToString();
                return new List<string> { q.Split('.')[1] };
            }
            else if (expre.Body is MemberExpression)
            {
                var q = (expre.Body as MemberExpression).Member.Name;
                return new List<string> { q };
            }
            return null;
        }
        public static List<string> GetSelectFields<T>(this Func<T, object> expre)
        {
            if (expre == null) return null;
            if (expre is NewExpression)
            {
                var q = (expre as NewExpression).Arguments.OfType<Expression>().ToList();
                return q.Select(a => a.ToString().Split('.')[1]).ToList();
            }
            else if (expre is UnaryExpression)
            {
                var q = (expre as UnaryExpression).Operand.ToString();
                return new List<string> { q.Split('.')[1] };
            }
            else if (expre is MemberExpression)
            {
                var q = (expre as MemberExpression).Member.Name;
                return new List<string> { q };
            }
            return null;
        }

        public static List<string> GetExceptionMessage(this Exception exception, bool isInnerException = false)
        {
            if (exception == null) return null;

            var errors = new List<string>();

            errors.Add(exception.Message);

            if (isInnerException && exception.InnerException != null)
            {
                var result = GetExceptionMessage(exception.InnerException, isInnerException);
                errors.AddRange(result);
            }
            return errors;
        }


        public static string GetPropertyDisplayName(this Type type, string name)
        {
            MemberInfo property = type.GetProperty(name);
            var attribute = property
               .GetCustomAttributes(typeof(DisplayAttribute), true)
               .Cast<DisplayAttribute>().FirstOrDefault();
            return attribute?.Name;
        }
        public static string GetClassDisplayName(this Type type)
        {
            var attribute = type
               .GetCustomAttributes(typeof(DisplayAttribute), true)
               .Cast<DisplayAttribute>().FirstOrDefault();
            return attribute?.Name;
        }
        public static int GetTypeMode(Type o)
        {
            var code = Type.GetTypeCode(o);
            if (o.IsGenericType && !o.IsNullableEnum())
            {
                Type type1 = o.GetGenericArguments()[0];
                code = Type.GetTypeCode(type1);
            }

            switch (code)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return 1;
                case TypeCode.String:
                case TypeCode.Char: return 2;
                case TypeCode.DateTime: return 3;
                case TypeCode.Boolean: return 4;
                case TypeCode.Object: return 5;
                default:
                    return 0;
            }
        }

        public static bool IsNullableEnum(this Type type)
        {
            Type u = Nullable.GetUnderlyingType(type);
            return u != null && u.IsEnum;
        }
    }

}
