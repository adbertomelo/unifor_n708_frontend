using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bolao10.Persistence.Repository;
using Bolao10.Model.Entities;
using Bolao10.Model.Entities.Views;
using Bolao10.Web.Filters;
using Bolao10.Services;

namespace Bolao10.Web.Controllers
{
    [TokenFilter]
    public class RankingController : Controller
    {

        RankingRepository _rankingRepository;
        BolaoRepository _bolaoRepository;
        ParticipanteService _participanteService;
        public RankingController()
        {
            _rankingRepository = new RankingRepository();
            _bolaoRepository = new BolaoRepository();
            _participanteService = new ParticipanteService();
        }

        public ActionResult Index()
        {
            
            IList<ViewRanking> ranking = _rankingRepository.GetRanking();

            ViewBag.Ranking = ranking;

            return View();

        }

        public JsonResult GetApostas(string codigoParticipante)
        {
            
            ParticipanteRepository participanteRepository = new ParticipanteRepository();
            
            Participante participante = participanteRepository.FindByCodigo(codigoParticipante);
            
            ApostaRepository apostaRepository = new ApostaRepository();
            
            IList<ViewApostas> apostas = apostaRepository.GetApostasPorParticipante(participante);
            
            return (Json(apostas, JsonRequestBehavior.AllowGet));

        }

        public JsonResult GetParticipante()
        {
            string codParticipante = _participanteService.GetCodigo();
            return (Json(codParticipante, JsonRequestBehavior.AllowGet));
        }

        public JsonResult GetRanking()
        {
            IList<ViewRanking> Ranking = _rankingRepository.GetRanking();

            var data = new { ranking = Ranking, participante = this.User.Identity.Name };

            return (Json(data, JsonRequestBehavior.AllowGet));

        }

        public ViewResult Chart()
        {
            return (View());
        }

        public JsonResult GetDataChart(string codigoParticipante)
        {

            AccountService accountService = new AccountService();

            string codParticipante = _participanteService.GetCodigo();
            ParticipanteService participanteService = new ParticipanteService();
            Participante participante = participanteService.GetByCodigo(codParticipante);


            ParticipanteRepository participanteRepository = new ParticipanteRepository();

            Participante participanteConsultado = participanteRepository.FindByCodigo(codigoParticipante);

            ArrayList data = _rankingRepository.GetDataChart(participante.Id, participanteConsultado.Id);

            return (Json(data, JsonRequestBehavior.AllowGet));

        }

        //public void Marcar(string codigoParticipanteMarcado)
        //{

        //    AccountService accountService = new AccountService();
            
        //    Participante participante = accountService.GetParticipante(this.HttpContext);

        //    ParticipanteRepository participanteRepository = new ParticipanteRepository();
            
        //    Participante participanteMarcado = participanteRepository.FindByCodigo(codigoParticipanteMarcado);

        //    Marcacao marcacao = new Marcacao(participante.Id, participanteMarcado.Id);

        //    MarcacaoRepository marcacaoRepository = new MarcacaoRepository();

        //    marcacaoRepository.Save(marcacao);
           
        //}

        //public void Desmarcar(string codigoParticipanteMarcado)
        //{

        //    AccountService accountService = new AccountService();
            
        //    Participante participante = accountService.GetParticipante(this.HttpContext);

        //    ParticipanteRepository participanteRepository = new ParticipanteRepository();
            
        //    Participante participanteMarcado = participanteRepository.FindByCodigo(codigoParticipanteMarcado);

        //    MarcacaoRepository marcacaoRepository = new MarcacaoRepository();

        //    Marcacao marcacao = marcacaoRepository.GetAllFilteredBy(x => x.ParticipanteMaracadoId == participanteMarcado.Id
        //        && x.ParticipanteId == participante.Id).SingleOrDefault();

        //    if (marcacao != null)
        //        marcacaoRepository.Delete(marcacao);


        //}

	}
}