using Bolao10.Model.Entities;
using Bolao10.Services;
using Bolao10.Web.Filters;
using Bolao10.Web.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using System.Web.Mvc;

namespace Bolao10.Web.Controllers
{

    [TokenFilter]
    public class HomeController : Controller
    {

        readonly ParticipanteService _participanteService;
        readonly BolaoService _bolaoService;
        readonly ConviteService _conviteService;
        readonly ParametrosService _parametroService;
        readonly ChatService _chatService;
        readonly TimeService _timeService;
        public HomeController()
        {
            _participanteService = new ParticipanteService();
            _bolaoService = new BolaoService();
            _conviteService = new ConviteService();
            _parametroService = new ParametrosService();
            _chatService = new ChatService();
            _timeService = new TimeService();

        }

        public ViewResult Enquete()
        {
            
            var participantes = _participanteService.GetAll();
            
            var times = _bolaoService.GetTimes();

            var totalGeral = (from t1 in times
                              join p0 in participantes on t1.Id equals p0.TimeCampeao
                              select t1).Count();

            var res1 = from t1 in times
                       join p0 in participantes on t1.Id equals p0.TimeCampeao
                       group t1 by t1.Nome into g
                       select new Enquete()
                       {
                           Time = g.Key,
                           Total = g.Count(),
                           Perc = (Math.Round(g.Count() / (decimal)totalGeral, 4, MidpointRounding.AwayFromZero)) * 100
                       };

            ViewBag.Dados = res1.OrderByDescending(x => x.Perc);

            return (View());
        }
        public ViewResult Regras()
        {
            Bolao bolao = _bolaoService.FindAtivo();
            
            Parametros param = _bolaoService.GetParametros(bolao);
            
            ViewBag.EmailAdmin = param.EmailAdmin;
            
            ViewBag.Valor = bolao.Valor;

            return (View());
        }
        public ActionResult Index()
        {

            string participanteCod = _participanteService.GetCodigo(); // HttpContext.User.Identity.Name;

            Participante participante = _participanteService.GetByCodigo(participanteCod);
            
            Bolao bolao =  _bolaoService.FindAtivo();

            bool bolaoIniciado = _bolaoService.BolaoIniciado(bolao);

            ViewBag.BolaoIniciado = bolaoIniciado;
            
            ViewBag.Participante = participante;
          
            return View();

        }

        [OutputCache(Duration=60)]
        public JsonResult PegarComentariosDoMural()
        {
            var comentarios = (IList<Comentario>)this.HttpContext.Cache["Comentarios"];

            if ( comentarios == null )
            {

                comentarios = _chatService.GetAll();
                
                this.HttpContext.Cache.Insert("Comentarios", comentarios, null, Cache.NoAbsoluteExpiration, TimeSpan.FromDays(1));

            }

            var results = new JsonResult() { Data = new { Comentarios = comentarios }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            return(results);
        }
        public PartialViewResult Indicadores2()
        {
            string participanteCod = _participanteService.GetCodigo(); // this.HttpContext.User.Identity.Name;

            IndicadorService indicadorService = new IndicadorService();

            Participante participante = _participanteService.GetByCodigo(participanteCod);
            
            IList<Indicador> indicadores = indicadorService.GetIndicadores(participante);
            
            ViewBag.Indicadores = indicadores;

            ViewBag.Participante = participante;
           
            return (PartialView());
        }
        public JsonResult GetProximoPlacar(string jogoAtual)
        {

            string codParticipante = _participanteService.GetCodigo(); //this.HttpContext.User.Identity.Name;
            Participante participante = _participanteService.GetByCodigo(codParticipante);


            IndicadorService indicadorService = new IndicadorService();

            Indicador indicador = indicadorService.GetProximoPlacar(participante, jogoAtual);

            return new JsonResult() { Data = indicador, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }
        public JsonResult GetProximoIndicador(string codIndicadorAtual)
        {


            string codParticipante = _participanteService.GetCodigo();
            Participante participante = _participanteService.GetByCodigo(codParticipante);


            IndicadorService indicadorService = new IndicadorService();

            Indicador indicador = indicadorService.GetProximoIndicador(codIndicadorAtual, participante);

            return new JsonResult() { Data = indicador, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }
        public ViewResult Notificacoes()
        {
         
            string codParticipante = _participanteService.GetCodigo();
            Participante participante = _participanteService.GetByCodigo(codParticipante);


            NotificacaoService notificacaoService = new NotificacaoService();

            ViewBag.Notificacoes = notificacaoService.TodasNotificacoes(participante);

            return View();
        }

        [HttpPost]
        public void MarcarNotificacoesComoLidas()
        {

            string codParticipante = _participanteService.GetCodigo();
            Participante participante = _participanteService.GetByCodigo(codParticipante);


            NotificacaoService notificacaoService = new NotificacaoService();

            notificacaoService.MarcarComoLidas(participante);


        }


	}

    public class Enquete
    {
        public string Time { get; set; }
        public decimal Perc { get; set; }

        public int Total { get; set; }
    }

}