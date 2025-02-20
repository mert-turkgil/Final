using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Final.Identity
{
    public class User : IdentityUser
    {
        #nullable disable
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        
        [DataType(DataType.PhoneNumber)]
        [AllowNull]
        public string TelNumber { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }
    }
}
