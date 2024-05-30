using Bolao10.Model.Entities;
using Bolao10.Persistence.Repository;
using Bolao10.Web.HttpModels;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.PeerToPeer;
using System.Text;
using System.Web;

namespace Bolao10.Services
{
    public class ParticipanteService
    {

        ApiService _apiService;
        AccountService _accountService;

        public ParticipanteService()
        {
            _apiService = new ApiService();
            _accountService = new AccountService();
        }

        public Participante CriarParticipante(Bolao bolao, Usuario usuario)
        {
            Participante participante = null;

            try
            {
                new BolaoService().ValidarBolao(bolao);

                new UsuarioService().ValidarUsuario(usuario);
                
                ParticipanteRepository participanteRepo = new ParticipanteRepository();

                participanteRepo.CriarParticipante(usuario, bolao);

                participante = participanteRepo.FindByUsuarioAndBolao(usuario, bolao);

            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return participante;

        }

        public void AtualizarHistorico(Participante participante)
        {
            ParticipanteRepository participanteRepository = new ParticipanteRepository();

            IList historicos = participanteRepository.GetHistorico(participante.Usuario.Email);

            if (historicos != null)
            {

                IList historico = (IList)historicos[0];

                int melhorPosicao = (int)historico[0];
                string nomeBolaoMelhorPosicao = historico[1].ToString();
                int numBoloesParticipou = (int)historico[2];
                string nomeBolesParticipou = historico[3].ToString();

                participante.MelhorColocacaoBoloesPassados = melhorPosicao;
                participante.NomeBolaoMelhorColocacao = nomeBolaoMelhorPosicao;
                participante.NumBoloesParticipou = numBoloesParticipou;
                participante.NomeBoloesParticipou = nomeBolesParticipou;

                participanteRepository.Update(participante);

            }

        }

        public void AtualizarOutrasInfo(Participante participante, Guid codConvite, int idTimeCampeao)
        {
            Convite convite = new ConviteRepository().GetByKey(codConvite);

            participante.IdConvidador = convite.Participante.Id;

            participante.TimeCampeao = idTimeCampeao;

            new ParticipanteRepository().Update(participante);

        }

        internal Participante FindByUsuarioAndBolao(Usuario usuario, Bolao bolao)
        {
            string url = $"{_apiService.GetUrlBase()}/Participante/GetByUsuarioBolao?idUsuario={usuario.Id}&idBolao={bolao.Id}";

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountService.GetToken());

                var res = client.GetAsync(url);

                if (res.Result.EnsureSuccessStatusCode().StatusCode == HttpStatusCode.OK)
                {
                    return res.Result.Content.ReadFromJsonAsync<Participante>().Result;
                }
                else
                    throw new Exception("Login inválido!");


            }
        }

        internal void AtualizarInformacoesAcesso(Participante participante)
        {
            string url = $"{_apiService.GetUrlBase()}/Participante/AtualizarInformacoesAcesso";

            using (var client = new HttpClient())
            {

                try
                {
                    
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountService.GetToken());

                    var res = client.PutAsync(url,
                      new StringContent(JsonConvert.SerializeObject(new {id = participante.Id}),
                        Encoding.UTF8, "application/json")
                    );

                    var statusCode = res.Result.EnsureSuccessStatusCode().StatusCode;

                }
                catch(Exception ex)
                {
                    var r = ex;
                }

            }

        }

        internal Participante GetByCodigo(string participanteCod)
        {
            string url = $"{_apiService.GetUrlBase()}/Participante/GetByCodigo?codigo={participanteCod}";

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountService.GetToken());

                var res = client.GetAsync(url);

                if (res.Result.EnsureSuccessStatusCode().StatusCode == HttpStatusCode.OK)
                {
                    return res.Result.Content.ReadFromJsonAsync<Participante>().Result;
                }
                else
                    throw new Exception("Participante não encontrado!");


            }
        }

        internal IList<Participante> GetAll()
        {
            string url = $"{_apiService.GetUrlBase()}/Participante/GetAll";

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountService.GetToken());

                var res = client.GetAsync(url);

                if (res.Result.EnsureSuccessStatusCode().StatusCode == HttpStatusCode.OK)
                {
                    return res.Result.Content.ReadFromJsonAsync<IList<Participante>>().Result;
                }
                else
                    throw new Exception("Nenhum participante encontrado!");


            }
        }

        internal string GetCodigo()
        {

            var codigo1 = HttpContext.Current.Request.Cookies["codigo1"];

//            var codigo = HttpContext.Current.Session["codigo"];

            if (codigo1 == null)
                return "";
            else
                return codigo1.Value;


        }
    }
}