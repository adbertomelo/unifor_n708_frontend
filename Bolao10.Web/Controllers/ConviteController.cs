using Bolao10.Model.Entities;
using Bolao10.Model.Entities.Views;
using Bolao10.Persistence.Repository;
using Bolao10.Services;
using Bolao10.ViewModels;
using Bolao10.Web.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;


namespace Bolao10.Web.Controllers
{
    [TokenFilter]
    public class ConviteController : Controller
    {

        ConviteRepository _conviteRepository;
        ParticipanteRepository _participanteRepository;
        ConviteService _conviteService;

        public ConviteController()
        {
            //_usuarioRepository = new UsuarioRepository();
            //_bolaoRepository = new BolaoRepository();
            //_parametroRepository = new ParametroRepository();
        }

        public ActionResult Index()
        {

            _participanteRepository = new ParticipanteRepository();

            string participanteCod = HttpContext.User.Identity.Name;

            Participante participante = _participanteRepository.FindByCodigo(participanteCod);

            _conviteRepository = new ConviteRepository();

            IList<ViewConvite> convites = _conviteRepository.GetConvitesPorParticipante(participante);

            ViewBag.Convites = convites;

            ViewBag.PrazoEncerrado = PrazoEncerrado();

            return View();
        }

        public ActionResult All()
        {

            ParticipanteService participanteService = new ParticipanteService();
            string codParticipante = participanteService.GetCodigo();            
            Participante participante = participanteService.GetByCodigo(codParticipante); 

            if (participante.Tipo == "M")
            {
                _conviteRepository = new ConviteRepository();
                
                IList<ViewConvite> convites = _conviteRepository.GetTodosConvites();
                
                ViewBag.Convites = convites;
            }

            ViewBag.Participante = participante;

            ViewBag.PrazoEncerrado = PrazoEncerrado();

            return (View());
        }

        private bool PrazoEncerrado()
        {
            ParametroRepository parametroRepository = new ParametroRepository();

            Parametros parametros = parametroRepository.FindOne();

            if (parametros.DataFinalConvites < DateTime.Today.Date)
            {
                return (true);
            }
            else
            {
                return (false);
            }

        }

        [HttpGet]
        public ActionResult New()
        {
            
            if (PrazoEncerrado())
            {
                TempData["Mensagem"] = "Prazo encerrado para convites";
                return (View("../Shared/_EmBranco"));
            }
            else
            {
                return (View());
            }

            
        }

        public ActionResult EnviarConvite(string email, string nome)
        {
            
            try
            {
                ConviteService conviteService = new ConviteService();

                conviteService.ValidarConvidado(email);

                string participanteCod = this.User.Identity.Name;

                ParticipanteRepository participanteRepository = new ParticipanteRepository();

                Participante participante = participanteRepository.FindByCodigo(participanteCod);

                Convite convite = conviteService.GetNovoConvite(email, nome, participante);

                string path = HttpContext.Request.PhysicalApplicationPath;
                
                conviteService.EnviarConvite(convite, path);

                conviteService.AtualizarSituacaoDeEnvio(convite);

                conviteService.AtualizarSituacaoLista(email);

                NotificacaoService notificacaoService = new NotificacaoService();

                notificacaoService.NotificarConviteConvidado(convite);

            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return (PartialView("_EmBranco"));

        }

        [HttpPost]
        public ActionResult New(ConviteViewModel model)
        {

            if (ModelState.IsValid)
            {

                try
                {
                    _conviteService = new ConviteService();

                    _conviteService.ValidarConvidado(model.Email);

                    string participanteCod = this.User.Identity.Name;

                    _participanteRepository = new ParticipanteRepository();

                    Participante participante = _participanteRepository.FindByCodigo(participanteCod);

                    Convite convite = _conviteService.GetNovoConvite(model.Email, model.Nome, participante);

                    string path = HttpContext.Request.PhysicalApplicationPath;

                    _conviteService.EnviarConvite(convite, path);

                    _conviteService.AtualizarSituacaoDeEnvio(convite);

                    _conviteService.AtualizarSituacaoLista(model.Email);

                    NotificacaoService notificationService = new NotificacaoService();

                    notificationService.NotificarConviteConvidado(convite);

                    TempData["Mensagem"] = String.Format("{0} foi convidado com sucesso!", model.Nome);

                    return (RedirectToAction("Index", "Convite"));

                }
                catch (Exception ex)
                {
                    TempData["Erro"] = ex.Message;
                    return (View(model));
                }
            }
            else
            {
                return (View(model));
            }
        }

        public PartialViewResult ReenviarConvite(Guid key)
        {
            try
            {
                _conviteRepository = new ConviteRepository();

                Convite convite = _conviteRepository.GetByKey(key);
                
                string path = HttpContext.Request.PhysicalApplicationPath;

                _conviteService = new ConviteService();

                _conviteService.EnviarConvite(convite, path);

                TempData["Mensagem"] = "Email reenviado com sucesso!";

            }
            catch (Exception ex)
            {
                
                TempData["Erro"] = "Não foi possível reenviar o convite:" + ex.Message;

            }

            return (PartialView("../Shared/_Result"));

        }

        public void Excluir(Guid key)
        {
            try
            {
                ConviteRepository rep = new ConviteRepository();

                Convite convite = rep.GetByKey(key);

                if (convite.Status == 0)
                    rep.Delete(convite);
                else
                    throw new Exception("Convite não pode ser excluído!");

            }
            catch (Exception ex)
            {

                throw ex;

            }

        }

        public ViewResult List()
        {

            if (PrazoEncerrado())
            {
                TempData["Mensagem"] = "Prazo encerrado para convites";
                return (View("../Shared/_EmBranco"));
            }
            else
            {
                return (View());
            }

        }
        
        [OutputCache(Duration=0)]
        public JsonResult getHistoricoConvites()
        {
 
            string participanteCod = HttpContext.User.Identity.Name;

            _participanteRepository = new ParticipanteRepository();

            Participante participante = _participanteRepository.FindByCodigo(participanteCod);

            _conviteRepository = new ConviteRepository();

            IList historicoConvites = _conviteRepository.GetHistoricoConvites(participante.Usuario.Email);

            return (Json(historicoConvites, JsonRequestBehavior.AllowGet));

        }

    }
}