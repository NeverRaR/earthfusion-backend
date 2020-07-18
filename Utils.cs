using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Net.Mail;

namespace Utils
{
    class Utils
    {
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
    }
}