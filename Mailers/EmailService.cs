using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MailKit.Net.Smtp;

namespace Inventory.Mailers
{
    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;

        public EmailService(string smtpHost, int smtpPort)
        {
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
        }

        public async Task SendPasswordResetEmail(string email, string token, string resetUrl)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Your Application", "noreply@yourdomain.com"));
            message.To.Add(new MailboxAddress("", email)); // Receiver's email address
            message.Subject = "Password Reset Request";

            var bodyBuilder = new BodyBuilder();
            //bodyBuilder.HtmlBody = $"Click the link below to reset your password:<br/><a href=\"{resetUrl}\">{resetUrl}</a>";
            bodyBuilder.HtmlBody = $"Link Start:\"{resetUrl}\":End";
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // Update the SMTP settings to point to MailHog
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(_smtpHost, _smtpPort, false);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        // Add other email sending methods as needed
    }
}