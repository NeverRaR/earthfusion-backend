using System;
using System.Text.RegularExpressions;

namespace Utils
{
    class GenericHelpers
    {
        public static string passwdPattern = "^(?=(.*\\d){2})(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z\\d]).{8,}$";
        public static bool CheckPasswordStrength(string passwdToCheck)
        {
            // call Regex.Match.
            Match match = Regex.Match(passwdToCheck, passwdPattern,
                RegexOptions.IgnoreCase);

            // check the Match for Success.
            if (match.Success)
            {
                // Part 4: get the Group value and display it.
                return true;
            }
            return false;
        }
    }
}