using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Web;
using Bolao10.Model.Entities;
using Bolao10.Persistence.Repository;
using System.Net.Http.Json;

namespace Bolao10.Services
{
    public class ApostaService
    {

        FaseRepository _faseRepository;
        JogoRepository _jogoRepository;
        ApostaRepository _apostaRepository;
        RodadaRepository _rodadaRepository;
        ParticipanteRepository _participanteRepository;

        ApiService _apiService;
        AccountService _accountService;
        public ApostaService()
        {
            _faseRepository = new FaseRepository();
            _jogoRepository = new JogoRepository();
            _apostaRepository = new ApostaRepository();
            _rodadaRepository = new RodadaRepository();
            _participanteRepository = new ParticipanteRepository();

            _apiService = new ApiService();
            _accountService = new AccountService();
        }

        public void CriarApostas(Participante participante)
        {
            CheckParticipante(participante);
        }

        public void CriarApostasPorRodada(Participante participante, Rodada rodada)
        {

            try
            {

                IList<Jogo> jogos = _jogoRepository.FindAllByRodada(rodada);
                IList<Aposta> apostas = new List<Aposta>();
                IList<Aposta> apostasBD = _apostaRepository.GetByParticipante(participante);

                foreach (Jogo jogo in jogos)
                {
                    //if (!_apostaRepository.Existe(participante, jogo))
                    if(apostasBD.Where(x => x.Jogo == jogo).SingleOrDefault<Aposta>() == null)
                    {
                        Aposta aposta = new Aposta(participante, jogo);
                        apostas.Add(aposta);
                        
                    }
                }

                _apostaRepository.SalvarApostas(apostas);
            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        public void CheckParticipante(Participante participante)
        {

            IList<Rodada> rodadas = _rodadaRepository.FindAllAbertas();

            foreach (Rodada rodada in rodadas)
            {
                int totalJogosParticipanteRodada = _apostaRepository.TotalJogos(participante, rodada);

                int totalJogosRodada = _jogoRepository.TotalJogos(rodada);

                if (totalJogosParticipanteRodada < totalJogosRodada)
                {
                    CriarApostasPorRodada(participante, rodada);
                }
                else
                {
                    participante.NumApostaRodadaCorrenteOk = "S";
                    _participanteRepository.Update(participante);
                }
            }

        }

        internal IList<Aposta> GetByParticipante(Participante participante)
        {
            string url = $"{_apiService.GetUrlBase()}/Apostas/GetByParticipante?idParticipante={participante.Id}";

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountService.GetToken());

                var res = client.GetAsync(url);

                if (res.Result.EnsureSuccessStatusCode().StatusCode == HttpStatusCode.OK)
                {
                    return res.Result.Content.ReadFromJsonAsync<List<Aposta>>().Result;
                }
                else
                    throw new Exception("Erro ao obter as apostas!");


            }
        }
    }
}