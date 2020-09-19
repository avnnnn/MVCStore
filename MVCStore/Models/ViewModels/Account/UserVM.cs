using MVCStore.Models.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVCStore.Models.ViewModels
{
    public class UserVM
    {
        public UserVM()
        {

        }
        public UserVM(UserDTO row)
        {
            Id = row.Id; 
            EmailAddress = row.EmailAddress;
            UserName = row.UserName;
            Password = row.Password;
        }
        
        public int Id { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        [DisplayName("Email ")]
        public string EmailAddress { get; set; }
        [Required]
        [DisplayName("User Name")]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [DisplayName("Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}