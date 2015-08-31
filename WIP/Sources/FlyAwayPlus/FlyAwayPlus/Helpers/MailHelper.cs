using System.Net;
using System.Net.Mail;
using FlyAwayPlus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace FlyAwayPlus.Helpers
{
    public class MailHelper
    {
        private const string User = "user";
        private const string SenderId = "flyawayplus.system@gmail.com";
        private const string SenderPassword = "doan2015";
        public static void SendMailWelcome(string email)
        {
            try
            {
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // smtp server address here…
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(SenderId, SenderPassword),
                    Timeout = 30000,
                };
                var message = new MailMessage(SenderId, email, "Report post",
                    "Your post is reported");
                smtp.Send(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        public static void SendMailWarningReportPost(String email)
        {
            try
            {
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // smtp server address here…
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(SenderId, SenderPassword),
                    Timeout = 30000,
                };
                var message = new MailMessage(SenderId, email, "Report post",
                    "Your post is reported");
                smtp.Send(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        public static void SendMailWarningReportUser(string email)
        {
            try
            {
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // smtp server address here…
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(SenderId, SenderPassword),
                    Timeout = 30000,
                };
                var message = new MailMessage(SenderId, email, "Report account",
                    "Your account is reported");
                smtp.Send(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }
    }
}