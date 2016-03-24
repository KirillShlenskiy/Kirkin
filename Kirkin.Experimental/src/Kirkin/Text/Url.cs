using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kirkin.Text
{
    /// <summary>
    /// URL string helpers.
    /// </summary>
    internal static class Url
    {
        public static string Encode(string url, params UrlArg[] args)
        {
            var sb = new StringBuilder();

            foreach (var arg in args)
            {
                // Skip empty values.
                if (!string.IsNullOrEmpty(arg.Value))
                {
                    sb.Append(sb.Length == 0 ? '?' : '&');
                    sb.Append(WebUtility.UrlEncode(arg.Name));
                    sb.Append("=");
                    sb.Append(WebUtility.UrlEncode(arg.Value));
                }
            }

            url = WebUtility.UrlEncode(url);

            return sb.Length == 0
                ? url
                : url + sb.ToString();
        }
    }

    /// <summary>
    /// Betfair-specific URL param formatting.
    /// </summary>
    internal class UrlArg
    {
        public string Name { get; }
        public string Value { get; }

        public UrlArg(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public UrlArg(string name, IEnumerable<string> values)
        {
            Name = name;
            Value = string.Join(",", values);
        }

        public UrlArg(string name, DateTime value)
        {
            if (value != value.Date) {
                throw new ArgumentException("Date value should not include time portion.");
            }

            Name = name;
            Value = value == DateTime.MinValue ? null : value.ToString("yyyy-MM-dd");
        }
    }
}
