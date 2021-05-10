using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NoteMarketPlaces.Models;
using PagedList;

namespace NoteMarketPlaces.Models
{
    public class Download
    {
        public NoteReview noteReview { get; set; }
        public SpamReport spamReport { get; set; }
        public IPagedList<NoteRequest> pagedList {get; set; }
    }
}