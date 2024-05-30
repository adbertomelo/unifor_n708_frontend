using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Bolao10.ViewModels
{
    public class ParticipanteEditViewModel
    {

        [Required]
        public string Codigo { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        public int Pais { get; set; }

        [Required]
        public int Estado { get; set; }

        [Required]
        public int Cidade { get; set; }

    }
}