using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bolao10.Services;
using Bolao10.Persistence.Repository;
using Bolao10.Model.Entities;
using Bolao10.Web.Filters;

namespace Bolao10.Web.Controllers
{

    [TokenFilter]
    public class ApostaController : Controller
    {

        ParticipanteRepository _participanteRepository;
        ApostaRepository _apostaRepository;
        ApostaHistRepository _apostaHistRepository;
        FaseRepository _faseRepository;
        RodadaRepository _rodadaRepository;
        AccountService _accountService;


        ApostaService _apostaService;
        BolaoService _bolaoService;
        public ApostaController() 
        {
            _faseRepository = new FaseRepository();
            _rodadaRepository = new RodadaRepository();
            _participanteRepository = new ParticipanteRepository();
            _apostaRepository = new ApostaRepository();
            _apostaHistRepository = new ApostaHistRepository();
            _accountService = new AccountService();

            _apostaService = new ApostaService();
            _bolaoService = new BolaoService();

        }

        //public void List()
        //{


        //    string codParticipante = _participanteService.GetCodigo();
        //    ParticipanteService participanteService = new ParticipanteService();
        //    Participante participante = participanteService.GetByCodigo(codParticipante);


        //    ViewBag["apostas"] = Aposta.FindByParticipante(participante);

        //}

        public ViewResult Index()
        {

            ParticipanteService participanteService = new ParticipanteService();
            
            string codParticipante = participanteService.GetCodigo();
            
            Participante participante = participanteService.GetByCodigo(codParticipante);

            Bolao bolao = _bolaoService.FindAtivo();

            IList<Aposta> apostas = _apostaService.GetByParticipante(participante);

            ViewBag.Apostas = apostas;

            IList<Fase> fases = _bolaoService.GetFases(bolao); //_faseRepository.GetAll();
            
            ViewBag.Fases = fases;

            IList<Rodada> rodadas = _bolaoService.GetRodadas(bolao); // _rodadaRepository.GetAll();

            ViewBag.Rodadas = rodadas;

            Rodada rodadaCorrente = _bolaoService.GetRodadaCorrente(rodadas); //_rodadaRepository.FindRodadaCorrente();

            ViewBag.RodadaCorrente = rodadaCorrente;

            Fase faseCorrente;

            if (rodadaCorrente != null)
                faseCorrente = rodadaCorrente.Fase;
            else
                faseCorrente = fases[0]; // _faseRepository.FindFirt();

            ViewBag.FaseCorrente = faseCorrente;

            ViewBag.NumRodadasAbertas = _bolaoService.TotalRodadasAbertas(rodadas);

            return (View());

        }

        private int numRodadasAbertas(Bolao bolao)
        {
            var rodadasAbertas = _bolaoService.RodadasAbertas(bolao);

            return rodadasAbertas; //(_rodadaRepository.FindAllAbertas().Count);
        }

        public PartialViewResult Update(Aposta[] apostas)
        {

            int? gols1, gols2;

            try
            {

                ParticipanteService participanteService = new ParticipanteService();
                
                string codParticipante = participanteService.GetCodigo();
                
                Participante participante = participanteService.GetByCodigo(codParticipante);


                int? acessoID = _accountService.GetAcessoId();

                int numApostas = 0;

                IList<Aposta> apostasRodadasAbertas = _apostaRepository.GetApostasDasRodadasAbertas(participante);

                foreach (Aposta aposta in apostas)
                {
                    gols1 = aposta.Gols1;
                    gols2 = aposta.Gols2;

                    if ((gols1 != null) && (gols2 != null))
                    {
                        if ((gols1 >= 0) && (gols2 >= 0))
                        {
                            Aposta apostaBD = apostasRodadasAbertas.Where(x => x.Id == aposta.Id).SingleOrDefault<Aposta>();

                            if (apostaBD == null)
                                continue;
    
                            if (gols1 != apostaBD.Gols1 || gols2  != apostaBD.Gols2)
                            {
                                if (apostaBD.Jogo.Rodada.Aberta)
                                {
                                    if (apostaBD.Participante == participante)
                                    {

                                        ApostaHist apostaHist = new ApostaHist(apostaBD.Id, apostaBD.Gols1, gols1, apostaBD.Gols2, gols2, acessoID);

                                        apostaBD.Gols1 = gols1;

                                        apostaBD.Gols2 = gols2;

                                        _apostaRepository.Update(apostaBD);

                                        _apostaHistRepository.Save(apostaHist);

                                        numApostas += 1;

                                    }
                                }
                            }
                        }
                    }
                }

                if (numApostas > 0)
                {
                    string texto = numApostas == 1 ? "aposta foi alterada" : "apostas foram alteradas";
                    TempData["Mensagem"] = String.Format("{0} {1} com sucesso!", numApostas, texto);
                }
                else
                {
                    TempData["Mensagem"] = "Nenhuma aposta foi alterada.";
                }

            }
            catch (Exception ex)
            {
                TempData["Erro"] = String.Format("Erro ao salvar as apostas. Erro = {0}", ex.Message);
            }

            return (PartialView("Result"));

        }


	}
}