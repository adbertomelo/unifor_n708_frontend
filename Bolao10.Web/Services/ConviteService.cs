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
using System.Collections;

namespace Bolao10.Services
{
    public class ConviteService
    {
        const int CON_ID_PARAMETROS = 1;
        const int CON_STATUS_ACEITO = 1;


        //private log4net.ILog _logger;

        public void ValidarConvite(Convite convite)
        {
            if (convite == null)
                throw new Exception("O convite informado não existe.");

            if (convite.Status == 1)
                throw new Exception("O convite informado já foi aceito.");

            if (convite.Status == 2)
                throw new Exception("O convite informado foi cancelado.");

            if (convite.Status != 0)
                throw new Exception("O convite informado já foi aceito ou cancelado");

            Parametros parametros = new ParametroRepository().FindOne();

            if (DateTime.Today >= parametros.DataFinalConvites)
                throw new Exception("A prazo para a aceitação do convite expirou. Você não poderá mais se cadastrar no bolão.");


        }

        public void ValidarConvite(Guid codConvite)
        {
            Convite convite = new ConviteRepository().GetByKey(codConvite);
            
            ValidarConvite(convite);
        }

        public void ValidarConvidado(string email)
        {

            try
            {

                Convite convite = new ConviteRepository().FindByEmailConvidado(email);

                if (convite != null)
                {
                    throw new Exception("E-mail do convidado já existe!");
                }
                
                Usuario usuario = new UsuarioRepository().FindByEmail(email);

                Bolao bolao = new BolaoRepository().FindAtivo();

                Participante participante = new ParticipanteRepository().FindByUsuarioAndBolao(usuario, bolao);

                if (participante != null)
                {
                    throw new Exception("Já existe um participante cadastrado neste bolão com o e-mail informado");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public Convite GetNovoConvite(string email, string nome, Participante participante)
        {
            try
            {

                Convite convite = new Convite(participante, nome, email, DateTime.Now);

                ConviteRepository _conviteRepository = new ConviteRepository();

                _conviteRepository.SaveOrUpdate(convite);

                return (convite);

            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        public void EnviarConvite(Convite convite, string path)
        {

            try
            {
                SendGridMessage email = GetEmail(convite, path);

                MailService _mailService = new MailService();

                _mailService.Send(email);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            

        }

        private SendGridMessage GetEmail(Convite convite, string path)
        {
            Parametros p = new ParametroRepository().GetById(CON_ID_PARAMETROS);

            var from = new EmailAddress(p.EmailAdmin, $"{convite.Participante.Usuario.Nome} - Participante do Bolão10");
            
            string subject = $"Participação no {convite.Participante.Bolao.Descricao}"; 
            
            var to = new EmailAddress(convite.EmailConvidado);
            
            var plainTextContent = "";
            
            string textoEmail = GetTextoEmail(path);

            var htmlContent = string.Format(textoEmail, convite.NomeConvidado, convite.Participante.Usuario.Nome, convite.Participante.Bolao.Descricao, convite.Id, convite.Participante.Usuario.Nome, convite.Participante.Usuario.Email); ;
            
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            string file = path + p.NomeArquivoRegras.Trim();

            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                MemoryStream memoryStream = new MemoryStream();

                stream.CopyTo(memoryStream);

                byte[] bytes = memoryStream.ToArray();

                string fileName = new FileInfo(file).Name;

                var attachment = new SendGrid.Helpers.Mail.Attachment
                {
                    Filename = fileName,
                    Content =  Convert.ToBase64String(bytes),
                    Type = MimeMapping.GetMimeMapping(file),
                    Disposition = "attachment",
                    ContentId = fileName
                };

                msg.AddAttachment(attachment);
                
            }


            return msg;

        }

        /*private MailMessage GetEmail(Convite convite, string path)
        {

            try
            {
                MailMessage mailmessage = new MailMessage();

                Parametros p = new ParametroRepository().GetById(CON_ID_PARAMETROS);

                string file =  path + p.NomeArquivoRegras.Trim();

                System.Net.Mail.Attachment data = new System.Net.Mail.Attachment(file);

                mailmessage.Attachments.Add(data);

                mailmessage.From = new MailAddress(String.Format("\"{0} - Participante do Bolão10\"{1}", convite.Participante.Usuario.Nome, p.EmailAdmin));

                mailmessage.To.Add(new MailAddress(convite.EmailConvidado));

                mailmessage.Subject = String.Format("Participação no {0}", convite.Participante.Bolao.Descricao);

                string textoEmail = getTextoEmail(path);

                mailmessage.Body = String.Format(textoEmail, convite.NomeConvidado, convite.Participante.Usuario.Nome, convite.Participante.Bolao.Descricao, convite.Id, convite.Participante.Usuario.Nome, convite.Participante.Usuario.Email);

                mailmessage.IsBodyHtml = true;

                return (mailmessage);

            }
            catch(Exception ex)
            {
                throw ex;
            }

        }*/

        private string GetTextoEmail(string pathModelo)
        {
            StreamReader sr = new StreamReader(pathModelo + "convite.html", Encoding.GetEncoding("ISO-8859-1"));

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

        public void AtualizarSituacaoLista(string emailConvidado)
        {

            try
            {
                ConviteRepository _conviteRepository = new ConviteRepository();
                _conviteRepository.AtualizarSituacaoLista(emailConvidado);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /*public void AtualizarConviteParaAceito(Guid codConvite, string nomeCadastrado, string emailCadastrado)
        {
            ConviteRepository repository = new ConviteRepository();

            Convite convite = repository.GetByKey(codConvite);

            convite.Status = 1;

            convite.EmailCadastrado = emailCadastrado;

            convite.NomeCadastrado = nomeCadastrado;

            repository.SaveOrUpdate(convite);

        }*/

        public Convite GetConvitePeloCodigo(Guid codConvite)
        {
            Convite convite = new ConviteRepository().GetByKey(codConvite);

            return convite;

        }

        public void AtualizarInformacoesConvidado(Convite convite, string nomeCadastrado, string emailCadastrado)
        {
            convite.NomeCadastrado = nomeCadastrado;
            convite.EmailCadastrado = emailCadastrado;
            new ConviteRepository().SaveOrUpdate(convite);
        }

        public void AtualizarConviteParaAceito(Convite convite)
        {
            convite.Status = CON_STATUS_ACEITO;
            new ConviteRepository().SaveOrUpdate(convite);
        }

        internal IList GetResumoConvites(Participante participante)
        {
            throw new NotImplementedException();
        }
    }
}