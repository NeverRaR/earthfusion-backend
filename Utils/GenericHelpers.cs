using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Net.Mail;
using System.Security.Cryptography;

namespace Utils
{
    public enum PasswordScore
    {
        Blank = 0,
        VeryWeak = 1,
        Weak = 2,
        Medium = 3,
        Strong = 4,
        VeryStrong = 5
    }
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
                return true;
            }
            return false;
        }

        public static PasswordScore CheckStrength(string password)
        {
            int score = 0;

            if (password.Length < 1)
                return PasswordScore.Blank;
            if (password.Length < 4)
                return PasswordScore.VeryWeak;
            
            if (password.Length >= 8)
                score++;
            if (password.Length >= 12)
                score++;
            if (Regex.Match(password, @"^*.\d+", RegexOptions.ECMAScript).Success)
                score++;
            if (Regex.Match(password, @"^*.[a-z]+", RegexOptions.ECMAScript).Success &&
              Regex.Match(password, @"^*.[A-Z]+", RegexOptions.ECMAScript).Success)
                score++;
            if (Regex.Match(password, @"^*.[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]+", RegexOptions.ECMAScript).Success)
                score++;

            return (PasswordScore)score;
        }
        public static bool IsEmailAddressValid(string emailAddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailAddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public static string ComputeSha256Hash(string rawData)  
        {  
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())  
            {  
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
  
                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();  
                for (int i = 0; i < bytes.Length; i++)  
                {  
                    builder.Append(bytes[i].ToString("x2"));  
                }  
                return builder.ToString();  
            }  
        }
    }
}