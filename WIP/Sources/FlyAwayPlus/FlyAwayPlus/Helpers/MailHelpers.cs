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
    public class MailHelpers : SingletonBase<MailHelpers>
    {
        private const string SenderId = "flyawayplus.system@gmail.com";
        private const string SenderPassword = "doan2015";
        private readonly SmtpClient _smtp;

        private MailHelpers()
        {
            _smtp = new SmtpClient
            {
                Host = "smtp.gmail.com", // smtp server address here…
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(SenderId, SenderPassword),
                Timeout = 30000,
            };
        }

        public void SendMailWelcome(string email, string firstName, string lastName)
        {
            try
            {
                var message = new MailMessage(SenderId, email, "Getting started on FlyAwayPlus",
                    "Welcome to FlyAwayPlus, " + firstName + " " + lastName + "!" + "\nJoin us now at: http://flyaway.ga");
                _smtp.Send(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        public void SendMailWarningReportPost(String url, String email)
        {
            try
            {
                var message = new MailMessage(SenderId, email, "Report post",
                    "Your post is reported\nLink: "+url);
                _smtp.Send(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        public void SendMailWarningReportUser(string email)
        {
            try
            {
                var message = new MailMessage(SenderId, email, "Report account",
                    "Your account is reported");
                _smtp.Send(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        public void SendMailResetPassword(string email, string newPassword)
        {
            try
            {
                var message = new MailMessage(SenderId, email, "Reset password FlyAwayPlus",
                        "Your new password: " + newPassword);
                _smtp.Send(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }
    }
}