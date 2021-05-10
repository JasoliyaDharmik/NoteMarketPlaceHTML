using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace NoteMarketPlaces.Models
{
    public class MemberDetail
    {
        public UserDetail UserDetail { get; set; }
        public IPagedList<NoteDetail> pagedList { get; set; }
        public List<NoteRequest> NoteRequest { get; set; }
    }
}