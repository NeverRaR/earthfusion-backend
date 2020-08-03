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
    }
}