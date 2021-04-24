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
        public string Password1 { get; set; }

        [Required]
        [Display(Name = "new password")]
        public string Password2 { get; set; }

        [Required]
        [Display(Name = "confirm password")]
        [Compare("Password2")]
        public string Password3 { get; set; }
    }
}