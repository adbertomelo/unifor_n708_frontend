using System.ComponentModel.DataAnnotations;

namespace Bolao10.ViewModels
{
    public class LoginViewModel
    {

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

    }
}