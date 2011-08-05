using System;
using System.Text;

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

        public static string ToAcronymProper(string stringToConvert)
        {
            StringBuilder strb = new StringBuilder();
            for (int charIndex = 0; charIndex < stringToConvert.Length; charIndex++)
            {
                bool insertSpace = false;
                if (Char.IsUpper(stringToConvert[charIndex]) && charIndex > 0)
                {
                    // If Prev Char is Upper, dont insert space
                    if (false == Char.IsUpper(stringToConvert[charIndex - 1]))
                    {
                        insertSpace = true;
                    }
                    else
                    {
                        if (charIndex < stringToConvert.Length - 1)
                        {
                            // If Next Char is Upper, dont insert space
                            insertSpace = !(Char.IsUpper(stringToConvert[charIndex + 1]));
                        }
                    }
                }
                if (insertSpace)
                {
                    strb.Append(" ");
                }
                strb.Append(stringToConvert[charIndex]);
            }
            return strb.ToString();
        }
    }
}
