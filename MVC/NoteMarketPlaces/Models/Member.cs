using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlaces.Models
{
    public class Member
    {
        public int UserID { get; set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Nullable<DateTime> JoiningDate { get; set; }
        public int UnderReviewNotes { get; set; }
        public int PublishedNotes { get; set; }
        public int DownloadedNote { get; set; }
        public int TotalExpenses { get; set; }
        public int TotalEarnings { get; set; }
    }
}