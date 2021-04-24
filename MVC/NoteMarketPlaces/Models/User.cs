//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NoteMarketPlaces.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            this.NoteDetails = new HashSet<NoteDetail>();
            this.UserDetails = new HashSet<UserDetail>();
            this.NoteRequests = new HashSet<NoteRequest>();
            this.NoteRequests1 = new HashSet<NoteRequest>();
            this.NoteReviews = new HashSet<NoteReview>();
            this.NoteReviews1 = new HashSet<NoteReview>();
            this.RejectedNotes = new HashSet<RejectedNote>();
            this.SpamReports = new HashSet<SpamReport>();
        }

        public int UserID { get; set; }

        [Required]
        [Display(Name = "first name")]
        [RegularExpression("^[a-zA-Z]*$", ErrorMessage = "Enter valid name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "last name")]
        [RegularExpression("^[a-zA-Z]*$", ErrorMessage = "Enter valid name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string EmailID { get; set; }

        [Required]
        [Display(Name = "password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*\d)(?=.*[^\da-z]).\S{5,23}$", ErrorMessage = "Password must be between 6 and 24 characters and contain one lowercase letter, one digit and one special character.")]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [Display(Name ="confirm password")]
        public string Password2 { get; set; }

        public bool IsEmailVerified { get; set; }
        public bool IsDetailsSubmitted { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public bool RememberMe { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NoteDetail> NoteDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserDetail> UserDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NoteRequest> NoteRequests { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NoteRequest> NoteRequests1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NoteReview> NoteReviews { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NoteReview> NoteReviews1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RejectedNote> RejectedNotes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SpamReport> SpamReports { get; set; }
    }
}
