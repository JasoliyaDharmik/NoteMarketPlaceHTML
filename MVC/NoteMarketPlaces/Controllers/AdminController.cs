using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NoteMarketPlaces.Models;
using PagedList;
using PagedList.Mvc;

namespace NoteMarketPlaces.Controllers
{
    public class AdminController : Controller
    {
        static int AdminId = 1;
        NoteMarketPlaceEntities db = new NoteMarketPlaceEntities();
        
        public ActionResult LogOut()
        {
            AdminId = 0;
            return RedirectToAction("Login","Home");
        }
        public ActionResult Dashboard(string AdminID, int? page, string search, string sortBy, string month)
        {
            /*
            if (string.IsNullOrEmpty(AdminID))
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                AdminId = int.Parse(AdminID);
            }
            */
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            List<NoteDetail> noteDetails = new List<NoteDetail>();
            List<NoteDetail> notes = db.NoteDetails.Where(m => m.Status == "published" && m.IsActive == true).ToList();
            foreach (var item in notes)
            {
                NoteDetail note = new NoteDetail();
                note.NoteID = item.NoteID;
                note.Title = item.Title;
                note.Category = item.Category;
                note.UploadNote = item.UploadNote;
                note.NoteSize = item.NoteSize;
                note.SellPrice = item.SellPrice;
                note.PublishedDate = item.PublishedDate;
                
                User user = db.Users.SingleOrDefault(m => m.UserID == item.OwnerID);
                note.Publisher = user.FirstName + " " + user.LastName;

                note.NumberOfDownload = db.NoteRequests.Where(m => m.NoteID == item.NoteID && m.Status == true).Count();
                noteDetails.Add(note);
            }

            noteDetails = noteDetails.OrderByDescending(m => m.NumberOfDownload).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                noteDetails = noteDetails.Where(m => m.Title.ToLower().StartsWith(search) || m.Category.Equals(search) || m.SellPrice.Equals(search) || m.Publisher.StartsWith(search) || m.PublishedDate.Equals(search)).ToList();
            }
            if(!string.IsNullOrEmpty(month))
            {
                noteDetails = noteDetails.Where(m => m.PublishedDate.Value.ToString("Y").Equals(month)).ToList();
            }
            if(!string.IsNullOrEmpty(sortBy))
            {
                if(sortBy == "title")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Title).ToList();
                }
                else if(sortBy == "category")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Category).ToList();
                }
                else if(sortBy == "size")
                {
                    noteDetails = noteDetails.OrderBy(m => m.NoteSize).ToList();
                }
                else if(sortBy == "price")
                {
                    noteDetails = noteDetails.OrderBy(m => m.SellPrice).ToList();
                }
                else if(sortBy == "publisher")
                {
                   noteDetails = noteDetails.OrderBy(m => m.Publisher).ToList();
                }
                else if(sortBy == "date")
                {
                    noteDetails = noteDetails.OrderBy(m => m.PublishedDate).ToList();
                }
                else if(sortBy == "noOfdownload")
                {
                   noteDetails = noteDetails.OrderBy(m => m.NumberOfDownload).ToList();
                }
            }

            var dates = DateTime.Now.Date.AddDays(-7);
            ViewBag.InReview = db.NoteDetails.Where(m => m.Status == "in review" && m.IsActive == true).Count();
            ViewBag.Download = db.NoteRequests.Where(m => m.Status == true).Count();
            ViewBag.NewRegistration = db.Users.Where(m => m.CreatedDate >= dates && m.IsActive == true).Count();
            return View(noteDetails.ToPagedList(page ?? 1, 5));
        }

        public ActionResult NoteDetail(string noteId)
        {
            if(AdminId == 0)
            {
                return RedirectToAction("Login","Home");
            }
            NoteDetail notedetail = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(noteId));
            int avg = 0, count = 0;
            var abc = db.NoteReviews.Where(m => m.NoteID == notedetail.NoteID && m.IsActive == true).ToList();
            if (abc != null)
            {
                count = abc.Count();
                if (count != 0)
                {
                    avg = abc.ToList().Sum(m => m.Rating) / count;
                }
            }

            ViewBag.Review = count;
            ViewBag.Rating = avg;
            ViewBag.Spam = db.SpamReports.Where(m => m.NoteID == notedetail.NoteID).Count();

            List<NoteReview> noteReviews = db.NoteReviews.Where(m => m.NoteID.ToString().Equals(noteId) && m.IsActive == true).ToList();
            List<NoteReview> customerReview = new List<NoteReview>();
            foreach (var temp in noteReviews)
            {
                NoteReview noteReview = new NoteReview();
                User v_user = db.Users.SingleOrDefault(m => m.UserID == temp.UserID);
                UserDetail userdetail = db.UserDetails.SingleOrDefault(m => m.UserID == temp.UserID);
                if (userdetail.ProfilePicture != null)
                {
                    noteReview.UserProfile = userdetail.ProfilePicture;
                }
                else
                {
                    noteReview.UserProfile = "default.jpg";
                }

                noteReview.NoteID = temp.NoteID;
                noteReview.UserID = temp.UserID;
                noteReview.Rating = temp.Rating;
                noteReview.Comments = temp.Comments;
                noteReview.FullName = v_user.FirstName + " " + v_user.LastName;

                customerReview.Add(noteReview);
            }
            ViewBag.Customer = customerReview.OrderByDescending(m => m.Rating);

            return View(notedetail);
        }
 
        public ActionResult DeleteReview(string noteId, string userId)
        {
            if(noteId != null && userId != null)
            {
                NoteReview noteReview = db.NoteReviews.SingleOrDefault(m => m.UserID.ToString().Equals(userId) && m.NoteID.ToString().Equals(noteId));
                noteReview.IsActive = false;
                db.SaveChanges();
                return RedirectToAction("NoteDetail", "Admin", new { noteId = noteId });
            }
            return HttpNotFound();
        }
    }
}