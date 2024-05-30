using Bolao10.Model.Entities;
using Bolao10.Persistence.Repository;
using System;
using System.Collections.Generic;
using Bolao10.Model.Entities.Views;
using System.Linq;
using System.Net.Http;

namespace Bolao10.Services
{
    public class IndicadorService
    {

        const int CON_DIA_SEM_JOGO = 0;
        const int CON_DIA_COM_JOGO_ANTES_DO_JOGO = 1;
        const int CON_DIA_COM_JOGO_DURANTE_O_JOGO = 2;
        const int CON_DIA_COM_JOGO_APOS_O_JOGO = 3;
        const int CON_NUM_MINUTOS_JOGO = 115;

        const string CON_POSICAO_ATUAL = "PosicaoAtual";
        const string CON_NUM_PONTOS = "NumPontos";
        const string CON_PONTOS_PRIMEIRAS_POSICOES = "PontosDasPrimeirasPosicoes";
        const string CON_POSICAO_ANTERIOR = "PosicaoAnterior";
        const string CON_PLACARES_HOJE = "PlacaresHoje";
        const string CON_POSICAO_TEMPO_REAL = "PosicaoTempoReal";
        const string CON_MELHOR_POSICAO_DIA = "MelhorPosicaoDia";

        public DateTime GetDataBase()
        {
            return (DateTime.UtcNow.AddHours(-3));
        }
        private DateTime GetProxDataComJogo(DateTime dataBase)
        {

            JogoRepository jogoRepository = new JogoRepository();

            DateTime? proxDataBase = jogoRepository.GetProximaData(dataBase);

            if (proxDataBase == null)
                proxDataBase = dataBase.Date;

            return (proxDataBase.Value);
        }
        public IList<Indicador> GetIndicadores(Participante participante)
        {

            DateTime dataBase = GetDataBase();

            int situacao = GetSituacao(dataBase);

            List<Indicador> indicadores = new List<Indicador>();

            if (situacao == CON_DIA_SEM_JOGO)
            {

                indicadores.Add(GetIndicadorPosicaoAtual(participante));

                indicadores.Add(GetIndicadorNumPontos(participante));

                indicadores.Add(GetIndicadorPontosDasPrimeirasPosicoes());

                DateTime proxDiaComJogo = GetProxDataComJogo(dataBase.Date);

                Indicador placaresHoje = GetIndicadorPlacaresHoje(participante, proxDiaComJogo, null);

                placaresHoje.AlterarNome("Próximo Placar");

                indicadores.Add(placaresHoje);

            }
            else if (situacao == CON_DIA_COM_JOGO_ANTES_DO_JOGO)
            {
                indicadores.Add(GetIndicadorPosicaoAtual(participante));

                indicadores.Add(GetIndicadorNumPontos(participante));

                indicadores.Add(GetIndicadorMelhorPosicaoDia(participante));

                indicadores.Add(GetIndicadorPlacaresHoje(participante, dataBase, null));

            }
            else if (situacao == CON_DIA_COM_JOGO_DURANTE_O_JOGO)
            {

                indicadores.Add(GetIndicadorPosicaoAtual(participante));

                indicadores.Add(GetIndicadorNumPontos(participante));

                indicadores.Add(GetIndicadorPosicaoTempoReal(participante));

                indicadores.Add(GetIndicadorPlacaresHoje(participante, dataBase, null));

            }
            else if (situacao == CON_DIA_COM_JOGO_APOS_O_JOGO)
            {

                indicadores.Add(GetIndicadorPosicaoAtual(participante));

                indicadores.Add(GetIndicadorPosicaoAnterior(participante));

                indicadores.Add(GetIndicadorPontosDasPrimeirasPosicoes());

                if (TodosOsJogosFinalizados(dataBase))
                {
                    
                    DateTime proxDiaComJogo = GetProxDataComJogo(dataBase);

                    Indicador placaresHoje = GetIndicadorPlacaresHoje(participante, proxDiaComJogo, null);

                    placaresHoje.AlterarNome("Próximo Placar");

                    indicadores.Add(placaresHoje);

                }
                else
                {
                    indicadores.Add(GetIndicadorPlacaresHoje(participante, dataBase, null));
                }
                    

            }

            return (indicadores);

        }
        private Indicador GetIndicadorPosicaoAtual(Participante p)
        {
            Indicador indic = new Indicador(CON_POSICAO_ATUAL, "Posição Atual");
            indic.Add(new Valor(p.PosicaoAtual.ToString()));
            return (indic);
        }
        private Indicador GetIndicadorNumPontos(Participante p)
        {
            Indicador indic = new Indicador(CON_NUM_PONTOS, "Meus Pontos");
            indic.Add(new Valor(p.NumPontos.ToString()));
            return (indic);
        }
        private Indicador GetIndicadorPontosDasPrimeirasPosicoes()
        {

            RankingRepository rankingRepository = new RankingRepository();

            Indicador pontos = new Indicador(CON_PONTOS_PRIMEIRAS_POSICOES, "Primeiras Posições");

            var texto = "";

            foreach (Object[] p in rankingRepository.PrimeirasPosicoes())
            {
                var pts = p[1];
                if (String.IsNullOrEmpty(texto))
                {
                    texto = pts.ToString();
                }
                else
                {
                    texto = String.Format("{0}, {1}", texto, pts);
                }

            }

            if (!String.IsNullOrEmpty(texto))
            {
                texto = String.Format("{0}:{1}", texto, "pts");
            }
            else
            {
                texto = String.Format("{0}, {1}, {2}:pts", 0, 0, 0);
            }

            pontos.Add(new Valor(texto));

            return (pontos);
        }
        private Indicador GetIndicadorPosicaoAnterior(Participante p)
        {

            Indicador posicaoAnterior = new Indicador(CON_POSICAO_ANTERIOR, "Posição Anterior");

            posicaoAnterior.Add(new Valor(p.PosicaoAnterior.ToString()));

            return (posicaoAnterior);

        }
        private Indicador GetIndicadorPlacaresHoje(Participante participante, DateTime dataBase, string partidaAtual)
        {

            Indicador placaresHoje = new Indicador(CON_PLACARES_HOJE, "Meu Placar");

            placaresHoje.Add(GetValorIndicadorPlacaresHoje(participante, dataBase, partidaAtual));

            return (placaresHoje);

        }
        private Valor GetValorIndicadorPlacaresHoje(Participante participante, DateTime database, string partidaAtual)
        {

            string texto = "";

            IList<Aposta> apostas = GetApostasData(participante, database);

            string partida = "";

            int indexAtual = 0;

            foreach (Aposta aposta in apostas)
            {

                indexAtual += 1;

                partida = string.Format("{0} X {1}", aposta.Jogo.Time1.Sigla, aposta.Jogo.Time2.Sigla);

                if (partida == partidaAtual)
                {
                    break;
                }
                else
                {
                    if (partidaAtual == null)
                    {
                        int situacaoJogo = GetSituacaoJogo(aposta.Jogo, database);

                        if ((situacaoJogo == CON_DIA_COM_JOGO_ANTES_DO_JOGO) || (situacaoJogo == CON_DIA_COM_JOGO_DURANTE_O_JOGO) || (situacaoJogo == CON_DIA_SEM_JOGO))
                        {
                            indexAtual = indexAtual - 1;
                            break;
                        }

                    }
                }

            }

            int indexNovo = 0;

            foreach (Aposta aposta in apostas)
            {
                indexNovo += 1;

                partida = string.Format("{0} X {1}", aposta.Jogo.Time1.Sigla, aposta.Jogo.Time2.Sigla);

                if (indexAtual == apostas.Count)
                {
                    if (indexNovo <= indexAtual)
                    {
                        texto = string.Format("{0}X{1}:{2} X {3}", aposta.Gols1, aposta.Gols2, aposta.Jogo.Time1.Sigla, aposta.Jogo.Time2.Sigla);
                        break;
                    }

                }
                else
                {
                    if (indexNovo > indexAtual)
                    {
                        texto = string.Format("{0}X{1}:{2} X {3}", aposta.Gols1, aposta.Gols2, aposta.Jogo.Time1.Sigla, aposta.Jogo.Time2.Sigla);
                        break;
                    }

                }

            }

            return (new Valor(texto));

        }
        private IList<Aposta> GetApostasData(Participante participante, DateTime database)
        {

            ApostaRepository apostaRepository = new ApostaRepository();

            IList<Aposta> apostas = apostaRepository.GetByParticipanteAndDate(participante, database);

            return (apostas);

        }
        private Indicador GetIndicadorMelhorPosicaoDia(Participante p)
        {
            Indicador indic = new Indicador(CON_MELHOR_POSICAO_DIA, "Melhor Posição");

            int posicao = p.MelhorPosicaoDia == 0 ? p.PosicaoAtual : p.MelhorPosicaoDia;

            indic.Add(new Valor(posicao.ToString()));
            return (indic);
        }
        private Indicador GetIndicadorPosicaoTempoReal(Participante p)
        {
            Indicador indic = new Indicador(CON_POSICAO_TEMPO_REAL, "Posição em Tempo Real");
            indic.Add(new Valor(p.PosicaoOnline.ToString()));
            return (indic);
        }
        public bool TodosOsJogosFinalizados(DateTime dataBase)
        {

            JogoRepository jogoRepository = new JogoRepository();

            IList<Jogo> jogos = jogoRepository.GetJogo(dataBase.Date);


            foreach (Jogo jogo in jogos)
            {
                if (jogo.Gols1 == null && jogo.Gols2 == null)
                {
                    return (false);
                }
            }

            return (true);

        }
        public int GetSituacao(DateTime dataBase)
        {

            JogoRepository jogoRepository = new JogoRepository();

            IList<Jogo> jogos = jogoRepository.GetJogo(dataBase.Date);

            if (jogos.Count == 0)
            {
                return (CON_DIA_SEM_JOGO);
            }
            else
            {
                int situacaoJogo = 0;

                foreach (Jogo jogo in jogos)
                {
                    situacaoJogo = GetSituacaoJogo(jogo, dataBase);

                    if ((situacaoJogo == CON_DIA_COM_JOGO_DURANTE_O_JOGO) || (situacaoJogo == CON_DIA_COM_JOGO_ANTES_DO_JOGO))
                    {
                        break;
                    }
                }

                return (situacaoJogo);
            }
        }
        private int GetSituacaoJogo(Jogo jogo, DateTime dataBase)
        {


            int ret = 0;

            if (jogo.Dia < dataBase && jogo.Dia.AddMinutes(CON_NUM_MINUTOS_JOGO) > dataBase)
            {
                ret = CON_DIA_COM_JOGO_DURANTE_O_JOGO;
            }
            else
            {
                if (dataBase <= jogo.Dia)
                {
                    ret = CON_DIA_COM_JOGO_ANTES_DO_JOGO;
                }
                else
                {
                    ret = CON_DIA_COM_JOGO_APOS_O_JOGO;
                }
            }

            return (ret);

        }
        public Indicador GetProximoIndicador(string codIndicadorAtual, Participante participante)
        {
            DateTime dataBase = GetDataBase();

            switch (codIndicadorAtual)
            {
                case CON_NUM_PONTOS:
                    return (GetIndicadorPosicaoAnterior(participante));
                case CON_POSICAO_ANTERIOR:
                    return (GetIndicadorNumPontos(participante));
                case CON_POSICAO_TEMPO_REAL:
                    return (GetIndicadorPontosDasPrimeirasPosicoes());
                case CON_PONTOS_PRIMEIRAS_POSICOES:
                    return (GetIndicadorMelhorPosicaoDia(participante));
                case CON_MELHOR_POSICAO_DIA:
                    return (GetIndicadorPosicaoTempoReal(participante));
                default:
                    return (null);
            }

        }
        public Indicador GetProximoPlacar(Participante participante, string partidaAtual)
        {
            DateTime dataBase = GetDataBase();

            int situacao = GetSituacao(dataBase);
            bool alterarNome = false;

            if (situacao == CON_DIA_SEM_JOGO)
                dataBase = GetProxDataComJogo(dataBase.Date);

            if (situacao == CON_DIA_COM_JOGO_APOS_O_JOGO)
            {
                if (TodosOsJogosFinalizados(dataBase))
                {
                    dataBase = GetProxDataComJogo(dataBase);
                    alterarNome = true;
                }
            }

            Indicador placaresHoje = GetIndicadorPlacaresHoje(participante, dataBase, partidaAtual);

            if (situacao == CON_DIA_SEM_JOGO)
                alterarNome = true;

            if (alterarNome)
                placaresHoje.AlterarNome("Próximo Placar");

            return (placaresHoje);
        }
    }


    public interface IIndicador
    {
        string Nome { get; set; }
        string Cod { get; set; }
        ICollection<IValor> Valores { get; set; }
    }

    public interface IValor
    {
        string Val { get; set; }
    }

    public class Valor : IValor
    {
        public string Val { get; set; }

        public Valor(string val)
        {
            this.Val = val;
        }
    }

    public class Indicador : IIndicador
    {

        public string Nome { get; set; }
        public string Cod { get; set; }
        public ICollection<IValor> Valores { get; set; }

        public Indicador()
        {
        }

        public Indicador(string nome)
        {
            this.Nome = nome;
            Valores = new List<IValor>();
        }

        public Indicador(string cod, string nome)
        {
            this.Nome = nome;
            this.Cod = cod;
            Valores = new List<IValor>();
        }

        public void Add(IValor valor)
        {
            this.Valores.Add(valor);
        }

        public void AlterarNome(string novoNome)
        {
            this.Nome = novoNome;
        }

    }
}