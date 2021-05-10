using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlaces.Models
{
    public class Admin_Download
    {
        public int NoteID { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Buyer { get; set; }
        public string Seller { get; set; }
        public string SellType { get; set; }
        public int Price { get; set; }
        public Nullable<DateTime> DownloadDate { get; set; }
        public string Notes { get; set; }
    }
}