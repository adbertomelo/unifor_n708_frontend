using Bolao10.Model.Entities;
using Bolao10.Persistence.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Bolao10.Services
{
    public class BolaoService
    {

        const int RODADA_ABERTA = 1;

        AccountService _accountService;
        ApiService _apiService;
        public BolaoService()
        {
            _accountService = new AccountService();
            _apiService = new ApiService();
        }
        public void ValidarBolao(Bolao bolao)
        {
            
            if (bolao.Situacao == 0)
            {
                throw new Exception(String.Format("Bolão {} não está ativo.", bolao.Descricao));
            }

        }

        public bool BolaoIniciado(Bolao bolao)
        {
            Parametros parametros = new ParametroRepository().FindOne();

            if (DateTime.Today > parametros.DataFinalConvites)
                return true;

            return false;

        }

        public Bolao FindAtivo()
        {
            string url = $"{_apiService.GetUrlBase()}/Boloes/Ativo";

            using (var client = new HttpClient())
            {

                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountService.GetToken());

                    var res = client.GetAsync(url);

                    if (res.Result.EnsureSuccessStatusCode().StatusCode == HttpStatusCode.OK)
                    {
                        return res.Result.Content.ReadFromJsonAsync<Bolao>().Result;
                    }
                    else
                        throw new Exception("Login inválido!");
                }
                catch
                {
                    return null;
                }

            }
        }

        internal IList<Fase> GetFases(Bolao bolao)
        {
            string url = $"{_apiService.GetUrlBase()}/Boloes/GetFases?idBolao={bolao.Id}";

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountService.GetToken());

                var res = client.GetAsync(url);

                if (res.Result.EnsureSuccessStatusCode().StatusCode == HttpStatusCode.OK)
                {
                    return res.Result.Content.ReadFromJsonAsync<List<Fase>>().Result;
                }
                else
                    throw new Exception("Erro ao obter as apostas!");


            }
        }
    
        internal IList<Rodada> GetRodadas(Bolao bolao)
        {
            string url = $"{_apiService.GetUrlBase()}/Boloes/GetRodadas?idBolao={bolao.Id}";

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountService.GetToken());

                var res = client.GetAsync(url);

                if (res.Result.EnsureSuccessStatusCode().StatusCode == HttpStatusCode.OK)
                {
                    return res.Result.Content.ReadFromJsonAsync<List<Rodada>>().Result;
                }
                else
                    throw new Exception("Erro ao obter as apostas!");


            }
        }

        internal Rodada GetRodadaCorrente(Bolao bolao)
        {
            throw new NotImplementedException();
        }

        internal Rodada GetRodadaCorrente(IList<Rodada> rodadas)
        {
            foreach (var r in rodadas.Where(x => x.Status == RODADA_ABERTA)
                    .OrderBy(x => x.DataEncerramento).ToList())
            {
                return (r);
            }

            return (rodadas.Where(x => x.Id > 0).OrderBy(x => x.Id).ToList()[0]);
        }
        internal Fase GetPrimeiraFase(Bolao bolao)
        {
            throw new NotImplementedException();
        }

        internal int RodadasAbertas(Bolao bolao)
        {
            throw new NotImplementedException();
        }

        internal int TotalRodadasAbertas(IList<Rodada> rodadas)
        {
            return (from a in rodadas where a.Aberta == true select a).Count();
        }

        internal List<Time> GetTimes()
        {
            string url = $"{_apiService.GetUrlBase()}/Boloes/Times";

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountService.GetToken());

                var res = client.GetAsync(url);

                if (res.Result.EnsureSuccessStatusCode().StatusCode == HttpStatusCode.OK)
                {
                    return res.Result.Content.ReadFromJsonAsync<List<Time>>().Result;
                }
                else
                    throw new Exception("Erro ao obter as apostas!");


            }
        }

        internal Parametros GetParametros(Bolao bolao)
        {

            string url = $"{_apiService.GetUrlBase()}/Boloes/Parametros?idBolao={bolao.Id}";

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountService.GetToken());

                var res = client.GetAsync(url);

                if (res.Result.EnsureSuccessStatusCode().StatusCode == HttpStatusCode.OK)
                {
                    return res.Result.Content.ReadFromJsonAsync<Parametros>().Result;
                }
                else
                    throw new Exception("Erro ao obter as apostas!");


            }

        }
    }
}