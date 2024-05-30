using Bolao10.Model.Entities;
using Bolao10.Persistence.Repository;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Bolao10.Services
{
    public class UsuarioService
    {

        AccountService _accountService;
        ApiService _apiService;
        public UsuarioService()
        {
            _accountService = new AccountService();
            _apiService = new ApiService();
        }

        public Usuario NovoUsuario(string nome, string email, string senha, int cidadeId)
        {

            Cidade cidade = new CidadeRepository().GetById(cidadeId);

            string senhaEncrypt = new EncryptionService().Encrypt(senha);

            Usuario novoUsuario = new Usuario(nome, email, cidade, senhaEncrypt);

            return novoUsuario;

        }

        public void ValidarUsuario(Usuario usuario)
        {

            string nome = usuario.Nome;
            string email = usuario.Email;
            Cidade cidade = usuario.Cidade;
            string senha = usuario.Senha;

            ValidarEmailUsuario(email);

            ValidarNomeUsuario(nome);

            ValidarCidade(cidade);

        }

        public void ValidarCidade(Cidade cidade)
        {
            if (cidade == null)
                throw new Exception("É necessário que você informe a sua cidade");

        }
        public void ValidarEmailUsuario(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("É necessário que você informe o seu e-mail.");

        }
        public void ValidarNomeUsuario(string nome)
        {

            if (string.IsNullOrWhiteSpace(nome))
                throw new Exception("É necessário que você informe o seu nome.");

            string[] nomes = nome.Split(' ');

            if (nomes.Where(x => !String.IsNullOrWhiteSpace(x)).Count() < 2)
                throw new Exception("É necessário que você informe nome e sobrenome.");


        }

        public void ValidarSenha(string senha, string confirmeSenha)
        {

            if (string.IsNullOrWhiteSpace(senha))
                throw new Exception("É preciso informar uma senha!");

            if ((senha == null) || (confirmeSenha == null))
                throw new Exception("Senha inválida!");

            if (senha != confirmeSenha)
                throw new Exception("As senhas digitadas não são iguais!");

            if (senha.Length < 6)
                throw new Exception("A senha tem que ter, no mínimo, 6(seis) caracteres!");

        }

        public Usuario FindByEmail(string email)
        {
            string url = $"{_apiService.GetUrlBase()}/Usuario/getbyemail?email={email}";
            
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountService.GetToken());

                var res = client.GetAsync(url);

                if (res.Result.EnsureSuccessStatusCode().StatusCode == HttpStatusCode.OK)
                {
                    return res.Result.Content.ReadFromJsonAsync<Usuario>().Result;
                }
                else
                    throw new Exception("Login inválido!");


            }
        }
    }
}