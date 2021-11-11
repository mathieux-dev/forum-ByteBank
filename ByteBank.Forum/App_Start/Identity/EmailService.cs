using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace ByteBank.Forum.App_Start.Identity
{
    public class EmailService : IIdentityMessageService
    {
        private readonly string ORIGIN_EMAIL = ConfigurationManager.AppSettings["emailService:origin_email"];
        private readonly string PASSWORD_EMAIL = ConfigurationManager.AppSettings["emailService:password_email"];
        public async Task SendAsync(IdentityMessage message)
        {
            using (var emailMessage = new MailMessage())
            {
                emailMessage.From = new MailAddress(ORIGIN_EMAIL);

                emailMessage.Subject = message.Subject;
                emailMessage.To.Add(message.Destination);
                emailMessage.Body = message.Body;

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = new NetworkCredential(ORIGIN_EMAIL, PASSWORD_EMAIL);
                    
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Host = "smtp.gmail.com";
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 20_000;

                    await smtpClient.SendMailAsync(emailMessage);
                }
            }
        }
    }
}