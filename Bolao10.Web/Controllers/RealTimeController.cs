using Bolao10.Model.Entities;
using Bolao10.Persistence.Repository;
using Bolao10.Web.Hubs;
using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Bolao10.Web.Controllers
{
    public class RealTimeController : Controller
    {

        public void AtualizarMelhorPosicaoParticipantes()
        {
            
        }
        public void AtualizaPaineis(int jogoid, int gols1, int gols2)
        {

            ParticipanteRepository participanteRepository = new ParticipanteRepository();

            IList<Participante> participantes = participanteRepository.GetAllFilteredBy(x => x.Status == 1);

            var context = GlobalHost.ConnectionManager.GetHubContext<BolaoHub>();

            foreach (var p in participantes)
            {
                context.Clients.All.atualizarPaineisRealTime(p.Codigo, p.PosicaoOnline, null, 0);
            }

        }

        public void AtualizarPaineis()
        {

            ParticipanteRepository participanteRepository = new ParticipanteRepository();

            IList<Participante> participantes = participanteRepository.GetAllFilteredBy(x => x.Status == 1);

            var context = GlobalHost.ConnectionManager.GetHubContext<BolaoHub>();

            foreach (var p in participantes)
            {
                context.Clients.All.atualizarPaineisRealTime(p.Codigo, p.PosicaoOnline, null, 0);
            }


        }

	}
}