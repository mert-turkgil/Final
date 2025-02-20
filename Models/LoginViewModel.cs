
using System.ComponentModel.DataAnnotations;

namespace Final.Models
{
    public class LoginViewModel
    {
        #nullable disable

        [Required(ErrorMessage ="User Name'i hatırlamıyorsanız yetkili biriyle iletişime geçiniz")]
        [Display(Name = "User Name")]
        [StringLength(70,MinimumLength =3,ErrorMessage ="İsim alanı 3-70 karakter arası olmalıdır ")]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}