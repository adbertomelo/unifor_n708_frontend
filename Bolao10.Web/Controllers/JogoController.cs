using Bolao10.Model.Entities;
using Bolao10.Model.Entities.Views;
using Bolao10.Persistence.Repository;
using Bolao10.Services;
using Bolao10.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Bolao10.Web.Controllers
{
    [TokenFilter]
    public class JogoController : Controller
    {

        private JogoRepository _jogoRepository;

        public JogoController()
        {
            
        }

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult RequisitarDadosParaAsEstatisticas(int jogoId)
        {
            
            ApostaRepository _apostaRepository = new ApostaRepository();
            
            IList<ViewApostas> apostasPorJogo = _apostaRepository.GetApostasPorJogo(jogoId);

            var data = new { apostas = apostasPorJogo, participante = this.User.Identity.Name }; 

            return (Json(data, JsonRequestBehavior.AllowGet));

        }

        public PartialViewResult Salvar(int jogoId, int gols1, int gols2)
        {


            try
            {
                AccountService accountService = new AccountService();

                ParticipanteService participanteService = new ParticipanteService();
                string codParticipante = participanteService.GetCodigo();                
                Participante participante = participanteService.GetByCodigo(codParticipante);


                JogoRepository jogoRepository = new JogoRepository();

                Jogo jogo = jogoRepository.GetById(jogoId);

                JogoService jogoService = new JogoService();

                jogoService.Salvar(participante, jogo, gols1, gols2);

                IndicadorService indicadorService = new IndicadorService();

                TempData["Mensagem"] = String.Format("Jogo {0}X{1} salvo com sucesso.", jogo.Time1.Nome, jogo.Time2.Nome);

            }
            catch(Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }
            
            
            return(PartialView("_Result"));
            
        }

        public JsonResult GetJogosDasRodadasFechadas()
        {

            _jogoRepository = new JogoRepository();

            IList<Jogo> jogos = _jogoRepository.GetJogoDasRodadasFechadas();

            var d = from jogo in jogos
                    group jogo by jogo.Dia.Date into diaJogos
                    orderby diaJogos.Key ascending
                    select new { dia = diaJogos.Key, jogos = diaJogos, arealizar = (diaJogos.Key.Date >= DateTime.UtcNow.AddHours(-3).Date) };

            return (Json(d, JsonRequestBehavior.AllowGet));

        }


	}
}