using Bolao10.Exceptions;
using Bolao10.Model.Entities;
using Bolao10.Persistence.Repository;
using Bolao10.ViewModels;
using System;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web.Configuration;
using Bolao10.Web.HttpModels;
using System.Net.Http.Headers;
using SendGrid;

namespace Bolao10.Services
{
    public class AccountService
    {
        CidadeRepository _cidadeRepository;
        ParticipanteRepository _participanteRepository;
        UsuarioRepository _usuarioRepository;
        EncryptionService _encryptionService;
        AcessoReposiroty _acessoRepository;
        ApiService _apiService;
        

        public AccountService()
        {
            
            _encryptionService = new EncryptionService();

            _cidadeRepository = new CidadeRepository();
            _participanteRepository = new ParticipanteRepository();
            _usuarioRepository = new UsuarioRepository();
            _acessoRepository = new AcessoReposiroty();
            _apiService = new ApiService();
            

        }

        public bool LoginIsValid(string userName, string password)
        {


           _usuarioRepository = new UsuarioRepository();

            Usuario usuario = _usuarioRepository.FindByEmail(userName);

            if (usuario == null) throw new LoginException(String.Format("Email {0} não encontrado.", userName));

            if (password != _encryptionService.Decrypt(usuario.Senha)) throw new LoginException("Login inválido!");

            return (true);

        }

        public UsuarioResponse Login(string userName, string password)
        {
            string url = $"{_apiService.GetUrlBase()}/Authentication/Entrar";

            using (var client = new HttpClient())
            {

                try
                {

                    /*https://www.stevejgordon.co.uk/sending-and-receiving-json-using-httpclient-with-system-net-http-json*/
                    var res = client.PostAsync(url,
                      new StringContent(JsonConvert.SerializeObject(new { Login = userName, Senha = password }),
                        Encoding.UTF8, "application/json")
                    );

                    if (res.Result.EnsureSuccessStatusCode().StatusCode == HttpStatusCode.OK)
                    {
                        
                        return res.Result.Content.ReadFromJsonAsync<UsuarioResponse>().Result;
                        
                        /*https://kevsoft.net/2021/12/19/traversing-json-with-jsondocument.html*/
                        //var token = json.RootElement.GetProperty("token").GetString();

                    }                        
                    else
                        throw new Exception("Login inválido!");
                }
                catch
                {
                    return null;
                }
            }

        }

        public Participante Create(ConvidadoViewModel p)
        {

            ConviteService conviteService = new ConviteService();

            Convite convite = conviteService.GetConvitePeloCodigo(p.Convite);

            conviteService.ValidarConvite(convite);

            UsuarioService usuarioService = new UsuarioService();

            usuarioService.ValidarSenha(p.Senha, p.ConfirmaSenha);

            Usuario usuario = usuarioService.NovoUsuario(p.Nome, p.Email, p.Senha, p.Cidade);

            Bolao bolao = convite.Participante.Bolao;

            ParticipanteService participanteService = new ParticipanteService();

            Participante participante = participanteService.CriarParticipante(bolao, usuario);

            return(participante);

        }


        public void BloquearAcesso(int participanteId)
        {
            Participante participante = _participanteRepository.GetById(participanteId);
            participante.Status = 2;
            _participanteRepository.SaveOrUpdate(participante);

        }
        
        //public Participante GetParticipante(HttpContextBase httpContext)
        //{

        //    try
        //    {
        //        string participanteCod = httpContext.User.Identity.Name;

        //        Participante participante = _participanteService.GetByCodigo(participanteCod);

        //        return (participante);

        //    }catch
        //    {
        //        return (null);
        //    }

        //}

        public void AtualizaParticipante(Participante participante)
        {
            participante.DataHoraUltimoAcesso = participante.DataHoraAcesso;

            participante.DataHoraAcesso = DateTime.Now;

            participante.NumAcessos = participante.NumAcessos + 1;

            _participanteRepository.Update(participante);

        }

        public void CriarAcesso(Participante participante)
        {
            string ticket = HttpContext.Current.Request.Cookies.Get(FormsAuthentication.FormsCookieName).Value;

            Acesso acesso = new Acesso(participante, ticket);

            _acessoRepository.Save(acesso);

            string acessoId = acesso.Id.ToString();

            SetAcessoId(acessoId);

        }

        private void SetAcessoId(string value)
        {
            HttpCookie httpcookie = new HttpCookie("UserInfo");

            string encryValue = _encryptionService.Encrypt(value);

            httpcookie.Values["key1"] = encryValue;

            httpcookie.Expires = DateTime.Now.AddMinutes(360);

            HttpContext.Current.Response.Cookies.Add(httpcookie);
        }

        public int? GetAcessoId()
        {
            int acessoId = 0;

            HttpCookie httpcookie = System.Web.HttpContext.Current.Request.Cookies["UserInfo"];

            if (httpcookie != null)
            {
                EncryptionService encry = new EncryptionService();

                string cookieValue = httpcookie["key1"];

                string decryCookieValue = encry.Decrypt(cookieValue);

                acessoId = System.Convert.ToInt32(decryCookieValue);

            }
            else
            {
                throw new Exception("Seu acesso não foi encontrado. Saia do sistema e entre novamente.");
            }

            return (acessoId);

        }

        public void AlterarSenha(int usuarioId, string senha, string confirmenovasenha)
        {
            UsuarioService usuarioService = new UsuarioService();
            usuarioService.ValidarSenha(senha, confirmenovasenha);
            EncryptionService encryService = new EncryptionService();
            string senhaEncr = encryService.Encrypt(senha);
            UsuarioRepository usuRepo = new UsuarioRepository();
            Usuario usu = usuRepo.FindById(usuarioId);
            usu.Senha = senhaEncr;
            usuRepo.Update(usu);
        }

        public void Atualizar(string codigo, string nome, int cidadeId)
        {
            Participante participante = _participanteRepository.FindByCodigo(codigo);

            UsuarioService usuarioService = new UsuarioService();

            usuarioService.ValidarNomeUsuario(nome);

            participante.Usuario.Nome = nome;
            
            Cidade cidade = _cidadeRepository.GetById(cidadeId);
            
            participante.Usuario.Cidade = cidade;
            
            _participanteRepository.Update(participante);
        }

        public void SalvarToken(string token)
        {
            HttpContext.Current.Session.Add("token1", token);
        }

        public string GetToken()
        {

            var token = HttpContext.Current.Session["token1"];

            return token == null ? "" : token.ToString();
            
            
        }

        internal void SalvarParticipante(Participante participante)
        {
            HttpCookie cookie = new HttpCookie("codigo1", participante.Codigo);
            HttpContext.Current.Response.Cookies.Add(cookie);
            //HttpContext.Current.Session.Add("codigo", participante.Codigo);
        }
    }

    public static class Auth
    {
        public static string Codigo { get; set; }
        public static string Token { get; set; }
    }
}