using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Web.Security;
using Bolao10.Persistence.Repository;
using Bolao10.Model.Entities;

namespace Bolao10.Web.Hubs
{
    public class BolaoHub : Hub
    {

        private class User
        {
            public string Codigo { get; set; }

            public string Connection { get; set; }

            //public bool Desconectou { get; set; }

            public User()
            {
            }

            public User(string codigo, string connection)
            {
                Codigo = codigo;
                Connection = connection;
                //Desconectou = false;
            }
        }

        static List<User> _users = new List<User>();

        public override System.Threading.Tasks.Task OnReconnected()
        {
            return base.OnReconnected();
        }

        private string GetCodigoParticipante()
        {
            var codigoCookie = Context.RequestCookies["codigo1"];

            if (codigoCookie != null) { return codigoCookie.Value; }

            return "";
        }
        public override System.Threading.Tasks.Task OnConnected()
        {
            
            string codigoParticipante = GetCodigoParticipante();
            
            string connection = Context.ConnectionId;

            User user = new User(codigoParticipante, connection);

            lock (_users)
            {
                _users.Add(user);

            }

            ShowUsersOnLine();

            return ( base.OnConnected() );
        }
        
        public override System.Threading.Tasks.Task OnDisconnected()
        {

            string connection = Context.ConnectionId;

            lock(_users)
            {
                if (_users.Count > 0)
                {
                    var firstUser = (from u in _users where u.Connection == connection select u)
                            .FirstOrDefault();

                    //firstUser.Desconectou = true;

                    _users.Remove(firstUser);

                }
            }

            ShowUsersOnLine();

            return base.OnDisconnected();
        }
        
        public void ShowUsersOnLine()
        {
            try
            {

                var counter = (from u in _users group u by u.Codigo into g select g).Count();

                var users = (from u in _users group u by new { u.Codigo } into g 
                             select new User { Codigo = g.Key.Codigo });

                Clients.All.showUsersOnLine(counter, users);
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        public void EnviarComentarioParaServidor(int? comentarioPaiId, string texto)
        {

            string participanteCode = GetCodigoParticipante(); 
            var participanteRepository = new ParticipanteRepository();
            var participante = participanteRepository.FindByCodigo(participanteCode);
            var comentarioRepository = new ComentarioRepository();
            Comentario novoComentario = comentarioRepository.AdicionarComentario(comentarioPaiId, texto, participante, comentarioRepository);

            Clients.All.adicionarComentario(comentarioPaiId, novoComentario.Id, texto, new { Id = participante.Id, Usuario = new { Nome = participante.Usuario.Nome } });

            IList<Comentario> comentarios = (IList<Comentario>)this.Context.Request.GetHttpContext().Cache["Comentarios"];

            this.Context.Request.GetHttpContext().Cache.Remove("Comentarios");

        }

        public void EnviarCurtirParaServidor(int comentarioId)
        {
            string codigoParticipante = GetCodigoParticipante();

            var comentarioRepository = new ComentarioRepository();

            comentarioRepository.Curtir(comentarioId, codigoParticipante, (participante) =>
            {
                this.Clients.All.atualizarCurtidas(comentarioId, new { Id = participante.Id, 
                        Nome = participante.Usuario.Nome });
            });

            this.Context.Request.GetHttpContext().Cache.Remove("Comentarios");

        }

        public void EnviarDescurtirParaServidor(int messageId)
        {
            string codigoParticipante = GetCodigoParticipante();

            var messageRepository = new ComentarioRepository();
            messageRepository.Descurtir(messageId, codigoParticipante, (participante) =>
            {
                Clients.All.atualizarDescurtidas(messageId, new { Id = participante.Id, 
                            Nome = participante.Usuario.Nome });
            });

            this.Context.Request.GetHttpContext().Cache.Remove("Comentarios");

        }


    }
}