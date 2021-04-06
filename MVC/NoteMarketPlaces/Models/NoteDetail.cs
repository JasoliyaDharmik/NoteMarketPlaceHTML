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
    
    public partial class NoteDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NoteDetail()
        {
            this.NoteRequests = new HashSet<NoteRequest>();
            this.NoteReviews = new HashSet<NoteReview>();
            this.RejectedNotes = new HashSet<RejectedNote>();
            this.SpamReports = new HashSet<SpamReport>();
        }
    
        public int NoteID { get; set; }
        public int OwnerID { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string BookPicture { get; set; }
        public string UploadNote { get; set; }
        public string NoteType { get; set; }
        public Nullable<int> NumberOfPages { get; set; }
        public string NotesDescription { get; set; }
        public string InstitutionName { get; set; }
        public string Country { get; set; }
        public string Course { get; set; }
        public string CourseCode { get; set; }
        public string Professor { get; set; }
        public int SellPrice { get; set; }
        public string NotePreview { get; set; }
        public Nullable<int> NoteSize { get; set; }
        public Nullable<System.DateTime> PublishedDate { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public bool IsActive { get; set; }
    
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NoteRequest> NoteRequests { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NoteReview> NoteReviews { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RejectedNote> RejectedNotes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SpamReport> SpamReports { get; set; }
    }
}
