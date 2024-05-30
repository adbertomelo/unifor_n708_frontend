using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using Bolao10.Persistence.Repository;
using Bolao10.Model.Entities;

namespace Bolao10.Services
{
    public class NotificacaoService
    {
        
        public NotificacaoService()
        {

        }

        private void CriarNotificacao(Participante participante, string mensagem)
        {

            Notificacao notificacao = new Notificacao()
            {
                Participante = participante,
                Mensagem = mensagem,
                TimeStamp  = DateTime.Now,
                Status = 0
            };

            new NotificacaoRepository().SaveOrUpdate(notificacao);
            
        }

        public void NotificarConviteAceito(Convite convite)
        {
            string mensagem = String.Format("O convite enviado para {0} foi aceito.", convite.NomeConvidado);

            CriarNotificacao(convite.Participante, mensagem);
        }

        public void NotificarConviteConvidado(Convite convite)
        {
            string email = convite.Participante.Usuario.Email;
            
            Convite conviteConvidado = new ConviteRepository().FindByEmailConvidado(email);

            if (conviteConvidado != null)
            {
                string mensagem = String.Format("O seu convidado {0} enviou um convite para {1}.", convite.Participante.Usuario.Nome, convite.NomeConvidado);
                CriarNotificacao(conviteConvidado.Participante, mensagem);
            }
        }

        public void MarcarComoLidas(Participante participante)
        {
            NotificacaoRepository repo = new NotificacaoRepository();

            foreach(Notificacao n in repo.FindNaoLidas(participante))
            {
                n.Status = 1;
                repo.Update(n);
            }
        }

        public IList<Notificacao> TodasNotificacoes(Participante participante)
        {
            NotificacaoRepository repo = new NotificacaoRepository();
            return repo.FindByParticipante(participante);
        }

        public IList<Notificacao> TodasNotificacoesNaoLidas(Participante participante)
        {
            NotificacaoRepository repo = new NotificacaoRepository();
            return repo.FindNaoLidas(participante);
        }

    }
}