using System;
using System.Web;
using System.Web.Security;
using Bolao10.Exceptions;
using Bolao10.Model.Entities;
using Bolao10.Persistence.Repository;

namespace Bolao10.Services
{
    public class AuthenticationService
    {

        private EncryptionService _encryptionService;
        private UsuarioRepository _usuarioRepository;
        private BolaoRepository _bolaoRepository;
        private ParticipanteRepository _participanteRepository;

        public AuthenticationService(){
        
            _encryptionService = new EncryptionService();
            _usuarioRepository = new UsuarioRepository();
            _bolaoRepository = new BolaoRepository();
            _participanteRepository = new ParticipanteRepository();

        }

        public void SignOut(HttpContextBase httpcontext)
        {

            //FormsAuthentication.SignOut();

            httpcontext.Session.Abandon();

        }

        public void Authenticate(Participante participante)
        {



            //HttpContext.Current.Request.Cookies.Remove(FormsAuthentication.FormsCookieName);

            //FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, participante.Codigo, DateTime.Now, 
            //                            DateTime.Now.AddMinutes(360), false, "userData");

            //string encTicket = FormsAuthentication.Encrypt(ticket);

            //HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);

            //HttpContext.Current.Response.Cookies.Add(cookie);

        }

        public string GetTicket()
        {

            string ticket = HttpContext.Current.Request.Cookies.Get(FormsAuthentication.FormsCookieName).Value;

            return ticket;
        }



         
    }
}