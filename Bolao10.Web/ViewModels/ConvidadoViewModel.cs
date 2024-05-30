using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Bolao10.ViewModels
{
    public class ConvidadoViewModel
    {

        [Required]
        public Guid Convite { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public int Pais { get; set; }

        [Required]
        public int Estado { get; set; }

        [Required]
        public int Cidade { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Senha{ get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string ConfirmaSenha { get; set; }

        [Required]
        [Display(Description="Que seleção ganha a copa?")]
        public int TimeCampeao { get; set; }

    }
}