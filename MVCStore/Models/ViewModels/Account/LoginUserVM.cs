using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVCStore.Models.ViewModels.Account
{
    public class LoginUserVM
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [DisplayName("Remember me")]
        public bool RememberMe { get; set; }
    }
}