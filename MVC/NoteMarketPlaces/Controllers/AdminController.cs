using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using NoteMarketPlaces.Models;
using PagedList;
using PagedList.Mvc;

namespace NoteMarketPlaces.Controllers
{
    public class AdminController : Controller
    {
        static int AdminId;
        NoteMarketPlaceEntities db = new NoteMarketPlaceEntities();
        
        public ActionResult LogOut()
        {
            AdminId = 0;
            return RedirectToAction("Login","Home");
        }

        public ActionResult Login(string email, string password)
        {
            var admin = db.AdminDetails.SingleOrDefault(m => m.Email == email && m.Password == password);
            if (admin != null)
            {
                AdminId = admin.AdminID;
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult Dashboard(int? page, string search, string sortBy, string month, string NoteID, string Remark)
        {
            if(AdminId == 0)
            {
                return RedirectToAction("Login","Home");
            }

            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            if (!string.IsNullOrEmpty(NoteID) && !string.IsNullOrEmpty(Remark))
            {
                NoteDetail noteDetail = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(NoteID) && m.IsActive == true);
                noteDetail.ModifiedDate = DateTime.Now;
                noteDetail.Status = "removed";
                noteDetail.IsActive = false;
                noteDetail.SellFor = "aa";
                try
                {
                    var rejectednote = new RejectedNote();
                    rejectednote.UserID = noteDetail.OwnerID;
                    rejectednote.NoteID = int.Parse(NoteID);
                    rejectednote.Remark = Remark;
                    rejectednote.CreatedDate = DateTime.Now;
                    rejectednote.ModifiedBy = AdminId;
                    rejectednote.IsActive = true;
                    db.RejectedNotes.Add(rejectednote);

                    db.SaveChanges();

                    User user = db.Users.SingleOrDefault(m => m.UserID == noteDetail.OwnerID && m.IsActive == true);
                    MailMessage mail = new MailMessage("DJpatel0134@gmail.com", "jasoliyadharmik81@gmail.com");//,user.EmailID.ToString());               
                    mail.Subject = "Sorry! We need to remove your notes from our portal.";
                    string Body = "Hello " + user.FirstName + " " + user.LastName + ",<br><br>We want to inform you that, your note " + noteDetail.Title + " has been removed from the portal.<br>Please find our remarks as below -<br>" + Remark + "<br><br>Regards,<br>Notes Marketplace";
                    mail.Body = Body;
                    mail.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;

                    smtp.Credentials = new System.Net.NetworkCredential("DJpatel0134@gmail.com", "DJpatel0134#0123456");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                catch (Exception)
                {
                    return HttpNotFound();
                }
            }

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
                noteDetails = noteDetails.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.Category.Equals(search) || m.SellPrice.Equals(search) || m.Publisher.StartsWith(search) || m.PublishedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search)).ToList();
            }
            if(!string.IsNullOrEmpty(month))
            {
                if(month == "all")
                {
                    noteDetails = noteDetails.ToList();
                }
                else
                {
                    noteDetails = noteDetails.Where(m => m.PublishedDate.Value.ToString("Y").Equals(month)).ToList();
                }
             
            }
            else
            {
                noteDetails = noteDetails.Where(m => m.PublishedDate.Value.ToString("Y").Equals(DateTime.Now.ToString("Y"))).ToList();
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

            Admin_Dashboard admin_Dashboard = new Admin_Dashboard();
            admin_Dashboard.pagedList = noteDetails.ToPagedList(page ?? 1, 5);
           
            return View(admin_Dashboard);
        }

        public ActionResult NoteDetail(string noteId)
        {
            if(AdminId == 0)
            {
                return RedirectToAction("Login","Home");
            }
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

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

        public ActionResult NoteUnderReview(int? page, string search, string sortBy, string sellerName, string buttenValue,string noteId, RejectedNote rejectedNote)
        {
            if(AdminId == 0)
            {
                return RedirectToAction("Login","Home");
            }
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            if (!string.IsNullOrEmpty(buttenValue))
            {
                if(buttenValue == "approve")
                {
                    NoteDetail notes = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(noteId));
                    notes.Status = "published";
                    notes.PublishedDate = DateTime.Now;
                    notes.ModifiedBy = AdminId;
                    notes.SellFor = "aa";
                    db.SaveChanges();
                }
            }

            if(rejectedNote.NoteID != 0 && rejectedNote.Remark != null)
            {
                rejectedNote.UserID = AdminId;
                rejectedNote.CreatedDate = DateTime.Now;
                rejectedNote.ModifiedBy = AdminId;
                rejectedNote.IsActive = true;
                db.RejectedNotes.Add(rejectedNote);

                NoteDetail notes = db.NoteDetails.SingleOrDefault(m => m.NoteID == rejectedNote.NoteID);
                notes.Status = "rejected";
                notes.SellFor = "aa";
                notes.IsActive = false;
                db.SaveChanges();
            }
            List<NoteDetail> noteDetails = db.NoteDetails.Where(m => m.Status.Equals("in review") && m.IsActive == true).ToList();
            noteDetails = noteDetails.OrderByDescending(m => m.CreatedDate).ToList();
            foreach(var item in noteDetails)
            {
                User user = db.Users.SingleOrDefault(m => m.UserID == item.OwnerID);
                item.Publisher = user.FirstName + " " + user.LastName;
            }

            ViewBag.sellerName = noteDetails.Select(m => m.Publisher).Distinct();

            if (!string.IsNullOrEmpty(search))
            {
                noteDetails = noteDetails.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.Category.Equals(search) || m.Publisher.ToLower().StartsWith(search.ToLower()) || m.CreatedDate.ToString().StartsWith(search) || m.Status.ToLower().Equals(search.ToLower())).ToList();
            }
            if(!string.IsNullOrEmpty(sortBy))
            {
                if(sortBy == "title")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Title).ToList();
                }
                if(sortBy == "category")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Category).ToList();
                }
                else if(sortBy == "seller")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Publisher).ToList();
                }
                else if(sortBy == "date")
                {
                    noteDetails = noteDetails.OrderBy(m => m.CreatedDate).ToList();
                }
                else if(sortBy == "status")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Status).ToList();
                }
            }
            if(!string.IsNullOrEmpty(sellerName))
            {
                noteDetails = noteDetails.Where(m => m.Publisher == sellerName).ToList();
            }

            NoteUnderReview noteUnderReview = new NoteUnderReview();
            noteUnderReview.pagedList = noteDetails.ToPagedList(page ?? 1, 5);
            return View(noteUnderReview);
        }

        public ActionResult DownloadNote(int? page, string search, string sortBy, string note, string seller, string buyer)
        {
            if(AdminId == 0)
            {
                return RedirectToAction("Login","Home");
            }

            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            List<NoteRequest> noteRequests = db.NoteRequests.Where(m => m.Status == true).ToList();
            List<Admin_Download> noteList = new List<Admin_Download>();
            
            foreach(var item in noteRequests)
            {
                Admin_Download temp = new Admin_Download();
                var notedetail = db.NoteDetails.SingleOrDefault(m => m.NoteID == item.NoteID);
                temp.NoteID = item.NoteID; 
                temp.Title = notedetail.Title;
                temp.Category = notedetail.Category;
                temp.Price = notedetail.SellPrice;
                temp.Notes = notedetail.UploadNote;
                temp.SellType = notedetail.SellPrice == 0 ? "Free" : "Paid";
                temp.DownloadDate = item.ApprovedDate;

                
                var user1 = db.Users.SingleOrDefault(m => m.UserID == item.BuyerID);
                var user2 = db.Users.SingleOrDefault(m => m.UserID == item.SellerID);
                temp.Buyer = user1.FirstName + " " + user1.LastName;
                temp.Seller = user2.FirstName + " " + user2.LastName;

                noteList.Add(temp);
            }

            ViewBag.notes = noteList.Select(m => m.Title).Distinct();
            ViewBag.buyers = noteList.Select(m => m.Buyer).Distinct();
            ViewBag.sellers = noteList.Select(m => m.Seller).Distinct();

            noteList = noteList.OrderByDescending(m => m.DownloadDate).ToList();
            if(!string.IsNullOrEmpty(search))
            {
                noteList = noteList.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.Category.ToLower().Equals(search.ToLower()) || m.Buyer.ToLower().StartsWith(search.ToLower()) || m.Seller.ToLower().StartsWith(search.ToLower()) || m.SellType.ToLower().Equals(search.ToLower()) || m.Price.ToString().Equals(search) || m.DownloadDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search)).ToList();
            }
            if(!string.IsNullOrEmpty(note))
            {
                noteList = noteList.Where(m => m.Title.Equals(note)).ToList();
            }
            if (!string.IsNullOrEmpty(seller))
            {
                noteList = noteList.Where(m => m.Seller.Equals(seller)).ToList();
            }
            if (!string.IsNullOrEmpty(buyer))
            {
                noteList = noteList.Where(m => m.Buyer.Equals(buyer)).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                if(sortBy == "title")
                {
                    noteList = noteList.OrderBy(m => m.Title).ToList();
                }
                else if (sortBy == "category")
                {
                    noteList = noteList.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy == "buyer")
                {
                    noteList = noteList.OrderBy(m => m.Buyer).ToList();
                }
                else if (sortBy == "seller")
                {
                    noteList = noteList.OrderBy(m => m.Seller).ToList();
                }
                else if (sortBy == "price")
                {
                    noteList = noteList.OrderBy(m => m.Price).ToList();
                }
                else if (sortBy == "date")
                {
                    noteList = noteList.OrderBy(m => m.DownloadDate).ToList();
                }

            }
            return View(noteList.ToPagedList(page ?? 1, 5));
        }

        public ActionResult Member(int? page, string search, string sortBy, string UserId)
        {
            if(AdminId == 0)
            {
                return RedirectToAction("Login","Home");
            }
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            if (!string.IsNullOrEmpty(UserId))
            {
                User user = db.Users.SingleOrDefault(m => m.UserID.ToString().Equals(UserId));
                user.IsActive = false;
                user.Password2 = user.Password;

                List<NoteDetail> noteDetails = db.NoteDetails.Where(m => m.OwnerID.ToString().Equals(UserId)).ToList();
                foreach(var note in noteDetails)
                {
                    note.Status = "removed";
                    note.SellFor = "aa";
                    note.IsActive = false;
                }
                db.SaveChanges();
            }
            List<User> users = db.Users.Where(m => m.IsActive == true).ToList();
            List<Member> memberList = new List<Member>();
            foreach(var user in users)
            {
                Member member = new Member();
                member.UserID = user.UserID;
                member.FirstName = user.FirstName;
                member.LastName = user.LastName;
                member.Email = user.EmailID;
                member.JoiningDate = user.CreatedDate;

                member.UnderReviewNotes = db.NoteDetails.Where(m => m.OwnerID == user.UserID && m.Status == "in review" && m.IsActive == true).Count(); ;
                member.PublishedNotes = db.NoteDetails.Where(m => m.OwnerID == user.UserID && m.Status == "published" && m.IsActive == true).Count(); ;
                

                List<NoteRequest> noteRequests = db.NoteRequests.Where(m => m.SellerID == user.UserID && m.Status == true).ToList();
                int count1 = 0;
                foreach (var noteReq in noteRequests)
                {
                    if (noteReq != null)
                        count1 += db.NoteDetails.SingleOrDefault(m => m.OwnerID == user.UserID && m.NoteID == noteReq.NoteID).SellPrice;
                }

                List<NoteRequest> noteRequests2 = db.NoteRequests.Where(m => m.BuyerID == user.UserID && m.Status == true).ToList();
                int count2 = 0;
                foreach (var noteReq in noteRequests2)
                {
                    if (noteReq != null)
                        count2 += db.NoteDetails.SingleOrDefault(m => m.NoteID == noteReq.NoteID).SellPrice;
                }
                member.DownloadedNote = noteRequests2.Count();
                member.TotalEarnings = count1;
                member.TotalExpenses = count2;

                memberList.Add(member);
            }

            memberList = memberList.OrderBy(m => m.JoiningDate).ToList();

            if(!string.IsNullOrEmpty(search))
            {
                memberList = memberList.Where(m => m.FirstName.ToLower().Equals(search.ToLower()) || m.LastName.ToLower().Equals(search.ToLower()) || m.Email.ToLower().StartsWith(search.ToLower()) || m.JoiningDate.Value.ToString().StartsWith(search) || m.TotalExpenses.ToString().Equals(search) || m.TotalEarnings.ToString().Equals(search.ToLower())).ToList();
            }
            if(!string.IsNullOrWhiteSpace(sortBy))
            {
                if(sortBy == "firstName")
                {
                    memberList = memberList.OrderBy(m => m.FirstName).ToList();
                }
                else if (sortBy == "lastName")
                {
                    memberList = memberList.OrderBy(m => m.LastName).ToList();
                }
                else if (sortBy == "email")
                {
                    memberList = memberList.OrderBy(m => m.Email).ToList();
                }
                else if (sortBy == "date")
                {
                    memberList = memberList.OrderBy(m => m.JoiningDate).ToList();
                }
                else if (sortBy == "underReviewNote")
                {
                    memberList = memberList.OrderBy(m => m.UnderReviewNotes).ToList();
                }
                else if (sortBy == "publishedNote")
                {
                    memberList = memberList.OrderBy(m => m.PublishedNotes).ToList();
                }
                else if (sortBy == "downloadNote")
                {
                    memberList = memberList.OrderBy(m => m.DownloadedNote).ToList();
                }
                else if (sortBy == "expenses")
                {
                    memberList = memberList.OrderBy(m => m.TotalExpenses).ToList();
                }
                else if (sortBy == "earning")
                {
                    memberList = memberList.OrderBy(m => m.TotalEarnings).ToList();
                }
            }
            return View(memberList.ToPagedList(page ?? 1, 5));
        }
        
        public ActionResult MemberDetail(int? page, string UserId, string sortBy)
        {
            if(AdminId == 0)
            {
                return RedirectToAction("Login","Home");
            }

            if(string.IsNullOrEmpty(UserId))
            {
                return HttpNotFound();
            }
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            UserDetail userDetail = db.UserDetails.SingleOrDefault(m => m.UserID.ToString().Equals(UserId) && m.User.IsActive == true);
            if(userDetail == null)
            {
                return HttpNotFound();
            }
            List<NoteDetail> noteDetails = db.NoteDetails.Where(m => m.OwnerID.ToString().Equals(UserId) && m.Status != "draft" && m.IsActive == true).ToList();
            noteDetails = noteDetails.OrderBy(m => m.CreatedDate).ToList();

            if (!string.IsNullOrEmpty(sortBy))
            {
                if(sortBy == "title")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Title).ToList();
                }
                else if (sortBy == "category")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy == "status")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Status).ToList();
                }
                else if (sortBy == "date")
                {
                    noteDetails = noteDetails.OrderBy(m => m.CreatedDate).ToList();
                }
                else if (sortBy == "publishdate")
                {
                    noteDetails = noteDetails.OrderBy(m => m.PublishedDate).ToList();
                }
            }

            MemberDetail memberDetail = new MemberDetail();
            memberDetail.UserDetail = userDetail;
            memberDetail.pagedList = noteDetails.ToPagedList(page ?? 1,5);
            memberDetail.NoteRequest = db.NoteRequests.ToList();

            return View(memberDetail);
        }

        public ActionResult ChangePassword()
        {
            if (AdminId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePassword changePassword)
        {
            if (AdminId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            if (!ModelState.IsValid)
            {
                return View();
            }
            try
            {
                AdminDetail adminDetail = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId && m.Password.Equals(changePassword.Password1));
                if (adminDetail != null)
                {
                    adminDetail.Password = changePassword.Password2;
                    db.SaveChanges();
                    return RedirectToAction("Login", "Home");
                }
                else
                {
                    ModelState.AddModelError("Password1", "Your password not match!");
                    return View();
                }
            }catch(Exception)
            {
                return View();
            }
        }

        public ActionResult AdminProfile()
        {
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            AdminDetail adminDetail = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if(adminDetail == null)
            {
                adminDetail = new AdminDetail();
            }
            return View(adminDetail);
        }

        [HttpPost]
        public ActionResult AdminProfile(AdminDetail adminDetail)
        {
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            adminDetail.IsActive = true;
            if(adminDetail.File != null)
            {
                if (adminDetail.File.ContentLength > 10 * 1024 * 1024)
                {
                    ViewBag.fileMsg = "file size is morethan 10 MB.";
                    return View(adminDetail);
                }
                else
                {
                    string extension = Path.GetExtension(adminDetail.File.FileName);
                    string fileName = AdminId.ToString() + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                    string path = Path.Combine(Server.MapPath("~/Uploads/ProfilePicture/"), fileName);
                    adminDetail.File.SaveAs(path);
                    adminDetail.ProfilePicture = fileName;
                }
            }

            try
            {
                var admin = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
                if(admin == null)
                {
                    db.AdminDetails.Add(adminDetail);
                }
                else
                {
                    admin.FirstName = adminDetail.FirstName;
                    admin.LastName = adminDetail.LastName;
                    admin.SecondaryEmail = adminDetail.SecondaryEmail;
                    admin.PhoneNumber = adminDetail.PhoneNumber;
                    admin.ProfilePicture = adminDetail.ProfilePicture;
                    admin.ModifiedDate = DateTime.Now;
                }

                db.SaveChanges();
                return RedirectToAction("Dashboard","Admin");
                
            }catch(Exception)
            {
                return View(adminDetail);
            }
        }

        public ActionResult PublishedNote(int? page, string search, string sortBy, string seller, string NoteID, string Remark)
        {
            if(AdminId == 0)
            {
                return RedirectToAction("Login","Home");
            }

            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            if (!string.IsNullOrEmpty(NoteID) && !string.IsNullOrEmpty(Remark))
            {
                NoteDetail noteDetail = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(NoteID) && m.IsActive == true);
                noteDetail.ModifiedDate = DateTime.Now;
                noteDetail.ModifiedBy = AdminId;
                noteDetail.Status = "removed";
                noteDetail.IsActive = false;
                noteDetail.SellFor = "aa";
                try
                {
                    var rejectednote = new RejectedNote();
                    rejectednote.UserID = noteDetail.OwnerID;
                    rejectednote.NoteID = int.Parse(NoteID);
                    rejectednote.Remark = Remark;
                    rejectednote.CreatedDate = DateTime.Now;
                    rejectednote.ModifiedBy = AdminId;
                    rejectednote.IsActive = true;
                    db.RejectedNotes.Add(rejectednote);

                    db.SaveChanges();

                    User user = db.Users.SingleOrDefault(m => m.UserID == noteDetail.OwnerID && m.IsActive == true);
                    MailMessage mail = new MailMessage("DJpatel0134@gmail.com", "jasoliyadharmik81@gmail.com");//,user.EmailID.ToString());               
                    mail.Subject = "Sorry! We need to remove your notes from our portal.";
                    string Body = "Hello " + user.FirstName + " " + user.LastName + ",<br><br>We want to inform you that, your note " + noteDetail.Title + " has been removed from the portal.<br>Please find our remarks as below -<br>" + Remark + "<br><br>Regards,<br>Notes Marketplace";
                    mail.Body = Body;
                    mail.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;

                    smtp.Credentials = new System.Net.NetworkCredential("DJpatel0134@gmail.com", "DJpatel0134#0123456");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                catch (Exception)
                {
                    return HttpNotFound();
                }
            }

            List<NoteDetail> noteDetails = db.NoteDetails.Where(m => m.Status == "published" && m.IsActive == true).ToList();
            noteDetails = noteDetails.OrderByDescending(m => m.CreatedDate).ToList();
            foreach (var item in noteDetails)
            {
                User user = db.Users.SingleOrDefault(m => m.UserID == item.OwnerID);
                item.Publisher = user.FirstName + " " + user.LastName;
                var v_user = db.AdminDetails.SingleOrDefault(m => m.AdminID == item.ModifiedBy);
                item.ModifiedName = v_user.FirstName + " " + v_user.LastName;
                item.NumberOfDownload = db.NoteRequests.Where(m => m.NoteID == item.NoteID).Count();
            }

            ViewBag.sellerName = noteDetails.Select(m => m.Publisher).Distinct();

            if (!string.IsNullOrEmpty(search))
            {
                noteDetails = noteDetails.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.Category.Equals(search) || m.SellPrice.Equals(search) || m.Publisher.ToLower().StartsWith(search.ToLower()) || m.PublishedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search)).ToList();
            }
            if(!string.IsNullOrEmpty(seller))
            {
                noteDetails = noteDetails.Where(m => m.Publisher == seller).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "title")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Title).ToList();
                }
                else if (sortBy == "category")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy == "price")
                {
                    noteDetails = noteDetails.OrderBy(m => m.SellPrice).ToList();
                }
                else if (sortBy == "seller")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Publisher).ToList();
                }
                else if (sortBy == "date")
                {
                    noteDetails = noteDetails.OrderBy(m => m.PublishedDate).ToList();
                }
                
            }
            
            return View(noteDetails.ToPagedList(page ?? 1, 5));
        }

        public ActionResult RejectedNote(int? page, string search, string sortBy, string seller, string noteId)
        {
            if(AdminId == 0)
            {
                return RedirectToAction("Login","Home");
            }
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            if(!string.IsNullOrEmpty(noteId))
            {
                var note = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(noteId));
                note.Status = "published";
                note.ModifiedBy = AdminId;
                note.ModifiedDate = DateTime.Now;
                note.PublishedDate = DateTime.Now;
                note.IsActive = true;
                note.SellFor = "aa";
                db.SaveChanges();
            }

            List<NoteDetail> noteDetails = db.NoteDetails.Where(m => m.Status == "rejected" && m.IsActive == false).ToList();

            foreach (var item in noteDetails)
            {
                User user = db.Users.SingleOrDefault(m => m.UserID == item.OwnerID);
                item.Publisher = user.FirstName + " " + user.LastName;
                var v_user = db.AdminDetails.SingleOrDefault(m => m.AdminID == item.ModifiedBy);
                item.ModifiedName = v_user.FirstName + " " + v_user.LastName;
                item.Remark = db.RejectedNotes.SingleOrDefault(m => m.NoteID == item.NoteID && m.IsActive == true).Remark;
            }

            ViewBag.sellerName = noteDetails.Select(m => m.Publisher).Distinct();

            if (!string.IsNullOrEmpty(search))
            {
                noteDetails = noteDetails.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.Category.ToLower().Equals(search.ToLower()) || m.Publisher.ToLower().StartsWith(search.ToLower()) || m.ModifiedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search)).ToList();
            }
            if (!string.IsNullOrEmpty(seller))
            {
                noteDetails = noteDetails.Where(m => m.Publisher == seller).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "title")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Title).ToList();
                }
                else if (sortBy == "category")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy == "seller")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Publisher).ToList();
                }
                else if (sortBy == "date")
                {
                    noteDetails = noteDetails.OrderBy(m => m.ModifiedDate).ToList();
                }
                
            }
            return View(noteDetails.ToPagedList(page ?? 1, 5));
        }

        public ActionResult SpamReport(int? page, string search, string sortBy, string noteId, string userId)
        {
            if(AdminId == 0)
            {
                return RedirectToAction("Login","Home");
            }

            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            if(!string.IsNullOrEmpty(noteId) && !string.IsNullOrEmpty(userId))
            {
                var spamList = db.SpamReports.SingleOrDefault(m => m.NoteID.ToString().Equals(noteId) && m.UserID.ToString().Equals(userId));
                spamList.IsActive = false;
                db.SaveChanges();
            }

            List<SpamReport> spamReports = db.SpamReports.Where(m => m.IsActive == true).ToList();

            foreach(var item in spamReports)
            {
                item.Title = db.NoteDetails.SingleOrDefault(m => m.NoteID == item.NoteID).Title;
                item.Category = db.NoteDetails.SingleOrDefault(m => m.NoteID == item.NoteID).Category;
            }
           
            if (!string.IsNullOrEmpty(search))
            {
                spamReports = spamReports.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.Category.ToLower().Equals(search.ToLower()) || m.CreatedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search)).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "title")
                {
                    spamReports = spamReports.OrderBy(m => m.Title).ToList();
                }
                else if (sortBy == "category")
                {
                    spamReports = spamReports.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy == "reportedBy")
                {
                    spamReports = spamReports.OrderBy(m => m.User.FirstName).ToList();
                    spamReports = spamReports.OrderBy(m => m.User.LastName).ToList();
                }
                else if (sortBy == "date")
                {
                    spamReports = spamReports.OrderBy(m => m.CreatedDate).ToList();
                }

            }
            return View(spamReports.ToPagedList(page ?? 1, 5));
        }

        public ActionResult ManageCategory(int ? page, string search, string sortBy)
        {
            if (AdminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            List<Category> categories = new List<Category>();
            List<Category> category = db.Categories.OrderByDescending(m => m.CreatedDate).ToList();
            foreach(var temp in category)
            {
                var admin = db.AdminDetails.SingleOrDefault(m => m.AdminID == temp.CreatedBy);
                temp.AddedBy = admin.FirstName + " " + admin.LastName;
                categories.Add(temp);
            }

            if(!string.IsNullOrWhiteSpace(search))
            {
                categories = categories.Where(m => m.Categories.ToLower().Equals(search.ToLower()) || m.Description.ToLower().StartsWith(search.ToLower()) || m.CreatedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search) || m.AddedBy.ToLower().StartsWith(search.ToLower())).ToList();
            }
            if(!string.IsNullOrWhiteSpace(sortBy))
            {
                if(sortBy == "category")
                {
                    categories = categories.OrderBy(m => m.Categories).ToList();
                }
                else if (sortBy == "description")
                {
                    categories = categories.OrderBy(m => m.Description).ToList();
                }
                else if (sortBy == "date")
                {
                    categories = categories.OrderBy(m => m.CreatedDate).ToList();
                }
                else if (sortBy == "addedBy")
                {
                    categories = categories.OrderBy(m => m.AddedBy).ToList();
                }
            }

            return View(categories.ToPagedList(page ?? 1,5));
        }

        public ActionResult AddCategory()
        {
            if(AdminId == 0)
            {
                return RedirectToAction("Login","Admin");
            }
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;
            return View(new Category());
        }

        [HttpPost]
        public ActionResult AddCategory(Category category)
        {
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                Category temp = db.Categories.FirstOrDefault(m => m.Categories.ToLower().Equals(category.Categories.ToLower()));
                if(temp == null)
                {
                    category.CreatedBy = AdminId;
                    category.CreatedDate = DateTime.Now;
                    category.IsActive = true;
                    db.Categories.Add(category);
                }
                else
                {
                    temp.ModifiedBy = AdminId;
                    temp.ModifiedDate = DateTime.Now;
                }

                db.SaveChanges();
                return RedirectToAction("ManageCategory", "Admin");
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult EditCategory(String categoryId)
        {
            if(!string.IsNullOrEmpty(categoryId))
            {
                Category temp = db.Categories.FirstOrDefault(m => m.ID.ToString().Equals(categoryId));
                if (temp != null)
                {
                    return View("AddCategory", temp);
                }
            }
            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult EditCategory(Category category)
        {
            if(AdminId != 0)
            {
                if(category != null)
                {
                    Category temp = db.Categories.FirstOrDefault(m => m.ID == category.ID);
                    if(temp != null)
                    {
                        temp.Categories = category.Categories;
                        temp.Description = category.Description;
                        temp.ModifiedBy = AdminId;
                        temp.ModifiedDate = DateTime.Now;
                        temp.IsActive = true;
                    }
                    db.SaveChanges();
                    return RedirectToAction("ManageCategory","Admin");
                }
            }
            return HttpNotFound();
        }

        public ActionResult DeleteCategory(string categoryId)
        {
            if(AdminId == 0)
            {
                return RedirectToAction("Login","Admin");
            }
            if(!string.IsNullOrEmpty(categoryId))
            {
                Category temp = db.Categories.FirstOrDefault(m => m.ID.ToString().Equals(categoryId));
                if(temp != null)
                {
                    temp.IsActive = false;
                }
                db.SaveChanges();
                return RedirectToAction("ManageCategory","Admin");
            }
            return HttpNotFound();
        }

        public ActionResult ManageType(int? page, string search, string sortBy)
        {
            if (AdminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            List<Types> types = new List<Types>();
            List<Types> type = db.Types1.OrderByDescending(m => m.CreatedDate).ToList();
            foreach (var temp in type)
            {
                var admin = db.AdminDetails.SingleOrDefault(m => m.AdminID == temp.CreatedBy);
                temp.AddedBy = admin.FirstName + " " + admin.LastName;
                types.Add(temp);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                types = types.Where(m => m.TypeName.ToLower().Equals(search.ToLower()) || m.Description.ToLower().StartsWith(search.ToLower()) || m.CreatedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search) || m.AddedBy.ToLower().StartsWith(search.ToLower())).ToList();
            }
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy == "type")
                {
                    types = types.OrderBy(m => m.TypeName).ToList();
                }
                else if (sortBy == "description")
                {
                    types = types.OrderBy(m => m.Description).ToList();
                }
                else if (sortBy == "date")
                {
                    types = types.OrderBy(m => m.CreatedDate).ToList();
                }
                else if (sortBy == "addedBy")
                {
                    types = types.OrderBy(m => m.AddedBy).ToList();
                }
            }

            return View(types.ToPagedList(page ?? 1, 5));
        }

        public ActionResult AddType()
        {
            if (AdminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;
            return View(new Types());
        }

        [HttpPost]
        public ActionResult AddType(Types type)
        {
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            if (!ModelState.IsValid)
            {
                return View(type);
            }

            try
            {
                Types temp = db.Types1.FirstOrDefault(m => m.TypeName.ToLower().Equals(type.TypeName.ToLower()));
                if (temp == null)
                {
                    type.CreatedBy = AdminId;
                    type.CreatedDate = DateTime.Now;
                    type.IsActive = true;
                    db.Types1.Add(type);
                }
                else
                {
                    temp.ModifiedBy = AdminId;
                    temp.ModifiedDate = DateTime.Now;
                }

                db.SaveChanges();
                return RedirectToAction("ManageType", "Admin");
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult EditType(String typeId)
        {
            if (!string.IsNullOrEmpty(typeId))
            {
                Types temp = db.Types1.FirstOrDefault(m => m.ID.ToString().Equals(typeId));
                if (temp != null)
                {
                    return View("AddType", temp);
                }
            }
            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult EditType(Types type)
        {
            if (AdminId != 0)
            {
                if (type != null)
                {
                    Types temp = db.Types1.FirstOrDefault(m => m.ID == type.ID);
                    if (temp != null)
                    {
                        temp.TypeName = type.TypeName;
                        temp.Description = type.Description;
                        temp.ModifiedBy = AdminId;
                        temp.ModifiedDate = DateTime.Now;
                        temp.IsActive = true;
                    }
                    db.SaveChanges();
                    return RedirectToAction("ManageType", "Admin");
                }
            }
            return HttpNotFound();
        }

        public ActionResult DeleteType(string typeId)
        {
            if (AdminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            if (!string.IsNullOrEmpty(typeId))
            {
                Types temp = db.Types1.FirstOrDefault(m => m.ID.ToString().Equals(typeId));
                if (temp != null)
                {
                    temp.IsActive = false;
                }
                db.SaveChanges();
                return RedirectToAction("ManageType", "Admin");
            }
            return HttpNotFound();
        }

        public ActionResult ManageCountry(int? page, string search, string sortBy)
        {
            if (AdminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            List<Country> countries = new List<Country>();
            List<Country> country = db.Countries.OrderByDescending(m => m.CreatedDate).ToList();
            foreach (var temp in country)
            {
                var admin = db.AdminDetails.SingleOrDefault(m => m.AdminID == temp.CreatedBy);
                temp.AddedBy = admin.FirstName + " " + admin.LastName;
                countries.Add(temp);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                countries = countries.Where(m => m.CountryName.ToLower().Equals(search.ToLower()) || m.CountryCode.Equals(search) || m.CreatedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search) || m.AddedBy.ToLower().StartsWith(search.ToLower())).ToList();
            }
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy == "country")
                {
                    countries = countries.OrderBy(m => m.CountryName).ToList();
                }
                else if (sortBy == "countryCode")
                {
                    countries = countries.OrderBy(m => m.CountryCode).ToList();
                }
                else if (sortBy == "date")
                {
                    countries = countries.OrderBy(m => m.CreatedDate).ToList();
                }
                else if (sortBy == "addedBy")
                {
                    countries = countries.OrderBy(m => m.AddedBy).ToList();
                }
            }

            return View(countries.ToPagedList(page ?? 1, 5));
        }

        public ActionResult AddCountry()
        {
            if (AdminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;
            return View(new Country());
        }

        [HttpPost]
        public ActionResult AddCountry(Country country)
        {
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            if (!ModelState.IsValid)
            {
                return View(country);
            }

            try
            {
                Country temp = db.Countries.FirstOrDefault(m => m.CountryName.ToLower().Equals(country.CountryName.ToLower()));
                if (temp == null)
                {
                    country.CreatedBy = AdminId;
                    country.CreatedDate = DateTime.Now;
                    country.IsActive = true;
                    db.Countries.Add(country);
                }
                else
                {
                    temp.ModifiedBy = AdminId;
                    temp.ModifiedDate = DateTime.Now;
                }

                db.SaveChanges();
                return RedirectToAction("ManageCountry", "Admin");
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult EditCountry(String countryId)
        {
            if (!string.IsNullOrEmpty(countryId))
            {
                Country temp = db.Countries.FirstOrDefault(m => m.ID.ToString().Equals(countryId));
                if (temp != null)
                {
                    return View("AddCountry", temp);
                }
            }
            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult EditCountry(Country country)
        {
            if (AdminId != 0)
            {
                if (country != null)
                {
                    Country temp = db.Countries.FirstOrDefault(m => m.ID == country.ID);
                    if (temp != null)
                    {
                        temp.CountryName = country.CountryName;
                        temp.CountryCode = country.CountryCode;
                        temp.ModifiedBy = AdminId;
                        temp.ModifiedDate = DateTime.Now;
                        temp.IsActive = true;
                    }
                    db.SaveChanges();
                    return RedirectToAction("ManageCountry", "Admin");
                }
            }
            return HttpNotFound();
        }

        public ActionResult DeleteCountry(string countryId)
        {
            if (AdminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            if (!string.IsNullOrEmpty(countryId))
            {
                Country temp = db.Countries.FirstOrDefault(m => m.ID.ToString().Equals(countryId));
                if (temp != null)
                {
                    temp.IsActive = false;
                }
                db.SaveChanges();
                return RedirectToAction("ManageCountry", "Admin");
            }
            return HttpNotFound();
        }

        public ActionResult ManageAdministrator(int? page, string search, string sortBy, string ID)
        {
            if (AdminId != 3)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            if(!string.IsNullOrEmpty(ID))
            {
                var admin = db.AdminDetails.FirstOrDefault(m => m.AdminID.ToString().Equals(ID));
                if(admin != null)
                {
                    admin.IsActive = false;
                    db.SaveChanges();
                }
            }

            List<AdminDetail> adminDetails = db.AdminDetails.Where(m => m.Type == "Admin" && m.IsActive == true).OrderByDescending(m => m.ModifiedDate).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                adminDetails = adminDetails.Where(m => m.FirstName.ToLower().StartsWith(search.ToLower()) || m.LastName.ToLower().Equals(search.ToLower()) || m.Email.ToLower().StartsWith(search.ToLower()) || m.PhoneNumber.StartsWith(search) || m.CreatedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search)).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "firstName")
                {
                    adminDetails = adminDetails.OrderBy(m => m.FirstName).ToList();
                }
                else if (sortBy == "lastName")
                {
                    adminDetails = adminDetails.OrderBy(m => m.LastName).ToList();
                }
                else if (sortBy == "email")
                {
                    adminDetails = adminDetails.OrderBy(m => m.Email).ToList();
                }
                else if (sortBy == "phone")
                {
                    adminDetails = adminDetails.OrderBy(m => m.PhoneNumber).ToList();
                }
                else if (sortBy == "date")
                {
                    adminDetails = adminDetails.OrderBy(m => m.CreatedDate).ToList();
                }
            }
            return View(adminDetails.ToPagedList(page ?? 1, 5));
        }


        public ActionResult AddAdministrator(string ID)
        {
            if (AdminId != 3)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            AdminDetail adminDetail = new AdminDetail();
            if (!string.IsNullOrEmpty(ID))
            {
                adminDetail = db.AdminDetails.FirstOrDefault(m => m.AdminID.ToString().Equals(ID));
            }
            
            return View(adminDetail);
        }

        [HttpPost]
        public ActionResult AddAdministrator(AdminDetail adminDetail)
        {
            if(AdminId != 3)
            {
                return RedirectToAction("Dashboard","Admin");
            }
            if(!ModelState.IsValid)
            {
                return View(adminDetail);
            }

            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            var admin = db.AdminDetails.SingleOrDefault(m => m.AdminID == adminDetail.AdminID);
            if(admin == null)
            {
                adminDetail.Password = "123456";
                adminDetail.Type = "Admin";
                adminDetail.CreatedBy = AdminId;
                adminDetail.CreatedDate = DateTime.Now;
                adminDetail.IsActive = true;
                var temp = db.AdminDetails.FirstOrDefault(m => m.Email.ToLower().Equals(adminDetail.Email.ToLower()));
                if (temp != null)
                {
                    ModelState.AddModelError("Email", "This email is exist!");
                    return View(adminDetail);
                }
                db.AdminDetails.Add(adminDetail);
            }
            else
            {
                admin.FirstName = adminDetail.FirstName;
                admin.LastName = adminDetail.LastName;
                admin.PhoneNumber = adminDetail.PhoneNumber;
            }
            
            db.SaveChanges();

            return RedirectToAction("ManageAdministrator","Admin");
        }

        public ActionResult ManageSystemConfiguration()
        {
            if (AdminId != 3)
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            var AdminProfile = db.AdminDetails.SingleOrDefault(m => m.AdminID == AdminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.ProfilePicture;

            SystemConfiguration temp = db.SystemConfigurations.SingleOrDefault();
            return View(temp);
        }

        [HttpPost]
        public ActionResult ManageSystemConfiguration(SystemConfiguration systemConfiguration)
        {
            if(systemConfiguration.FileProfilePicture != null)
            {
                if (systemConfiguration.FileProfilePicture.ContentLength > 10 * 1024 * 1024)
                {
                    ViewBag.fileMsg = "file size is morethan 10 MB.";
                    return View(systemConfiguration);
                }
                else
                {
                    string fileName = "default.jpg";
                    string path = Path.Combine(Server.MapPath("~/Uploads/ProfilePicture/"), fileName);
                    systemConfiguration.FileProfilePicture.SaveAs(path);
                    systemConfiguration.DefaultProfilePicture = fileName;
                }
            }
            if(systemConfiguration.FileNotePicture != null)
            {
                if (systemConfiguration.FileNotePicture.ContentLength > 10 * 1024 * 1024)
                {
                    ViewBag.fileMsg = "file size is morethan 10 MB.";
                    return View(systemConfiguration);
                }
                else
                {
                    string fileName = "default.jpg";
                    string path = Path.Combine(Server.MapPath("~/Uploads/BookPicture/"), fileName);
                    systemConfiguration.FileNotePicture.SaveAs(path);
                    systemConfiguration.DefaultNotePreview = fileName;
                }
            }

            SystemConfiguration temp = db.SystemConfigurations.SingleOrDefault();

            if(temp != null)
            {
                temp.EmailID1 = systemConfiguration.EmailID1;
                temp.EmailID2 = systemConfiguration.EmailID2;
                temp.PhoneNumber = systemConfiguration.PhoneNumber;
                temp.FacebookURL = systemConfiguration.FacebookURL;
                temp.TwitterURL = systemConfiguration.TwitterURL;
                temp.LinkedInURL = systemConfiguration.LinkedInURL;
                temp.ModifiedDate = DateTime.Now;
                temp.ModifiedBy = AdminId;
            }
            else
            {
                systemConfiguration.CreatedDate = DateTime.Now;
                systemConfiguration.CreatedBy = AdminId;
                db.SystemConfigurations.Add(systemConfiguration);
            }
            db.SaveChanges();

            return RedirectToAction("Dashboard","Admin");
        }
    }
}