using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CrystalOcean.Web.Configuration;
using Microsoft.Extensions.Options;

namespace CrystalOcean.Web.Services
{
    public class AuthMessageSender : IEmailSender
    {
        public AuthMessageSender(IOptions<AuthMessageSenderSettings> settings)
        {
            this.Settings = settings.Value;
        }

        public Task SendEmailAsync(String email, String subject, String message)
        {
            SmtpClient client = new SmtpClient(this.Settings.Host, this.Settings.Port);
            client.Timeout = 3000;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(this.Settings.Username, this.Settings.Password);
            client.EnableSsl = this.Settings.UseSSL;

            MailMessage msg = new MailMessage(this.Settings.FromEmail, email, subject, message);
            msg.Priority = (MailPriority)this.Settings.Priority;
            return client.SendMailAsync(msg);
        }

        public AuthMessageSenderSettings Settings { get; }
    }
}