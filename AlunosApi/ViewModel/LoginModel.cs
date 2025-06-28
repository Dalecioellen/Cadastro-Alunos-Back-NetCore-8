using System.ComponentModel.DataAnnotations;

namespace AlunosApi.ViewModel
{
    public class LoginModel
    {
        [Required (ErrorMessage = "Email é obrigatório")]
        [EmailAddress (ErrorMessage = "Formato incorreto")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(20, ErrorMessage = "Asenha deve ter no minimo tantos caracteres")]
        public string? Password { get; set; }


        }
}
