using Bolao10.Persistence.Repository;
using System;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Bolao10.Services
{
    public class MailService
    {
        
        public MailService()
        {

        }

        public void Send(SendGridMessage mailmessage)
        {
            try
            {

                var apiKey = "";

                var client = new SendGridClient(apiKey);

                var response = client.SendEmailAsync(mailmessage).Result;

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Serviço de envio de email inoperante.");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Send(MailMessage mailmessage)
        {
            try
            {

                var apiKey = "";

                var client = new SendGridClient(apiKey);

                var from = new EmailAddress(mailmessage.From.Address, mailmessage.From.DisplayName);
                string subject = mailmessage.Subject;
                var to = new EmailAddress(mailmessage.To[0].Address);
                var plainTextContent = "";
                var htmlContent = mailmessage.Body;
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = client.SendEmailAsync(msg).Result;

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Serviço de envio de email inoperante.");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}