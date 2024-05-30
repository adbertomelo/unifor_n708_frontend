using Bolao10.Model.Entities;
using Bolao10.Model.Entities.Views;
using Bolao10.Persistence.Repository;
using Bolao10.Services;
using Bolao10.Web.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Bolao10.Web.Controllers
{
    [TokenFilter]
    public class SimulacaoController : Controller
    {

        JogoRepository _jogoRepository;
        ApostaRepository _apostaRepository;
        BolaoRepository _bolaoRepository;
        AccountService _accountService;
        
        public SimulacaoController()
        {
            _jogoRepository = new JogoRepository();
            _apostaRepository = new ApostaRepository();
            _bolaoRepository = new BolaoRepository();

        }

        public ActionResult Index()
        {
            return (View());
        }

        public JsonResult GetTodosOsJogos()
        {

            _accountService = new AccountService();
            ParticipanteService participanteService = new ParticipanteService();
            string codParticipante = participanteService.GetCodigo();            
            Participante participante = participanteService.GetByCodigo(codParticipante);

            IList<ViewJogoSimulador> jogos = _jogoRepository.GetJogosParticipante(participante);

            var d = from jogo in jogos
                    group jogo by jogo.Dia.Date into diaJogos
                    orderby diaJogos.Key ascending
                    select new { dia = diaJogos.Key, jogos = diaJogos, arealizar = diaJogos.Key.Date >= DateTime.UtcNow.AddHours(-3).Date };

            var data = new { Jogos = d, Participante = participante };

            return (Json(data, JsonRequestBehavior.AllowGet ));

        }

        public JsonResult GetApostas()
        {

            Bolao bolao = _bolaoRepository.FindAtivo();

            ArrayList apostas = _apostaRepository.GetApostas(bolao);

            return (Json(apostas, JsonRequestBehavior.AllowGet));

        }

        
    }
}