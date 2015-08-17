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
        public int userID { get; set; }
        public int typeID { get; set; }
        [Required(ErrorMessage="email is required")]
        public string email { get; set; }
        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "Must not greater than 50 characters", MinimumLength = 1)]
        [DisplayName("First Name")]
        public string firstName { get; set; }
        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Must not greater than 50 characters", MinimumLength = 1)]
        public string lastName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [StringLength(50, ErrorMessage = "Must be between 5 and 50 characters", MinimumLength = 5)]
        public string password { get; set; }
        [Compare("password", ErrorMessage = "Passwords must match")]
        public string confirmPassword { get; set; }
        public string address { get; set; }
        [Required(ErrorMessage = "Gender is required")]
        public string gender { get; set; }
        public string phoneNumber { get; set; }
        public string dateOfBirth { get; set; }
        public string status { get; set; }
        public string dateJoined { get; set; }
        public string avatar { get; set; }
    }
}