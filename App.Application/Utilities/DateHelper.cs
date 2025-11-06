using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace App.Application.Utilities
{
    public static class DateHelper
    {
        public static string? ToMd5(this string? value) //Encrypt using MD5   
        {
            if (value == null) return null;

            MD5 md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ASCIIEncoding.Default.GetBytes(value)));
        }
        public static string? ToHash(this string? value) //Encrypt using MD5   
        {
            if (value == null) return null;

            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
        }
        public static string ToTimeStamp(this object value)
        {
            string result = string.Empty;
            if (value == null ||  value.IsNullEmpty()) return result;
            DateTime dt = DateTime.Parse(value.ToString());
            DateTime dtut = DateTime.SpecifyKind(dt, DateTimeKind.Utc);

            result = new DateTimeOffset(dtut).ToUnixTimeMilliseconds().ToString();

            return result;
        }
        public static long? ToTimeStampLong(this object? value)
        {
            long? result = null;
            if (value == null ||  value.IsNullEmpty()) return result;
            DateTime dt = DateTime.Parse(value.ToString());
            DateTime dtut = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            result = new DateTimeOffset(dtut).ToUnixTimeMilliseconds();
            return result;
        }
        public static string ToShamsiTime(this DateTime value)
        {
            if (value == null)
                return string.Empty;

            PersianCalendar pc = new PersianCalendar();
            return value.ToString("HH:mm");
        }
        public static string ToShamsiDateTime(this DateTime value)
        {
            if (value == null)
                return string.Empty;

            try
            {
                PersianCalendar pc = new PersianCalendar();
                return pc.GetYear(value) + "/"
                       + pc.GetMonth(value).ToString("00") + "/"
                       + pc.GetDayOfMonth(value).ToString("00") + " " + value.ToString("HH:mm");
            }
            catch
            {
                return string.Empty;
            }
        }
        public static string ToShamsiDateTimeNull(this DateTime? value)
        {
            if (value == null)
                return string.Empty;

            try
            {
                PersianCalendar pc = new PersianCalendar();
                return
                      pc.GetYear(((DateTime)value)) + "/"
                       + pc.GetMonth(((DateTime)value)).ToString("00") + "/"
                       + pc.GetDayOfMonth(((DateTime)value)).ToString("00") + " " + ((DateTime)value).ToString("HH:mm");
            }
            catch
            {
                return string.Empty;
            }
        }
        public static int ToDayOfDateNull(this string? value, int defaultValue = 0)
        {
            if (value.IsNullEmpty())
                return defaultValue;

            return value.Substring(8, 2).ToInt();
        }
        public static int ToYearOfDateNull(this string? value, int defaultValue = 0)
        {
            if (value.IsNullEmpty())
                return defaultValue;

            return value.Substring(0, 4).ToInt();
        }
        public static int ToMounthOfDateNull(this string? value, int defaultValue = 0)
        {
            if (value.IsNullEmpty())
                return defaultValue;

            return value.Substring(5, 2).ToInt();
        }
        public static string ToShamsiDateNull(this DateTime? value)
        {
            if (value == null)
                return string.Empty;

            try
            {
                PersianCalendar pc = new PersianCalendar();
                return
                      pc.GetYear(((DateTime)value)) + "/"
                       + pc.GetMonth(((DateTime)value)).ToString("00") + "/"
                       + pc.GetDayOfMonth(((DateTime)value)).ToString("00");
            }
            catch
            {
                return string.Empty;
            }
        }
        public static string ToShamsiDateTime(this string value)
        {
            DateTime data = DateTime.Now;
            try
            {
                data = DateTime.Parse(value);

                if (value == null)
                    return string.Empty;

                PersianCalendar pc = new PersianCalendar();
                return pc.GetYear(data) + "/"
                       + pc.GetMonth(data).ToString("00") + "/"
                       + pc.GetDayOfMonth(data).ToString("00") + " " + data.ToString("HH:mm");
            }
            catch
            {
                return string.Empty;
            }
        }
        public static string ToShamsiDate(this DateTime value)
        {
            if (value == null)
                return string.Empty;

            PersianCalendar pc = new PersianCalendar();
            return pc.GetYear(value) + "/"
                                     + pc.GetMonth(value).ToString("00") + "/" +
                                     pc.GetDayOfMonth(value).ToString("00");
        }
        public static string ToShamsiDate(this string value)
        {
            if (value == null)
                return string.Empty;
            DateTime dt = DateTime.Parse(value);
            PersianCalendar pc = new PersianCalendar();
            return pc.GetYear(dt) + "/"
                                     + pc.GetMonth(dt).ToString("00") + "/" +
                                     pc.GetDayOfMonth(dt).ToString("00");
        }



        public static bool IsPersianDate(int year, int month, int day)
        {
            if (year > 1300 && month < 1600 && month > 0 && month < 13 && day > 0 && day < 32)
                return true;
            return false;
        }
        public static DateTime? ToGregorianDate(int yearFa, int monthFa, int dayFa)
        {
            if (yearFa > 1300 && monthFa < 1600 && monthFa > 0 && monthFa < 13 && dayFa > 0 && dayFa < 32)
                return ToGregorianDate($"{yearFa.ToString("0000")}/{monthFa.ToString("00")}/{dayFa.ToString("00")}");
            return null;
        }
        public static DateTime? ToGregorianDate(this string? value)
        {
            value = value.ToFix();
            if (value.IsNullEmpty() || value.Length != 10)
                return null;

            return new DateTime(int.Parse(value.Substring(0, 4)), int.Parse(value.Substring(5, 2)), int.Parse(value.Substring(8, 2)), new PersianCalendar());
        }
        public static string? ToGregorianDate(this string? value, string format = "yyyy/MM/dd")
        {
            return ToGregorianDate(value)?.ToString(format);
        }

        public static bool IsDate(this string date)
        {
            try
            {
                date = date.ToFix().Replace("-", "/");
                if (date.Replace("/", "").Length < 8 || date.Replace("/", "").Length > 8)
                    return false;

                DateTime dt = DateTime.Parse(date);
            }
            catch
            {
                return false;
            }
            return true;
        }


        public static bool IsGuid(this string value)
        {
            try
            {
                Guid id = Guid.Parse(value);
            }
            catch
            {
                return false;
            }
            return true;
        }


        public static int DifferenceMonth(this DateTime startDate, DateTime endDate)
        {
            return (startDate.Month - endDate.Month) + 12 * (startDate.Year - endDate.Year);
        }



        public static string ToDateFormat(this string value, string format = "yyy/MM/dd")
        {
            if (value == null)
                return string.Empty;

            DateTime dt = DateTime.Parse(value);
            return dt.ToString(format);
        }
        public static string ToDateFormat(this DateTime value, string format = "yyy/MM/dd")
        {
            if (value == null)
                return string.Empty;

            return value.ToString(format);
        }
        public static string ToDateFormat(this DateTime? value, string format = "yyy/MM/dd")
        {
            if (value == null)
                return string.Empty;

            return ((DateTime)value).ToString(format);
        }

        public static string AddMonth(this string date, int value)
        {
            if (date.IsNullEmpty()) return string.Empty;
            string result = string.Empty;
            int year = int.Parse(date.Substring(0, 4));
            int month = int.Parse(date.Substring(5, 2)) + value;
            int day = int.Parse(date.Substring(8, 2));

            if (month > 12)
            {
                year += month / 12;
                month = month % 12;
            }
            return string.Format("{0}/{1}/{2}", year, month.ToString("00"), day.ToString("00"));
        }

    }

}
