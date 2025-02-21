using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Final.Models
{
    public class UserCreateModel
    {
        #nullable disable
        [Required(ErrorMessage = "First Name is required.")]
        [MaxLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last Name is required.")]
        [MaxLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "User Name is required.")]
        public string UserName { get; set; }
        
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        
        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }
        
        // Lockout properties
        [Display(Name = "Lockout Enabled")]
        public bool LockoutEnabled { get; set; }
        
        [Display(Name = "Lockout End Date")]
        public DateTimeOffset? LockoutEnd { get; set; }
        
        // Optional: Role management properties
        public List<string> SelectedRoles { get; set; } = new List<string>();
        public List<string> AllRoles { get; set; } = new List<string>();
    }
}
