using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mandrill;
using Mandrill.Models;
using Mandrill.Requests.Messages;

namespace RasmedTestEngine.Common.Email
{
    public class MailPusher
    {

        private readonly MandrillApi _mandrill;

        public MailPusher()
        {
             _mandrill = new MandrillApi("GhvvPv11lJzoVE3jpDotTA");
        }

        private const string EmailFromAddress = "no-reply@domain.com";
        private const string EmailFromName = " HR TEAM";





        public Task SendMail(MailModel mm)
        {
            var msg = new EmailMessage();
            var recips = new List<EmailAddress> {new EmailAddress(mm.To)};
            msg.Html = mm.Message;
            msg.Subject = mm.Subject;
            msg.To = recips;
            msg.FromName = EmailFromName;
            msg.FromEmail = EmailFromAddress;

            return  _mandrill.SendMessage(new SendMessageRequest(msg));
        }
     

      
        public void SendEmail(string to,string name, string bcc, string subject, string message)
        {
            var msg = new MailModel
            {
               
                To = to,
                Bcc = bcc,
                Name = name,
                Message = message,
                Subject = subject
            };
            
            ThreadPool.QueueUserWorkItem(stateObject =>  SendMail(msg));
        }
    }
}