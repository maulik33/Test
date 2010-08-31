using System;

namespace Emailer.Utilities
{
    public static class EmailUtilities
    {
        public static DateTime SafeGetDateTime(object input)
        {
            DateTime result;
            if (DateTime.TryParse(input.ToString(), out result))
                return result;
            return DateTime.MinValue;
        }
    }
}
