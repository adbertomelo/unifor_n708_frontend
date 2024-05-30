using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bolao10.Persistence.Repository;
using Bolao10.Model.Entities;
using System.Net.Mail;
using System.IO;
using System.Text;
using Bolao10.Services;
using SendGrid.Helpers.Mail;

namespace Bolao10.Services
{
    public class AdminService
    {

        public void EnviarEmail(string email, string nome, int rodadaId)
        {

            try
            {
                SendGridMessage mailMessage = GetEmail(email, nome, rodadaId);

                MailService _mailService = new MailService();

                _mailService.Send(mailMessage);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            

        }

        private SendGridMessage GetEmail(string email, string nome, int rodadaId)
        {
            ParametroRepository _parametroRepository = new ParametroRepository();

            Parametros p = _parametroRepository.FindOne();

            var from = new EmailAddress(p.EmailAdmin, "Administrador do Bolão10");

            string subject = "Apostas Pendentes";

            var to = new EmailAddress(email);

            var plainTextContent = "";

            string textoEmail = GetTextoEmail();

            RodadaRepository rodadaRepository = new RodadaRepository();

            Rodada rodada = rodadaRepository.GetById(rodadaId);

            var htmlContent = String.Format(textoEmail, nome, rodada.Nome, rodada.DataEncerramento);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            return msg;

        }
        /*private MailMessage GetEmail(string email, string nome, int rodadaId)
        {

            try
            {
                MailMessage mailmessage = new MailMessage();
                
                ParametroRepository _parametroRepository = new ParametroRepository();

                Parametros param = _parametroRepository.FindOne();

                mailmessage.From = new MailAddress(String.Format("\"{0} - Administrador do Bolão10\"{1}", nome, param.EmailAdmin));

                mailmessage.To.Add(new MailAddress(email));

                mailmessage.Subject = String.Format("Apostas Pendentes");

                string textoEmail = GetTextoEmail();

                RodadaRepository rodadaRepository = new RodadaRepository();

                Rodada rodada = rodadaRepository.GetById(rodadaId);

                //mailmessage.Body = String.Format(textoEmail, nome, rodada.Nome, rodada.Fase.Nome, rodada.DataEncerramento);
                mailmessage.Body = String.Format(textoEmail, nome, rodada.Nome, rodada.DataEncerramento);

                mailmessage.IsBodyHtml = true;

                return (mailmessage);

            }
            catch(Exception ex)
            {
                throw ex;
            }

        }*/

        private string GetTextoEmail()
        {
            string path = HttpContext.Current.Request.PhysicalApplicationPath;

            StreamReader sr = new StreamReader(path + "apostaspendentes.html", Encoding.GetEncoding("ISO-8859-1"));

            string texto = sr.ReadToEnd();

            sr.Close();

            return (texto);


        }

        public void AtualizarSituacaoDeEnvio(Convite convite)
        {
            try
            {
                ConviteRepository _conviteRepository = new ConviteRepository();
                convite.EmailEnviado = 1;
                _conviteRepository.Save(convite);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        

    }
}