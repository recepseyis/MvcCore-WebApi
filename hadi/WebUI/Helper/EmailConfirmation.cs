using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace WebUI.Helper
{
    public static class EmailConfirmation
    {
        public static void EmailConfirmForSendEmail(string kime, string link)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            mail.From = new MailAddress("stealtman@gmail.com");
            mail.To.Add(kime);
            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential("stealtman@gmail.com", "Fujitsu.zb8702/");
            mail.IsBodyHtml = true;
            SmtpServer.EnableSsl = true;
            mail.Subject = "Email Doğrulama";
            mail.Body = $"<h2> Email Adresinizi Doğrulamak için linke <a href='{link}'>tıklayınız.</a></h2><hr/>";
            SmtpServer.Send(mail);
        }
    }
}
