using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NoteMarketPlaces.Models
{
    public class ChangePassword
    {
        [Required]
        [MinLength(6, ErrorMessage = "Requird Minimum length 6")]
        public string Password1 { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Requird Minimum length 6")]
        [Display(Name = "new password")]
        public string Password2 { get; set; }

        [Required]
        [Compare("Password2")]
        [Display(Name = "confirm password")]
        public string Password3 { get; set; }
    }
}