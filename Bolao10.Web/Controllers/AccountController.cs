using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bolao10.Model.Entities;
using Bolao10.Persistence.Repository;
using System.ComponentModel.DataAnnotations;
using Bolao10.Services;
using Bolao10.ViewModels;
using System.Net.Mail;
using Bolao10.Exceptions;
using System.Net.PeerToPeer;
using Bolao10.Web.HttpModels;
using Bolao10.Web.Filters;

namespace Bolao10.Controllers
{
    
    public class AccountController : Controller
    {

        readonly AuthenticationService _authenticationService;
        readonly AccountService _accountService;
        readonly ApostaService _apostaService;
        readonly ParametroRepository _parametroRepository;
        readonly UsuarioService _usuarioService;
        readonly BolaoService _bolaoService;
        readonly ParticipanteService _participanteService;

        ConviteRepository _conviteRepository;
        readonly PaisRepository _paisRepository;
        readonly CidadeRepository _cidadeRepository;
        readonly EstadoRepository _estadoRepository;
        readonly ParticipanteRepository _participanteRepository;
        readonly UsuarioRepository _usuarioRepository;
        readonly BolaoRepository _bolaoRepository;


        public AccountController() 
        {
            _authenticationService = new AuthenticationService();
            _apostaService = new ApostaService();
            _accountService = new AccountService();
            _usuarioService = new UsuarioService();
            _bolaoService = new BolaoService();
            _participanteService = new ParticipanteService();

            _parametroRepository = new ParametroRepository();
            _paisRepository = new PaisRepository();
            _cidadeRepository = new CidadeRepository(); 
            _estadoRepository = new EstadoRepository();
            _participanteRepository = new ParticipanteRepository();
            _usuarioRepository = new UsuarioRepository();
            _bolaoRepository = new BolaoRepository();

    
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult UsuarioMenu()
        {
            string participanteCod = _participanteService.GetCodigo();

            Participante participante = _participanteService.GetByCodigo(participanteCod);

            return PartialView(participante);
        }


        //tirar isso daqui
        //atentar para que ao colocar num controller com filter
        //vai dar erro se houve um redirecionamento de página
        //o ideal é que houvesse um "skip filter"
        [ChildActionOnly]
        public ActionResult NotificacoesMenu()
        {
            IList<Notificacao> notificacoes = new List<Notificacao>();

            try
            {
                string participanteCod = HttpContext.User.Identity.Name;

                Participante participante = _participanteService.GetByCodigo(participanteCod);

                NotificacaoService notServ = new NotificacaoService();

                notificacoes = notServ.TodasNotificacoesNaoLidas(participante);

            }
            catch { }

            ViewBag.Notificacoes =  notificacoes;

            ViewBag.TotalNotificacoes = notificacoes.Count();

            return PartialView();

        }

        
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            LoginViewModel model = new LoginViewModel();
            return( View(model) );
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {

                try
                {

                    UsuarioResponse usuarioResponse = _accountService.Login(model.UserName, model.Password);
                    
                    if (usuarioResponse != null)
                    {

                        _accountService.SalvarToken(usuarioResponse.Token);

                        Bolao bolao = _bolaoService.FindAtivo();

                        Usuario usuario = _usuarioService.FindByEmail(usuarioResponse.Email);

                        Participante participante = _participanteService.FindByUsuarioAndBolao(usuario, bolao);

                        if (_bolaoService.BolaoIniciado(bolao))
                        {
                            if (participante.Status != 1 || participante.SituacaoPagamento != 2)
                                throw new Exception("Período de entrada no bolão encerrado.");
                        }

                        _accountService.SalvarParticipante(participante);

                        //_authenticationService.Authenticate(participante);

                        _participanteService.AtualizarInformacoesAcesso(participante);

                    }
                    else
                    {
                        TempData["Erro"] = String.Format("   {0} não encontrado.", model.UserName);
                    }

                    return (RedirectToAction("Index", "Home"));

                }
                catch (Exception ex)
                {
                    TempData["Erro"] = ex.Message;
                }

            }

            return (View(model));



        }

        [Authorize]
        public ActionResult LogOut()
        {
            
            _authenticationService.SignOut(HttpContext);

            return(RedirectToAction("Login"));

        }

        public void EsquecimentoDeSenha()
        {
        }

        [TokenFilter]
        [HttpGet]
        public ActionResult Edit()
        {

            string participanteCod = _participanteService.GetCodigo();

            Participante participante = _participanteService.GetByCodigo(participanteCod);

            ViewBag.Codigo = participante.Codigo;
            ViewBag.Nome = participante.Usuario.Nome;
            IList<Pais> paises = _paisRepository.GetAll();
            ViewBag.Paises = new SelectList(_paisRepository.GetAll(), "Id", "Nome", participante.Usuario.Cidade.Estado.Pais.Id);
            ViewBag.Estados = new SelectList(_estadoRepository.FindAllByPais(paises[0]), "Id", "Nome", participante.Usuario.Cidade.Estado.Id);
            ViewBag.Cidades = new SelectList(_cidadeRepository.FindAllByEstado(participante.Usuario.Cidade.Estado), "Id", "Nome", participante.Usuario.Cidade.Id);

            return (View());
        }

        [TokenFilter]
        [HttpPost]
        public ActionResult Edit(ParticipanteEditViewModel p)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    _accountService.Atualizar(p.Codigo, p.Nome, p.Cidade);

                    TempData["Mensagem"] = "Usuário alterado com sucesso!";

                    ViewBag.Codigo = p.Codigo;
                    
                    ViewBag.Nome = p.Nome;

                    PreencherBagsComLocais(p.Cidade);

                    return (View());

                }
                catch (Exception ex)
                {
                    ViewBag.Codigo = p.Codigo;
                    
                    ViewBag.Nome = p.Nome;

                    PreencherBagsComLocais(p.Cidade);

                    TempData["Erro"] = ex.Message;
                    
                    return (View(p));
                }

            }

            return (View());
        }

        private IList<Time> GetTimes()
        {
            TimeRepository timeRepository = new TimeRepository();
            IList<Time> times = timeRepository.GetAll().OrderBy(x => x.Nome).ToList();
            return (times);

        }

        private void PreencherBagsComLocais(int cidadeid)
        {
            Cidade cidade = _cidadeRepository.GetById(cidadeid);

            IList<Pais> paises = _paisRepository.GetAll();
            ViewBag.Paises = new SelectList(_paisRepository.GetAll(), "Id", "Nome", cidade.Estado.Pais.Id);            
            ViewBag.Estados = new SelectList(_estadoRepository.FindAllByPais(paises[0]), "Id", "Nome", cidade.Estado.Id);
            ViewBag.Cidades = new SelectList(_cidadeRepository.FindAllByEstado(cidade.Estado), "Id", "Nome", cidade.Id);

        }

        [HttpGet]
        public ActionResult New(Guid key)
        {

            try
            {

                ConviteService conviteService = new ConviteService();

                Convite convite = conviteService.GetConvitePeloCodigo(key);

                conviteService.ValidarConvite(convite);

                ViewBag.ConviteId = key;
                ViewBag.Email = convite.EmailConvidado;
                ViewBag.Nome = convite.NomeConvidado;
                IList<Pais> paises = _paisRepository.GetAll();
                ViewBag.Paises = new SelectList(_paisRepository.GetAll(), "Id", "Nome");
                ViewBag.Estados = new SelectList(_estadoRepository.FindAllByPais(paises[0]), "Id", "Nome");
                ViewBag.Cidades = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.Times = new SelectList(GetTimes(), "Id", "Nome");


            }
            catch(Exception ex)
            {
                TempData["Erro"] = ex.Message;
                return (View("_EmBranco"));
            }

            return (View());
        }

        public ActionResult Remove(Guid key)
        {

            _conviteRepository = new ConviteRepository();

            Convite convite = _conviteRepository.GetByKey(key);

            if (convite.Status == 0)
            {
                convite.Status = 2;
                _conviteRepository.Update(convite);
                TempData["Mensagem"] = "Seu convite foi cancelado com sucesso!";
            }
            else
            {
                TempData["Mensagem"] = "Não é possível cancelar este convite.";
            }

            return (View("../Shared/_EmBranco"));
        }

        private void AtualizarInformacoesParticipante(Participante participante, Guid codConvite, int idTimeCampeao)
        {
            log4net.ILog logger;
            
            logger = log4net.LogManager.GetLogger("LogInFile");

            try
            {
                ParticipanteService participanteService = new ParticipanteService();

                participanteService.AtualizarHistorico(participante);

                participanteService.AtualizarOutrasInfo(participante, codConvite, idTimeCampeao);

            }
            catch(Exception ex)
            {
                logger.Error("Erro ao atualizar informações do participante '" + participante.Usuario.Nome + "'. Erro:" + ex.Message);
            }

        }

        private void AtualizarInformacoesConvite(Guid codConvite, string nomeCadastrado, string emailCadastrado)
        {
            log4net.ILog logger;

            logger = log4net.LogManager.GetLogger("LogInFile");

            try
            {
                ConviteService conviteService = new ConviteService();

                Convite convite = conviteService.GetConvitePeloCodigo(codConvite);

                conviteService.AtualizarConviteParaAceito(convite);

                NotificacaoService notificacaService = new NotificacaoService();

                notificacaService.NotificarConviteAceito(convite);

                conviteService.AtualizarInformacoesConvidado(convite, nomeCadastrado, emailCadastrado);

            }
            catch(Exception ex)
            {
                logger.Error("Erro ao atualizar informações do convite'" + codConvite.ToString() + "'. Erro:" + ex.Message);
            }

        }

        private void GerarApostas(Participante participante)
        {
            log4net.ILog logger;

            logger = log4net.LogManager.GetLogger("LogInFile");

            try
            {
                ApostaService apostaService = new ApostaService();

                apostaService.CriarApostas(participante);

            }
            catch(Exception ex)
            {
                logger.Error("Erro ao criar apostas para o participante '" + participante.Usuario.Nome + "'. Erro:" + ex.Message);
            }

        }


        [HttpPost]
        public ActionResult Create(ConvidadoViewModel convidado)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    Participante participante = _accountService.Create(convidado);

                    AtualizarInformacoesParticipante(participante, convidado.Convite, convidado.TimeCampeao);

                    AtualizarInformacoesConvite(convidado.Convite, convidado.Nome, convidado.Email);

                    GerarApostas(participante);

                    TempData["Mensagem"] = String.Format("Usuário criado com sucesso! Entre com o e-mail e senha que você criou e faça o pagamento para ter acesso ao {0}", participante.Bolao.Descricao);

                    return(RedirectToAction("Login"));

                }
                catch (Exception ex)
                {
                    
                    ViewBag.ConviteId = convidado.Convite;
                    ViewBag.Nome = convidado.Nome;
                    ViewBag.Email = convidado.Email;
                    ViewBag.Times = new SelectList(GetTimes(), "Id", "Nome", convidado.TimeCampeao);
                    PreencherBagsComLocais(convidado.Cidade);

                    TempData["Erro"] = ex.Message;

                    return (View("New", convidado));

                }

            }

            return (View("New"));
        }

        public void GetEstados(int paisId)
        {
            if (paisId == 0)
                return;

            Pais pais = _paisRepository.GetById(paisId);
            
            IList<Estado> estados = _estadoRepository.FindAllByPais(pais);
            
            ViewBag["estados"] = estados;
            
        }

        public ActionResult GetCidades(int estadoId)
        {
            if (estadoId == 0)
                return(View());

            Estado estado = _estadoRepository.GetById(estadoId);
            
            IList<Cidade> cidades = _cidadeRepository.FindAllByEstado(estado);
            
            return(Json(cidades, JsonRequestBehavior.AllowGet));
            
        }

        public void AlterarSenha()
        {
        }
        
        [HttpPost]
        public ViewResult ChangePassword(string password, string confirmpassword)
        {
            try
            {
                string codParticipante = _participanteService.GetCodigo();
                ParticipanteService participanteService = new ParticipanteService();
                Participante participante = participanteService.GetByCodigo(codParticipante);

                Usuario usuario = participante.Usuario;
                _accountService.AlterarSenha(usuario.Id, password, confirmpassword);
                TempData["Mensagem"] = "Senha alterada com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return (View());
            

        }

        [AllowAnonymous]
        [HttpGet]
        public ViewResult RecuperarSenha()
        {
            return (View());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RecuperarSenha(RecuperarSenhaViewModel model)
        {
            if (ModelState.IsValid)
            {

                try
                {

                    Usuario usuario = _usuarioRepository.FindByEmail(model.UserName);

                    EncryptionService _encrService = new EncryptionService();

                    string senha = _encrService.Decrypt(usuario.Senha);

                    Parametros param = _parametroRepository.FindOne();

                    MailMessage mailMessage = new MailMessage();

                    mailMessage.From = new MailAddress(param.EmailAdmin);

                    mailMessage.To.Add(new MailAddress(usuario.Email));

                    mailMessage.Subject = "Recuperação de Senha";
                    
                    mailMessage.Body = String.Format("Olá, {0}. Você solicitou a sua senha. A sua senha é: {1}.", usuario.Nome, senha);

                    mailMessage.IsBodyHtml = true;

                    MailService mailService = new MailService();

                    mailService.Send(mailMessage);

                    TempData["Mensagem"] = "Email enviado com sucesso!";

                    return (View(model));

                }
                catch
                {
                    ModelState.AddModelError("", "Email não encontrado.");
                }

            }

            return (View(model));



        }

        public ViewResult OpcoesConta()
        {
            return (View());
        }

    }
}

