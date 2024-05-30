using Bolao10.Model.Entities;
using Bolao10.Persistence.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bolao10.Services
{
    public class JogoService
    {

        private bool PlacarInvalido(int gols1, int gols2)
        {
            return ((gols1.ToString().Length > 2) || (gols2.ToString().Length > 2) || (gols1 < 0) || (gols2 < 0));
        }

        private bool ParticipanteValido(Participante participante)
        {
            return (participante.Tipo == "M");
        }
        public void Salvar(Participante participante, Jogo jogo, int gols1, int gols2)
        {

            if (PlacarInvalido(gols1, gols2))
            {
                throw new Exception("Erro: Placar inválido.");
            }

            if (!ParticipanteValido(participante))
            {
                throw new Exception("Erro: Você não tem permissão para salvar jogos.");
            }

            JogoRepository jogoRepository = new JogoRepository();

            jogo.Gols1 = gols1;

            jogo.Gols2 = gols2;

            jogo.Gols1RealTime = gols1;

            jogo.Gols2RealTime = gols2;

            jogoRepository.Update(jogo);


        }

        
    }
}