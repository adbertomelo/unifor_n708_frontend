
namespace Bolao10.Services
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Net;
    using System.IO;
    using System.Text;
    using Bolao10.Model.Entities;
    using Bolao10.Persistence.Repository;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NHibernate.Transaction;
    using System.Xml;

    public class PaymentService
    {
        const int STATUS_CADASTRADO = 0;
        const int STATUS_LIBERADO = 1;
        const int STATUS_BLOQUEADO = 2;

        const int SITUACAO_PGTO_NAO_INICIADO = 0;
        const int SITUACAO_PGTO_INICIADO = 1;
        const int SITUACAO_PGTO_APROVADO = 2;
        const int SITUACAO_PGTO_CANCELADO = 3;


        private log4net.ILog _logger;
        private PedidoRepository _pedidoRepository;

        public PaymentService()
        {
            _pedidoRepository = new PedidoRepository();

        }

        private void atualizaSituacaoParticipante(string id, string pedidoid, 
                                                  string formapgto, string datecreated, string dateapproved, string status)
        {
            int statusParticipante = 0;
            int situacaoPagamento = 0;

            switch(status)
            {
                case "approved":
                    statusParticipante = STATUS_LIBERADO;
                    situacaoPagamento = SITUACAO_PGTO_APROVADO;
                    break;
                case "pending":
                case "in_process":                
                    statusParticipante = STATUS_LIBERADO;
                    situacaoPagamento = SITUACAO_PGTO_INICIADO;
                    break;
                case "rejected":
                case "refunded":
                case "cancelled":
                case "in_mediation":
                    statusParticipante = STATUS_BLOQUEADO;
                    situacaoPagamento = SITUACAO_PGTO_CANCELADO;
                    break;
            }

            int pedidoId = int.Parse(pedidoid);
            Pedido pedido = _pedidoRepository.GetById(pedidoId);
            pedido.Status = status;
            pedido.Transacao = id;
            pedido.FormaPgto = formapgto;
            if (!String.IsNullOrEmpty(datecreated))
            {
                pedido.DataTransacao = DateTime.Parse(datecreated);
            }
            if (!String.IsNullOrEmpty(dateapproved))
            {
                pedido.DataPagamento = DateTime.Parse(dateapproved);
            }
            pedido.Participante.Status = statusParticipante;
            pedido.Participante.SituacaoPagamento = situacaoPagamento;
            _pedidoRepository.Update(pedido);

        }

        
        public void VerifyPagSeguro2(HttpContextBase httpcontext)
        {

            _logger = log4net.LogManager.GetLogger("LogInFile");

            try
            {

                string notificationCode = "";

                foreach (var key in httpcontext.Request.Form.AllKeys)
                {
                    if (!String.IsNullOrEmpty(key))
                    {
                        if (key.Equals("notificationCode"))
                        {
                            notificationCode = httpcontext.Request.Form[key].ToString();
                        }
                    }
                }

                string email = HttpUtility.UrlEncode("adbertomelo@yahoo.com.br");
                string token = "77A2F529028440B4987E6B0AE9AEA36C";
                //string tokenSandbox = "02A8B5DEBDF04A87A02552C013A54E1C";
                //string Pagina = String.Format("https://ws.sandbox.pagseguro.uol.com.br/v3/transactions/notifications/{0}?email={1}&token={2}", notificationCode, email, tokenSandbox);
                
                string Pagina = String.Format("https://ws.pagseguro.uol.com.br/v3/transactions/notifications/{0}?email={1}&token={2}", notificationCode, email, token);

                System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(Pagina);

                req.Method = "GET";
                
                req.ContentType = "application/xml";

                System.IO.StreamReader stIn = new System.IO.StreamReader(req.GetResponse().GetResponseStream(), System.Text.Encoding.GetEncoding("ISO-8859-1"));
                
                string result = stIn.ReadToEnd();
                
                stIn.Close();

                XmlDocument doc = new XmlDocument();
        
                doc.LoadXml(result);

                string transacao, referencia, datatransacao, tipopagamento, statustransacao;

                transacao = doc.SelectSingleNode("transaction/code").InnerText;
                referencia = doc.SelectSingleNode("transaction/reference").InnerText;
                datatransacao = doc.SelectSingleNode("transaction/date").InnerText;
                tipopagamento = doc.SelectSingleNode("transaction/paymentMethod/type").InnerText;
                statustransacao = doc.SelectSingleNode("transaction/status").InnerText;

                _logger.Info(String.Format("PagSeguro: Post da transação {0} recebido", transacao));

                atualizaSituacaoParticipantePagSeguro2(transacao, referencia, tipopagamento, datatransacao, null, statustransacao);


            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("Erro desconhecido no recebimento do retorno do PagSeguro. Transacao = {0}.", httpcontext.Request.Params["TransacaoID"]), ex);
            }


        }
        public void VerifyPagSeguro(HttpContextBase httpcontext)
        {

            _logger = log4net.LogManager.GetLogger("LogInFile");

            try
            {

                string Dados = "";

                foreach (var key in httpcontext.Request.Form.AllKeys)
                {
                    if (!String.IsNullOrEmpty(key))
                    {
                        String value = httpcontext.Request.Form[key].ToString();
                        value = HttpUtility.UrlEncode(value, Encoding.GetEncoding("ISO-8859-1"));
                        Dados += String.Format("{0}={1}&", key, value);
                    }
                }

                string Token = "77A2F529028440B4987E6B0AE9AEA36C";

                string Pagina = "https://pagseguro.uol.com.br/pagseguro-ws/checkout/NPI.jhtml";

                Dados += "Comando=validar&Token=" + Token;

                System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(Pagina);

                req.Method = "POST";
                req.ContentLength = Dados.Length;
                req.ContentType = "application/x-www-form-urlencoded";


                System.IO.StreamWriter stOut = new System.IO.StreamWriter(req.GetRequestStream(), System.Text.Encoding.GetEncoding("ISO-8859-1"));
                stOut.Write(Dados);
                stOut.Close();

                System.IO.StreamReader stIn = new System.IO.StreamReader(req.GetResponse().GetResponseStream(), System.Text.Encoding.GetEncoding("ISO-8859-1"));
                string Result = stIn.ReadToEnd();
                stIn.Close();

                string transacao, referencia, datatransacao, tipopagamento, statustransacao;

                transacao = httpcontext.Request.Params["TransacaoID"];
                referencia = httpcontext.Request.Params["Referencia"];
                datatransacao = httpcontext.Request.Params["DataTransacao"];
                tipopagamento = httpcontext.Request.Params["TipoPagamento"];
                statustransacao = httpcontext.Request.Params["StatusTransacao"];

                _logger.Info(String.Format("PagSeguro: Post da transação {0} recebido", transacao));

                if (Result == "VERIFICADO")
                {
 
                    atualizaSituacaoParticipantePagSeguro(transacao, referencia, tipopagamento, datatransacao, null, statustransacao);

                }
                else if (Result == "FALSO")
                {
                    _logger.Error(String.Format("Retorno do PagSeguro não foi validado. Transacao = {0}.", transacao));
                }
                else
                {
                    _logger.Error(String.Format("Erro desconhecido no recebimento do retorno do PagSeguro. Transacao = {0}.", transacao));
                }

            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("Erro desconhecido no recebimento do retorno do PagSeguro. Transacao = {0}.", httpcontext.Request.Params["TransacaoID"]), ex);
            }


        }

        private void atualizaSituacaoParticipantePagSeguro2(string id, string pedidoid, string formapgto, string datecreated, string dateapproved, string status)
        {
            int statusParticipante = 0;
            int situacaoPagamento = 0;
            string statusDsc = "";

            switch (status)
            {
                case "3": //Paga ~"Aprovado":
                case "4": //Disponível ~"Completo":
                    statusParticipante = STATUS_LIBERADO;
                    situacaoPagamento = SITUACAO_PGTO_APROVADO;
                    statusDsc = "Aprovado";
                    break;
                case "1": //"Aguardando Pagto":
                case "2": //"Em Análise":
                    statusParticipante = STATUS_LIBERADO;
                    situacaoPagamento = SITUACAO_PGTO_INICIADO;
                    statusDsc = "Aguardando pgto.";
                    break;
                case "7": //cancelado
                case "5": //em disputa
                case "6": //devolvida
                case "8": //debitada
                case "9": //retenção temporária
                    statusParticipante = STATUS_BLOQUEADO;
                    situacaoPagamento = SITUACAO_PGTO_CANCELADO;
                    statusDsc = "Cancelado";
                    break;
            }

            int pedidoId = int.Parse(pedidoid);
            Pedido pedido = _pedidoRepository.GetById(pedidoId);
            pedido.Status = statusDsc;
            pedido.Transacao = id;
            pedido.FormaPgto = formapgto;
            pedido.DataTransacao = DateTime.Now;
            pedido.Participante.Status = statusParticipante;
            pedido.Participante.SituacaoPagamento = situacaoPagamento;
            _pedidoRepository.Update(pedido);

        }
        private void atualizaSituacaoParticipantePagSeguro(string id, string pedidoid, string formapgto, string datecreated, string dateapproved, string status)
        {
            int statusParticipante = 0;
            int situacaoPagamento = 0;

            switch (status)
            {
                case "Aprovado":
                case "Completo":
                    statusParticipante = STATUS_LIBERADO;
                    situacaoPagamento = SITUACAO_PGTO_APROVADO;
                    break;
                case "Aguardando Pagto":
                case "Em Análise":
                    statusParticipante = STATUS_LIBERADO;
                    situacaoPagamento = SITUACAO_PGTO_INICIADO;
                    break;
                case "Cancelado":
                    statusParticipante = STATUS_BLOQUEADO;
                    situacaoPagamento = SITUACAO_PGTO_CANCELADO;
                    break;
            }

            int pedidoId = int.Parse(pedidoid);
            Pedido pedido = _pedidoRepository.GetById(pedidoId);
            pedido.Status = status;
            pedido.Transacao = id;
            pedido.FormaPgto = formapgto;
            pedido.DataTransacao = DateTime.Now;
            pedido.Participante.Status = statusParticipante;
            pedido.Participante.SituacaoPagamento = situacaoPagamento;
            _pedidoRepository.Update(pedido);

        }


    }

    
}
