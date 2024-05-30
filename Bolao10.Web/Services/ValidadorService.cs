using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bolao10.ViewModels;

namespace Bolao10.Services
{
    public class ValidadorService
    {

        string _mensagemErro = "";

        public ValidadorService()
        {
            
        }

        public string MensagemErro 
        {
            get
            {
                return _mensagemErro;
            }

        }

        public bool ValidarNumeroDoCartao(Int64 numeroCartao)
        {
            if (numeroCartao == 0)
            {
                _mensagemErro = "Número do cartão inválido.";
                return false;
            }
                

            return true;
        }

        public bool ValidarValidade(string validade)
        {
            if (string.IsNullOrWhiteSpace(validade))
            {
                _mensagemErro = "Validade do cartão inválida.";
                return false;
            }
                

            if (validade.Length < 7)
            {
                _mensagemErro = "Validade do cartão inválida. Informe no formato MM/AAAA.";
                return false;
            }
                

            string mes = validade.Substring(0, 2);
            string ano = validade.Substring(3, 4);

            int outmes;
            if (!int.TryParse(mes, out outmes))
            {
                _mensagemErro = "Mês da validade do cartão inválido.";
                return false;
            }
                

            if (outmes < 1 || outmes > 12)
            {
                _mensagemErro = "Mês do cartão inválido. Informe um número entre 1 e 12.";
                return false;
            }
                

            int outano;
            if (!int.TryParse(ano, out outano))
            {
                _mensagemErro = "Ano da validade do cartão inválido.";
                return false;
            }
                

            int anoCorrente = DateTime.Today.Year;

            if (outano < anoCorrente)
            {
                _mensagemErro = "Ano de validade do cartão inferior ao ano atual.";
                return false;
            }
                

            return true;
        }

        public bool ValidarDataNascimento(string dataNascimento)
        {

            if(!ValidarSeExisteValor(dataNascimento))
            {
                _mensagemErro = "Data de nascimento inválida.";
                return false;
            }

            if (dataNascimento.Length < 10)
            {
                _mensagemErro = "Formato da data de nascimento inválida. Informe uma data no formato DD/MM/AAAA";
                return false;
            }

            string dia = dataNascimento.Substring(0, 2);
            string mes = dataNascimento.Substring(3, 2);
            string ano = dataNascimento.Substring(6, 4);

            if (ano.Trim().Length < 4)
            {
                _mensagemErro = "Ano de nascimento inválido. Informe o ano no formato AAAA";
                return false;
            }

            DateTime datNasc;
            if (!DateTime.TryParse(dataNascimento, out datNasc))
            {
                _mensagemErro = "Data de nascimento inválida.";
                return false;
            }
                
            if (datNasc >= DateTime.Today)
            {
                _mensagemErro = "Data de nascimento maior que hoje.";
                return false;
            }

            int idade = DateTime.Today.Year - Convert.ToInt16(ano);

            if (idade > 100)
            {
                _mensagemErro = "Informe uma data de nascimento válida.";
                return false;
            }


            return true;
        }

        public bool ValidarCelular(string nroCelular)
        {
            if (!ValidarSeExisteValor(nroCelular))
            {
                _mensagemErro = "Número do celular inválido.";
                return false;
            }

            if (nroCelular.Length < 12)
            {
                _mensagemErro = "Número do celular inválido.";
                return false;
            }

            
            int ddd = 0;
            if (!int.TryParse(nroCelular.Substring(1, 2), out ddd))
            {
                _mensagemErro = "DDD inválido.";
                return false;
            }

            if ("11,12,13,14,15,16,17,18,19,21,22,24,27,28,31,32,33,34,35,37,38,41,42,43,44,45,46,47,48,49,51,53,54,55,61,62,63,64,65,66,67,68,69,71,73,74,75,77,79,81,82,83,84,85,86,87,88,89,91,92,93,94,95,96,97,98,99".IndexOf(ddd.ToString()) < 0)
            {
                _mensagemErro = "DDD inválido";
                return false;
            }
                

            return true;
        }

        public bool ValidarCodigoSeguranca(int valor)
        {
            if (valor == 0)
            {
                _mensagemErro = "Código de segurança inválido.";
                return false;
            }
                

            return true;
        }

        public bool ValidarCPF(string valor)
        {
            if (!ValidarSeExisteValor(valor) || !NumeroDoCPFValido(valor))
            {
                _mensagemErro = "Número do CPF inválido";
                return false;
            }
                


            return true;
        }

        public bool ValidarCEP(string valor)
        {
            if (!ValidarSeExisteValor(valor))
            {
                _mensagemErro = "CEP inválido.";
                return false;
            }
                

            if (valor.Length != 9)
            {
                _mensagemErro = "CEP inválido.";
                return false;
            }
                

            Int32 cepBase = 0;
            if (!Int32.TryParse(valor.Substring(0, 5), out cepBase))
            {
                _mensagemErro = "CEP inválido.";
                return false;
            }
                

            Int32 cepDig = 0;
            if (!Int32.TryParse(valor.Substring(6, 3), out cepDig))
            {
                _mensagemErro = "CEP inválido.";
                return false;
            }
                

            return true;
        }

        public bool TemMaisDeUmNome(string valor)
        {
            string[] nomes = valor.Split(' ');
            
            return nomes.Where(x => x.Trim() != "").Count() > 1;
        }

        public bool ValidarNomeTitular(string valor)
        {
            if (!ValidarSeExisteValor(valor))
            {
                _mensagemErro = "Informe o nome impresso no cartão.";
                return false;
            }
                
            if (!TemMaisDeUmNome(valor))
            {
                _mensagemErro = "Informe nome e sobrenome.";
                return false;
            }

            return true;
        }

        public bool ValidarEndereco(string valor)
        {
            if (!ValidarSeExisteValor(valor))
            {
                _mensagemErro = "Informe o endereço da fatura.";
                return false;
            }


            return true;
        }

        public bool ValidarBairro(string valor)
        {
            if (!ValidarSeExisteValor(valor))
            {
                _mensagemErro = "Informe o bairro da fatura.";
                return false;
            }


            return true;
        }

        public bool ValidarSeExisteValor(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return false;
            return true;
        }

        public bool ehUmNumero(string valor)
        {
            int num = 0;
            if (!int.TryParse(valor, out num))
                return false;

            return true;
        }

        public static bool NumeroDoCPFValido(string CPF)
        {
            int[] mt1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mt2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string TempCPF;
            string Digito;
            int soma;
            int resto;

            CPF = CPF.Trim();
            CPF = CPF.Replace(".", "").Replace("-", "");

            if (CPF.Length != 11)
                return false;

            TempCPF = CPF.Substring(0, 9);
            soma = 0;
            for (int i = 0; i < 9; i++)
                soma += int.Parse(TempCPF[i].ToString()) * mt1[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            Digito = resto.ToString();
            TempCPF = TempCPF + Digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(TempCPF[i].ToString()) * mt2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            Digito = Digito + resto.ToString();

            return CPF.EndsWith(Digito);
        }

        private bool ValidarNumeroCasa(string numeroCasa)
        {
            if (!ValidarSeExisteValor(numeroCasa))
            {
                _mensagemErro = "Número do endereço inválido.";
                return false;
            }
                

            return true;
        }

        private bool ValidarCidade(string cidade)
        {
            if (!ValidarSeExisteValor(cidade))
            {
                _mensagemErro = "Cidade do endereço inválida.";
                return false;
            }


            return true;
        }

        private bool ValidarUF(string uf)
        {
            if (!ValidarSeExisteValor(uf))
            {
                _mensagemErro = "UF do endereço inválida.";
                return false;
            }

            if ("AC AL AM AP BA CE DF ES GO MA MG MS MT PA PB PE PI PR RJ RN RO RR RS SC SE SP TO".IndexOf(uf) < 0)
            {
                _mensagemErro = "UF do endereço inválida";
                return false;
            }

            return true;
        }

        public bool ValidarEndereco(PagadorModel infoPagador)
        {
            
            bool res = ValidarCEP(infoPagador.CEP);
            res = res && ValidarEndereco(infoPagador.Endereco);
            res = res && ValidarBairro(infoPagador.Bairro);
            res = res && ValidarNumeroCasa(infoPagador.NumeroCasa);
            res = res && ValidarCidade(infoPagador.Cidade);
            res = res && ValidarUF(infoPagador.UF);
            
            return res;

        }

        public bool ValidarCartao(CartaoCreditoModel infoCartaoCredito)
        {
            bool res = ValidarNumeroDoCartao(infoCartaoCredito.Numero);
            res = res && ValidarValidade(infoCartaoCredito.Validade);
            res = res && ValidarNomeTitular(infoCartaoCredito.NomeTitular);
            res = res && ValidarCodigoSeguranca(infoCartaoCredito.CodigoSeguranca);
            return res;
        }

        public bool ValidarPagadorBoleto(PagadorModel infoPagador)
        {
            bool res = ValidarCPF(infoPagador.CPF);
            res = res && ValidarCelular(infoPagador.Telefone);

            return res;
        }

        public bool ValidarEnderecoBoleto(PagadorModel infoPagador)
        {

            bool res = ValidarCEP(infoPagador.CEP);
            res = res && ValidarEndereco(infoPagador.Endereco);
            res = res && ValidarNumeroCasa(infoPagador.NumeroCasa);
            res = res && ValidarCidade(infoPagador.Cidade);
            res = res && ValidarUF(infoPagador.UF);

            return res;

        }

        public bool ValidarPagador(PagadorModel infoPagador)
        {
            bool res = ValidarCPF(infoPagador.CPF);
            res = res && ValidarDataNascimento(infoPagador.DataNascimento);
            res = res && ValidarCelular(infoPagador.Telefone);

            return res;
        }

        public bool Validar(CartaoCreditoModel infoCartaoCredito, PagadorModel infoPagador)
        {

            bool res = false;
            res = ValidarCartao(infoCartaoCredito);
            res = res && ValidarPagador(infoPagador);
            res = res && ValidarEndereco(infoPagador);

            return res;

        }



        public bool InfoBoleto(PagadorModel infoPagador)
        {
            bool res = false;
            res = ValidarPagadorBoleto(infoPagador);
            res = res && ValidarEnderecoBoleto(infoPagador);

            return res;

        }
    }
}