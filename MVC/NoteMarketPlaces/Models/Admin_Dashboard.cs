using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using PagedList;

namespace NoteMarketPlaces.Models
{
    public class Admin_Dashboard
    {
        public IPagedList<NoteDetail> pagedList { get; set; }

        public string ApprovedBy { get; set; }
    }
}