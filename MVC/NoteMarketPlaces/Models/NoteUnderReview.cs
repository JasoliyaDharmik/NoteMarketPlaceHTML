using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NoteMarketPlaces.Models;
using PagedList;

namespace NoteMarketPlaces.Models
{
    public class NoteUnderReview
    {
        public IPagedList<NoteDetail> pagedList { get; set; }
        public RejectedNote RejectedNote { get; set; }
    }
}