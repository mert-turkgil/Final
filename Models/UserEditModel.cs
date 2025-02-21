using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Final.Models
{
    public class UserEditModel
    {
        #nullable disable
        public string UserId { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [MaxLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is required.")]
        [MaxLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }
        [Required]
        public string UserName { get; set; }
        public string Password { get; set; }
        [Required]
        public bool EmailConfirmed { get; set; }
        [Required]
        public bool Lockout  { get; set; }
        public List<string> SelectedRoles { get; set; } 
        public List<string> AllRoles { get; set; }
        

    }
}