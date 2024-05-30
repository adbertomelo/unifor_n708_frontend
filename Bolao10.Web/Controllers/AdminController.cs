using Bolao10.Model.Entities;
using Bolao10.Persistence.Repository;
using Bolao10.Services;
using Bolao10.Web.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Bolao10.Controllers
{

    [TokenFilter]
    public class AdminController : Controller
    {

        AdminService _adminService;

        [HttpPost]
        public ActionResult SalvarArquivo()
        {
            string path = Server.MapPath("~/documents/Files");
            Request.Files["arquivo"].SaveAs(path);
            return (View());
        }

        public ViewResult Upload()
        {
            return (View());
        }

        public ViewResult GerarApostas()
        {

            ParticipanteRepository participanteRepository = new ParticipanteRepository();
            
            BolaoRepository bolaoRepository = new BolaoRepository();
            
            var participantes = participanteRepository.FindByBolao(bolaoRepository.FindAtivo());

            ApostaService apostaService = new ApostaService();

            foreach (Participante p in participantes)
            {
                apostaService.CheckParticipante(p);
            }


            return View();
        }

        public AdminController()
        {
            _adminService = new AdminService();
        }

        public ViewResult ApostasPendentes()
        {
            return (View());
        }
        
        public ViewResult LancarPlacar()
        {
            return(View());
        }


        public ViewResult ListarParticipantesPendentesPagamento()
        {
            BolaoRepository bolaoRepository = new BolaoRepository();

            Bolao bolaoAtual = bolaoRepository.FindAtivo();

            ParticipanteRepository participanteRepository = new ParticipanteRepository();

            var participantes = participanteRepository.FindByPendentesDePagamento(bolaoAtual);

            ViewBag.Participantes = participantes;

            RecebimentoPixRepository recebimentoPixRepository = new RecebimentoPixRepository();

            IList<RecebimentoPix> recebimentos = recebimentoPixRepository.Pendentes();

            ViewBag.RecebimentosPix = recebimentos;

            return (View());
        }

        public ActionResult LiberarParticipante(string codigo, int pix)
        {
            ParticipanteRepository participanteRepository = new ParticipanteRepository();

            Participante participante = participanteRepository.FindByCodigo(codigo);

            participante.SituacaoPagamento = 2;
            
            participante.Status = 1;

            participanteRepository.Update(participante);

            RecebimentoPixRepository recebimentoPixRepository = new RecebimentoPixRepository();

            RecebimentoPix recebimentoPix = recebimentoPixRepository.GetById(pix);

            recebimentoPix.IdParticipante = participante.Id;

            recebimentoPixRepository.Update(recebimentoPix);

            ViewBag.Participante = participante;

            TempData["Mensagem"] = $"{participante.Usuario.Nome} liberado com sucesso!";

            return (RedirectToAction("ListarParticipantesPendentesPagamento"));
        }

        public ViewResult ConfirmacaoLiberacao(string codigo, int pixSelecionado)
        {
            ParticipanteRepository participanteRepository = new ParticipanteRepository();

            Participante participante = participanteRepository.FindByCodigo(codigo);

            ViewBag.Participante = participante;
            ViewBag.Pix = pixSelecionado; 

            return (View());
        }
        public JsonResult GetApostasPendentes()
        {


            ApostaRepository apostaRepository = new ApostaRepository();

            IList apostasPendentes = apostaRepository.GetApostasPendentes();

            return (Json(apostasPendentes, JsonRequestBehavior.AllowGet));

        }

        public void EnviarEmail(string email, string nome, int rodadaId)
        {


            try
            {

                _adminService.EnviarEmail(email, nome,rodadaId);

            }
            catch (Exception ex)
            {
                throw ex;
            }

            

        }

    }

}

