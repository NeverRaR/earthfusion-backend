// using SendGrid's C# Library
// https://github.com/sendgrid/sendgrid-csharp
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace Utils
{
    class SendgridHelpers
    {
        public static void TestSend(string receiver)
        {
            Test(receiver).Wait();
        }

        private static async Task Test(string receiver)
        {
            var apiKey = Environment.GetEnvironmentVariable("EARTH_FUSION_EMAIL_API_KEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("earth-fusion@anzupop.com", "Developers");
            var subject = "Hello from Earth Fusion";
            var to = new EmailAddress(receiver, "Tester");
            var plainTextContent = "";
            var htmlContent = "Your verification code is <strong>1145141919810</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

        public static void SendVerificationCodeTask(string receiver, string nickname, string verificationCodeString)
        {
            SendWithVerificationTemplate(receiver, nickname, verificationCodeString).Wait();
        }

        private static async Task SendWithVerificationTemplate(string receiver, string nickname, string verificationCodeString)
        {
            var apiKey = Environment.GetEnvironmentVariable("EARTH_FUSION_EMAIL_API_KEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("earth-fusion@anzupop.com", "Administrators");
            var subject = "Your Earth Fusion verification code";
            var to = new EmailAddress(receiver, nickname);
            var plainTextContent = "";
            var htmlContent = "Your verification code is <strong>" + verificationCodeString + "</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}