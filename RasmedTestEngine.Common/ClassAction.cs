//Sample license text.

using System;

namespace RasmedTestEngine.Common
{

    public static class ClassAction
    {
        public static string TruncateAtWord(this string value, int length)
        {
            if (((value == null) || (value.Length < length)) || (value.IndexOf(" ", length, StringComparison.Ordinal) == -1))
            {
                return value;
            }
            return value.Substring(0, value.IndexOf(" ", length, StringComparison.Ordinal));
        }
    }
}

