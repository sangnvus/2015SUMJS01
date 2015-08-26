using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Models
{
    public class User
    {
        public int UserId { get; set; }
        public int TypeId { get; set; }
        [Required(ErrorMessage="email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "Must not greater than 50 characters", MinimumLength = 1)]
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Must not greater than 50 characters", MinimumLength = 1)]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [StringLength(50, ErrorMessage = "Must be between 5 and 50 characters", MinimumLength = 5)]
        public string Password { get; set; }
        [Compare("password", ErrorMessage = "Passwords must match")]
        public string ConfirmPassword { get; set; }
        public string Address { get; set; }
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string DateOfBirth { get; set; }
        public string Status { get; set; }
        public string DateJoined { get; set; }
        public string Avatar { get; set; }
    }
}