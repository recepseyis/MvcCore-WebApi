using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace WebUI.Helper
{
    public static class PasswordReset
    {
        public static void PasswordResetSendEmail(string kime, string link)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            mail.From = new MailAddress("stealtman@gmail.com");
            mail.To.Add(kime);
            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential("stealtman@gmail.com", "Fujitsu.zb8702/");
            mail.IsBodyHtml = true;
            SmtpServer.EnableSsl = true;
            mail.Subject = "Şifre Kurtarma ";
            mail.Body = $"<h2> Şifrenizi Yenilemek için <a href='{link}'>tıklayınız.</a></h2><hr/>";
            SmtpServer.Send(mail);
        }
    }
}
