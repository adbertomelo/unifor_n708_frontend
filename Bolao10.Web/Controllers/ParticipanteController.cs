using Bolao10.Model.Entities;
using Bolao10.Persistence.Repository;
using Bolao10.Services;
using Bolao10.Web.Filters;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Bolao10.Controllers
{
    [TokenFilter]
    public class ParticipanteController : Controller
    {

        private AccountService _accountService;

        private ParticipanteRepository _participanteRepository;

        public ParticipanteController()
        {
            _accountService = new AccountService();
            _participanteRepository = new ParticipanteRepository();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {

            ParticipanteService participanteService = new ParticipanteService();
            string codParticipante = participanteService.GetCodigo();            
            Participante participante = participanteService.GetByCodigo(codParticipante);


            IList<Participante> participantes = _participanteRepository.FindByBolao(participante.Bolao);

            ViewBag.Participantes = participantes;
            
            ViewBag.Total = participantes.Count;
            
            return ( View() );
        }



    }
}
