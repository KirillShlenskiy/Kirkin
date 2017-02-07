using System;
using System.Text;

namespace Kirkin.Text
{
    /// <summary>
    /// Common URL string utility methods.
    /// </summary>
    public static class UrlUtil
    {
        const char Delimiter = '/';

        /// <summary>
        /// Combines the given URL segments.
        /// </summary>
        public static string Combine(params string[] urlSegments)
        {
            // Validation.
            if (urlSegments == null) throw new ArgumentNullException("urlSegments");

            if (urlSegments.Length < 2) {
                throw new ArgumentException(string.Format("An invalid number of URL segments supplied: {0}.", urlSegments.Length));
            }

            // Build URL.
            StringBuilder sb = new StringBuilder();

            foreach (string urlSegment in urlSegments)
            {
                ValidateSegment(urlSegment);

                if (urlSegment.Length == 0) {
                    continue;
                }

                if (sb.Length != 0)
                {
                    if (sb[sb.Length - 1] == Delimiter)
                    {
                        if (urlSegment[0] == Delimiter)
                        {
                            // Delimiter collision: trim end.
                            sb.Length--;
                        }
                    }
                    else if (urlSegment[0] != Delimiter)
                    {
                        // No delimiter at the end of previous
                        // segment, or start of the new one.
                        sb.Append(Delimiter);
                    }
                }

                sb.Append(urlSegment);
            }

            return sb.ToString();
        }

        private static void ValidateSegment(string urlSegment)
        {
            if (urlSegment == null) throw new ArgumentNullException();

            if (urlSegment.StartsWith("//")) {
                throw new ArgumentException("URL segment format invalid: double '/' detected at start of segment.");
            }

            if (urlSegment.EndsWith("//") && !urlSegment.EndsWith("://")) {
                throw new ArgumentException("URL segment format invalid: double '/' detected at end of segment.");
            }
        }
    }
}