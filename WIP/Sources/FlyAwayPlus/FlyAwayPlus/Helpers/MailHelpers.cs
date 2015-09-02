using System.Net;
using System.Net.Mail;
using System;

namespace FlyAwayPlus.Helpers
{
    public class MailHelpers : SingletonBase<MailHelpers>
    {
        private const string SenderId = "flyawayplus.system@gmail.com";
        private const string SenderPassword = "doan2015";
        private readonly SmtpClient _smtp;
        private readonly MailMessage _mail;

        private MailHelpers()
        {
            _mail = new MailMessage();
            _smtp = new SmtpClient
            {
                Host = "smtp.gmail.com", // smtp server address here…
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(SenderId, SenderPassword),
                Timeout = 30000,
            };
            _mail.From = new MailAddress(SenderId);
            _mail.IsBodyHtml = true;
        }

        public void SendMailWelcome(string email, string firstName, string lastName)
        {
            try
            {
                _mail.To.Add(email);
                _mail.Subject = "An account has been created for you";
                _mail.Body = "Welcome " + firstName + " " + lastName + " to FlyAwayPlus. " +
                            "<br/> You'll be excited to know a user account was just created for you at <a href='http://flyaway.ga/'>FlyAwayPlus website</a>." +
                            "<br/> <br/> Get Started Now" +
                            "<br/> <br/> If you have any technical difficulties,  just click the Contact Us link at the bottom of the Help page to contact your Program Administrator to get additional support." +
                            "<br/> If you have any questions, please mail us at: <a href='mailto:flyawayplus.system@gmail.com' target='_top'>flyawayplus.system@gmail.com</a> " +
                            "<br/> <br/> Thank you, <br/> <a href='http://flyaway.ga/'>FAP - FlyAwayPlus</a>";
                _smtp.Send(_mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        public void SendMailWarningReportPost(String url, String email, int userReportedId)
        {
            try
            {
                var user = GraphDatabaseHelpers.Instance.FindUser(userReportedId);
                _mail.To.Add(email);
                _mail.Subject = "Your post at FlyAwayPlus has been reported";
                _mail.Body = "Hello " + user.FirstName + " " + user.LastName + "," +
                            "<br/> We are administrator of <a href='http://flyaway.ga/'>FlyAwayPlus website</a>." +
                            "<br/> <br/> We would like to inform you that" +
                            "<br/> <br/> Your post at <a href='" + url + "'>this</a> has been reported by another user." +
                            "<br/> If you have any questions, please mail us at: <a href='mailto:flyawayplus.system@gmail.com' target='_top'>flyawayplus.system@gmail.com</a> " +
                            "<br/> <br/> Thank you, <br/> <a href='http://flyaway.ga/'>FAP - FlyAwayPlus</a>";
                _smtp.Send(_mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        public void SendMailWarningReportUser(string email, int userReportedId)
        {
            try
            {
                var user = GraphDatabaseHelpers.Instance.FindUser(userReportedId);
                _mail.To.Add(email);
                _mail.Subject = "Your account at FlyAwayPlus has been reported";
                _mail.Body = "Hello " + user.FirstName + " " + user.LastName + "," +
                            "<br/> We are administrator of <a href='http://flyaway.ga/'>FlyAwayPlus website</a>." +
                            "<br/> <br/> We would like to inform you that" +
                            "<br/> <br/> Your account has been reported by another user." +
                            "<br/> If you have any questions, please reply this email." +
                            "<br/> <br/> Thank you, <br/> <a href='http://flyaway.ga/'>FAP - FlyAwayPlus</a>";
                _smtp.Send(_mail);
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