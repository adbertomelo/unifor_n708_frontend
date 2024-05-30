using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Bolao10.ViewModels
{
    public class RecuperarSenhaViewModel
    {

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string UserName { get; set; }


    }
}