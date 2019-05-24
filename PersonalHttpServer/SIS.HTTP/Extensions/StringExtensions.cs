using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.HTTP.Extensions
{
    public static class StringExtensions
    {
        public static string Capitalize(this string input)
        {

            StringBuilder capitalized = new StringBuilder();

            capitalized.Append(input[0].ToString().ToUpper());
            capitalized.Append(input.Substring(1).ToLower());

            return capitalized.ToString().Trim();
        }
    }
}
