using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.Common.Extensions
{
    public static class StringExtensions
    {
        public static string ToTitleCase(this string value)
        {
            if(string.IsNullOrWhiteSpace(value))
                return value;

            return System.Globalization.CultureInfo.CurrentCulture
               .TextInfo.ToTitleCase(value.ToLower());
        }
    }
}
