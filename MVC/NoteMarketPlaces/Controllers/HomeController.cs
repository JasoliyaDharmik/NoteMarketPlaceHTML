using NoteMarketPlaces.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace NoteMarketPlaces.Controllers
{
    
    public class HomeController : Controller
    {
        static int userId = 0;
        NoteMarketPlaceEntities db = new NoteMarketPlaceEntities();

       // private ActionResult EmailSend(string body)
       // {

       // }

        public ActionResult Home()
        {
            ViewBag.isRegister = false;
            ViewBag.isHomePage = true;
            if (userId != 0)
            {
                ViewBag.isRegister = true;
                var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;
            }
            return View();
        }
        public ActionResult SignUp()
        {
            ViewBag.isCreated = false;
            return View();
        }


        [HttpPost]
        public ActionResult SignUp(User user)
        {
            ViewBag.iscreated = false;
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (db.Users.Any(m => m.EmailID == user.EmailID))
            {
                ModelState.AddModelError("EmailID", "This Email Address is exist!");
                return View(user);
            }
            else
            {
                user.IsActive = true;
                user.IsDetailsSubmitted = false;
                user.IsEmailVerified = false;
                user.CreatedDate = DateTime.Now.Date;
                
                db.Users.Add(user);

                try
                {
                    db.SaveChanges();

                    MailMessage mail = new MailMessage("DJpatel0134@gmail.com", "jasoliyadharmik81@gmail.com");//,user.EmailID.ToString());               
                    mail.Subject = "Note MarketPlace - Email Verification";
                    string Body = "Hello " + user.FirstName + " " + user.LastName + ",<br><br>Thank you for signing up with us.Please click on below link to verify your email address and to do login.<br>https://localhost:44365/Home/EmailVerification?emailId=" + user.EmailID + "<br><br>Regards,<br>Notes Marketplace";
                    mail.Body = Body;
                    mail.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;

                    smtp.Credentials = new System.Net.NetworkCredential("DJpatel0134@gmail.com", "DJpatel0134#0123456");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);

                    ViewBag.isCreated = true;
                    return View();
                }
                catch (Exception)
                {
                    return View();
                }
            }
        }

        public ActionResult EmailVerification(string emailId)
        {
            try
            {
                User v_user = db.Users.SingleOrDefault(m => m.EmailID.Equals(emailId.ToString()));
                if (v_user == null)
                    return HttpNotFound();
                else
                    return View(v_user);
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult EmailVerification(int id)
        {
            User v_user = db.Users.SingleOrDefault(m => m.UserID == id);
            try
            {
                if (v_user != null)
                {
                    v_user.FirstName = v_user.FirstName;
                    v_user.LastName = v_user.LastName;
                    v_user.Password2 = v_user.Password;
                    v_user.IsDetailsSubmitted = false;
                    v_user.IsEmailVerified = true;
                    v_user.IsActive = true;
                    db.SaveChanges();
                    return RedirectToAction("Login", "Home");
                }
                else
                    return HttpNotFound();

            }
            catch (Exception)
            {
                return View(v_user);
            }
        }

        public ActionResult Login()
        {
            HttpCookie cookie = Request.Cookies["RememberMe"];
            User v_user = new User();
            if (cookie != null)
            {
                v_user.EmailID = cookie["emailId"];
                v_user.Password = cookie["password"];
                if (cookie["emailId"].Length != 0)
                {
                    v_user.RememberMe = true;
                }
                else
                {
                    v_user.RememberMe = false;
                }
            }
            return View(v_user);
        }

        [HttpPost]
        public ActionResult Login(User user)
        {
            HttpCookie cookie = new HttpCookie("RememberMe");
            if (user.RememberMe == true)
            {
                cookie["emailId"] = user.EmailID;
                cookie["password"] = user.Password;
                cookie.Expires = DateTime.Now.AddMonths(3);
                Response.Cookies.Add(cookie);
            }
            else
            {
                cookie["emailId"] = null;
                cookie["password"] = null;
                Response.Cookies.Add(cookie);
            }
            
            
            User auth = db.Users.Where(m => m.EmailID.Equals(user.EmailID) && m.Password.Equals(user.Password)).SingleOrDefault();
            if (auth != null)
            {
                if (auth.IsActive == true)
                {
                    if (auth.IsEmailVerified == true)
                    {
                        userId = auth.UserID;
                        if (auth.IsDetailsSubmitted == true)
                        {
                            return RedirectToAction("SearchNote", "Home");
                        }
                        else
                        {
                            return RedirectToAction("UserProfile", "Home");
                        }
                    }
                    else
                    {
                        ViewBag.auth_msg = "Please verify your Email Address!";
                        return View(user);
                    }
                }
                else
                {
                    ViewBag.auth_msg = "You are not a Member!";
                }
            }else 
            {
                var admin =  db.AdminDetails.SingleOrDefault(m => m.Email.Equals(user.EmailID) && m.Password.Equals(user.Password));
                if(admin != null)
                {
                    return RedirectToAction("Dashboard","Admin", new { AdminID = admin.AdminID.ToString()});
                }
                else
                {
                    ViewBag.auth_msg = "Enter valid username or password!";
                }
            }         
            return View(user);
        }

        public ActionResult ForgotPassword()
        {
            ViewBag.isAuth = false;
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(User user)
        {
            User v_user = db.Users.SingleOrDefault(m => m.EmailID.Equals(user.EmailID.ToString()));
            if (v_user != null)
            {
                try
                {
                    MailMessage mail = new MailMessage("DJpatel0134@gmail.com", "jasoliyadharmik81@gmail.com");//user.EmailID.ToString());
                    mail.Subject = "New Temporary Password has been created for you";
                    string password = Membership.GeneratePassword(10, 1);
                    string Body = "Hello " + v_user.FirstName + " " + v_user.LastName + ",<br><br>We have generated a new password for you.<br>Password:" + password + "<br><br>Regards,<br>Notes Marketplace";
                    mail.Body = Body;
                    mail.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;

                    smtp.Credentials = new System.Net.NetworkCredential("DJpatel0134@gmail.com", "DJpatel0134#0123456");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);

                    v_user.Password = password;
                    db.SaveChanges();
                    ViewBag.isAuth = true;
                    ViewBag.msg = "Your password has been changed successfully and newly generated password is sent on your registered email address.";
                    return RedirectToAction("Login", "Home");

                }
                catch (Exception)
                {
                    return HttpNotFound();
                }
            }
            else
            {
                ViewBag.isAuth = false;
                ViewBag.msg = "This email address does not exist!";
                return View();
            }
        }

        public ActionResult FAQ()
        {
            ViewBag.isRegister = false;
            if (userId != 0)
            {
                ViewBag.isRegister = true;
                var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;
            }
            return View();
        }


        public ActionResult ContactUs()
        {
            ViewBag.IsEmailReadOnly = false;
            ViewBag.IsRegister = false;
            if (userId != 0)
            {
                ViewBag.ISRegister = true;
                User v_user = db.Users.SingleOrDefault(m => m.UserID == userId);
                ContactU contact = new ContactU();
                contact.FullName = v_user.FirstName + " " + v_user.LastName;
                contact.EmailAddress = v_user.EmailID;
                ViewBag.IsEmailReadOnly = true;
                var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;

                return View(contact);

            }
            return View();
        }

        [HttpPost]
        public ActionResult ContactUs(ContactU contact)
        {
            ViewBag.IsRegister = false;
            if(userId != 0)
            {
                ViewBag.IsRegister = true;
                var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;
                ViewBag.IsEmailReadOnly = true;
            }
            try
            {
                MailMessage mail = new MailMessage("DJpatel0134@gmail.com", "jasoliyadharmik81@gmail.com");
                mail.Subject = contact.FullName + " - " + contact.Subject;
                string Body = "Hello,<br><br>" + contact.Comments + "<br><br>Regards,<br>" + contact.FullName;
                mail.Body = Body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("DJpatel0134@gmail.com", "DJpatel0134#0123456");
                smtp.EnableSsl = true;
                smtp.Send(mail);

                return View();
            }
            catch (Exception)
            {
                return View();
            }

        }

        public ActionResult SearchNote(int? page, string search, string type, string category, string university, string course, string country, string rating)
        {
            ViewBag.IsRegister = false;
            if (userId != 0)
            {
                ViewBag.IsRegister = true;
                var userprofile = db.UserDetails.FirstOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;
            }

            List<NoteDetail> notedetail = db.NoteDetails.Where(m => m.IsActive == true && m.Status == "published").ToList();
            if (!string.IsNullOrEmpty(search))
            {
                notedetail = notedetail.Where(m => m.Title.ToLower().StartsWith(search.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(type))
            {
                notedetail = notedetail.Where(m => m.NoteType != null).ToList();
                notedetail = notedetail.Where(m => m.NoteType.ToLower().Equals(type)).ToList();
            }
            if (!string.IsNullOrEmpty(category))
            {
                notedetail = notedetail.Where(m => m.Category != null).ToList();
                notedetail = notedetail.Where(m => m.Category.ToLower().Equals(category.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(university))
            {
                notedetail = notedetail.Where(m => m.InstitutionName != null).ToList();
                notedetail = notedetail.Where(m => m.InstitutionName.ToLower().Equals(university.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(course))
            {
                notedetail = notedetail.Where(m => m.Course != null).ToList();
                notedetail = notedetail.Where(m => m.Course.ToLower().Equals(course.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(country))
            {
                notedetail = notedetail.Where(m => m.Country != null).ToList();
                notedetail = notedetail.Where(m => m.Country.ToLower().Equals(country.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(rating))
            {
                List<NoteDetail> newList = new List<NoteDetail>();
                foreach (var notes in notedetail)
                {
                    int avg = 0;
                    var abc = db.NoteReviews.Where(m => m.NoteID == notes.NoteID);
                    if (abc != null)
                    {
                        var count = abc.ToList().Count();
                        if (count != 0)
                        {
                            avg = abc.ToList().Sum(m => m.Rating) / count;
                        }
                    }
                    if (avg >= int.Parse(rating))
                    {
                        newList.Add(notes);
                    }
                }
                notedetail = newList;
            }

            ViewBag.noteCount = notedetail.Count();

            return View(notedetail.ToPagedList(page ?? 1, 9));
        }

        public ActionResult UserProfile()
        {
            UserDetail userdetail = new UserDetail();
            if (userId != 0)
            {
                var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;

                User v_user = db.Users.SingleOrDefault(m => m.UserID == userId);
                userdetail = db.UserDetails.FirstOrDefault(m => m.UserID == userId);
                if (userdetail != null)
                {
                    userdetail.CountryCode = userdetail.PhoneNumber.Substring(1,2);
                    userdetail.PhoneNumber = userdetail.PhoneNumber.Substring(3);
                }
                else
                {
                    userdetail = new UserDetail();
                }
                userdetail.User = v_user;
                List<Country> country = db.Countries.ToList();
                ViewBag.country = country;

                return View(userdetail);
            }
            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        public ActionResult UserProfile(UserDetail userdetail)
        {
            List<Country> country = db.Countries.ToList();
            ViewBag.country = country;
            var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
            if (userprofile != null)
                ViewBag.profilePicture = userprofile.ProfilePicture;

            try
            {
                User v_user = db.Users.FirstOrDefault(m => m.UserID == userId);
                v_user.FirstName = userdetail.User.FirstName;
                v_user.LastName = userdetail.User.LastName;
                v_user.Password2 = v_user.Password;
                v_user.IsDetailsSubmitted = true;
                v_user.IsActive = true;

                userdetail.UserID = userId;
                userdetail.PhoneNumber = userdetail.CountryCode + userdetail.PhoneNumber;
                userdetail.ModifiedDate = DateTime.Now.Date;
                userdetail.IsActive = true;
                userdetail.User = v_user;

                if (userdetail.File != null)
                {
                    if (userdetail.File.ContentLength > 10 * 1024 * 1024)
                    {
                        ViewBag.fileMsg = "file size is morethan 10 MB.";
                        return View(userdetail);
                    }
                    else
                    {
                        string extension = Path.GetExtension(userdetail.File.FileName);
                        string fileName = userId.ToString() + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                        string path = Path.Combine(Server.MapPath("~/Uploads/ProfilePicture/"), fileName);
                        userdetail.File.SaveAs(path);
                        userdetail.ProfilePicture = fileName;
                    }
                }

                var userExist = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
                if (userExist == null)
                    db.UserDetails.Add(userdetail);
                else
                {
                    UserDetail v_userdetail = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
                    v_userdetail.DOB = userdetail.DOB;
                    v_userdetail.Gender = userdetail.Gender;
                    v_userdetail.Address1 = userdetail.Address1;
                    v_userdetail.Address2 = userdetail.Address2;
                    v_userdetail.City = userdetail.City;
                    v_userdetail.State = userdetail.State;
                    v_userdetail.ZipCode = userdetail.ZipCode;
                    v_userdetail.Country = userdetail.Country;
                    v_userdetail.CountryCode = userdetail.CountryCode;
                    v_userdetail.PhoneNumber = userdetail.PhoneNumber;
                    v_userdetail.College = userdetail.College;
                    v_userdetail.University = userdetail.University;
                    v_userdetail.ModifiedDate = DateTime.Now;
                    v_userdetail.IsActive = true;
                    v_userdetail.UserID = userdetail.UserID;
                    v_userdetail.User = userdetail.User;
                    if (userdetail.File != null)
                    {
                        v_userdetail.ProfilePicture = userdetail.ProfilePicture;
                    }
                    else
                    {
                        v_userdetail.ProfilePicture = userExist.ProfilePicture;
                    }
                }

                db.SaveChanges();
                return RedirectToAction("SearchNote", "Home");
           }
            catch (Exception)
            {
                return View(userdetail);
            }
        }

        public ActionResult LogOut()
        {
            userId = 0;
            return RedirectToAction("Login", "Home");
        }

        public ActionResult BuyerRequest(int? page, string search, string sortBy, string NoteId)
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
            if (userprofile != null)
                ViewBag.profilePicture = userprofile.ProfilePicture;

            if(!string.IsNullOrEmpty(NoteId))
            {
                NoteRequest noteRequest = db.NoteRequests.SingleOrDefault(m => m.NoteID.ToString().Equals(NoteId));
                if(noteRequest != null)
                {
                    noteRequest.Status = true;
                    db.SaveChanges();
                    try
                    {
                        User buyer = db.Users.SingleOrDefault(m => m.UserID == userId);
                        User seller = db.Users.SingleOrDefault(m => m.UserID == noteRequest.SellerID);
                        MailMessage mail = new MailMessage("DJpatel0134@gmail.com", "jasoliyadharmik81@gmail.com");//,user.EmailID.ToString());               
                        mail.Subject = seller.FirstName + " " + seller.LastName + " Allows you to download a note";
                        string Body = "Hello " + buyer.FirstName+ " " + buyer.LastName +",<br>We would like to inform you that,"+ seller.FirstName + " " + seller.LastName + " Allows you to download a note.<br>Please login and see My Download tabs to download particular note.<br><br>Regards,<br>Notes Marketplace";

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
                    catch(Exception)
                    {
                        return HttpNotFound();
                    }
                }
            }

            List<NoteRequest> noteRequests = new List<NoteRequest>();
            List<NoteRequest> noteRequests1 = db.NoteRequests.Where(m => m.SellerID == userId && m.Status == false).ToList();

            foreach (var notes in noteRequests1)
            {
                var noteRequestsObj = new NoteRequest();
                NoteDetail noteDetails = db.NoteDetails.SingleOrDefault(m => m.NoteID == notes.NoteID);
                noteRequestsObj.NoteID = noteDetails.NoteID;
                noteRequestsObj.NoteTitle = noteDetails.Title;
                noteRequestsObj.Category = noteDetails.Category;
                noteRequestsObj.Price = noteDetails.SellPrice;
                noteRequestsObj.ApprovedDate = notes.ApprovedDate;
                noteRequestsObj.BuyerEmailID = db.Users.SingleOrDefault(m => m.UserID == notes.BuyerID).EmailID;
                noteRequestsObj.BuyerPhoneNumber = db.UserDetails.SingleOrDefault(m => m.UserID == notes.BuyerID).PhoneNumber;
                if (noteRequestsObj.Price == 0)
                    noteRequestsObj.SellType = "Free";
                else
                    noteRequestsObj.SellType = "Paid";

                noteRequests.Add(noteRequestsObj);
            }

            noteRequests = noteRequests.OrderByDescending(m => m.ApprovedDate).ToList();
            if (!string.IsNullOrEmpty(search))
                noteRequests = noteRequests.Where(m => m.NoteTitle.ToLower().StartsWith(search.ToLower()) || m.Category.ToLower().StartsWith(search.ToLower()) || m.BuyerEmailID.ToLower().StartsWith(search.ToLower()) || m.BuyerPhoneNumber.ToLower().StartsWith(search.ToLower()) || m.Price.ToString().Equals(search.ToLower())).ToList();
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy.Equals("title"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.NoteTitle).ToList();
                }
                else if (sortBy.Equals("category"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy.Equals("buyer"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.BuyerEmailID).ToList();
                }
                else if (sortBy.Equals("PhoneNumber"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.BuyerPhoneNumber).ToList();
                }
                else if (sortBy.Equals("price"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.Price).ToList();
                }
                else if (sortBy.Equals("download"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.ApprovedDate).ToList();
                }
            }
            return View(noteRequests.ToPagedList(page ?? 1, 10));
        }

        public ActionResult Dashboard()
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
            if (userprofile != null)
                ViewBag.profilePicture = userprofile.ProfilePicture;

            List<NoteDetail> notes = db.NoteDetails.Where(m => m.OwnerID == userId && m.IsActive == true).ToList();
            int count = 0;
            foreach (var note in notes)
            {
                count += db.RejectedNotes.Where(m => m.NoteID == note.NoteID).Count();
            }

            List<NoteRequest> noteRequests = db.NoteRequests.Where(m => m.SellerID == userId && m.Status == true).ToList();
            int count2 = 0;
            foreach (var noteReq in noteRequests)
            {
                if(noteReq != null)
                    count2 += db.NoteDetails.SingleOrDefault(m => m.OwnerID == userId && m.NoteID == noteReq.NoteID).SellPrice;
            }

            ViewBag.SoldNotes = db.NoteRequests.Where(m => m.SellerID == userId && m.Status == true).Count();
            ViewBag.moneyEarned = count2;
            ViewBag.downloads = db.NoteRequests.Where(m => m.BuyerID == userId && m.Status == true).Count();
            ViewBag.rejected = count;
            ViewBag.buyerRequest = db.NoteRequests.Where(m => m.SellerID == userId && m.Status == false).Count();

            return View();
        }


        public ActionResult InProgress(int? page, string search, string sortBy, string noteId)
        {
            List<NoteDetail> notedetails = db.NoteDetails.Where(m => m.OwnerID == userId && m.IsActive == true && m.Status != "published").OrderByDescending(m => m.CreatedDate).ToList();
            if (!string.IsNullOrEmpty(noteId))
            {
                NoteDetail notes = notedetails.FirstOrDefault(m => m.NoteID.ToString().Equals(noteId.ToString()));
                if (notes != null)
                {
                    notes.IsActive = false;
                    notes.SellFor = "fdsf";
                    db.SaveChanges();
                }
            }
            notedetails = db.NoteDetails.Where(m => m.OwnerID == userId && m.IsActive == true && m.Status != "published").OrderByDescending(m => m.CreatedDate).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                notedetails = notedetails.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.Category.ToLower().StartsWith(search.ToLower()) || m.Status.ToLower().Equals(search.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "date")
                {
                    notedetails = notedetails.OrderBy(m => m.CreatedDate).ToList();
                }
                else if (sortBy == "title")
                {
                    notedetails = notedetails.OrderBy(m => m.Title).ToList();
                }
                else if (sortBy == "category")
                {
                    notedetails = notedetails.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy == "status")
                {
                    notedetails = notedetails.OrderBy(m => m.Status).ToList();
                }
            }
            return PartialView(notedetails.ToPagedList(page ?? 1, 5));
        }

        public ActionResult Published(int? page2, string search2, string sortBy2)
        {
            List<NoteDetail> notedetails = db.NoteDetails.Where(m => m.OwnerID == userId && m.IsActive == true && m.Status == "published").OrderByDescending(m => m.CreatedDate).ToList();
            if (!string.IsNullOrEmpty(search2))
            {
                notedetails = notedetails.Where(m => m.Title.ToLower().StartsWith(search2.ToLower()) || m.Category.ToLower().StartsWith(search2.ToLower()) || m.SellPrice.ToString().Equals(search2.ToString())).ToList();
            }

            if (!string.IsNullOrEmpty(sortBy2))
            {
                if (sortBy2 == "date")
                {
                    notedetails = notedetails.OrderBy(m => m.PublishedDate).ToList();
                }
                else if (sortBy2 == "title")
                {
                    notedetails = notedetails.OrderBy(m => m.Title).ToList();
                }
                else if (sortBy2 == "category")
                {
                    notedetails = notedetails.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy2 == "selltype")
                {
                    notedetails = notedetails.OrderBy(m => m.NoteType).ToList();
                }
                else if (sortBy2 == "price")
                {
                    notedetails = notedetails.OrderBy(m => m.SellPrice).ToList();
                }
            }
            return PartialView(notedetails.ToPagedList(page2 ?? 1, 5));
        }

        public ActionResult RejectedNote(int? page, string search, string sortBy)
        {
            if (userId != 0)
            {
                var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;

                List<NoteDetail> notedetail = new List<NoteDetail>();
                List<NoteDetail> notes = db.NoteDetails.Where(m => m.OwnerID == userId && m.IsActive == true).ToList();
                foreach (var note in notes)
                {
                    if (db.RejectedNotes.SingleOrDefault(m => m.NoteID == note.NoteID) != null)
                    {
                        notedetail.Add(note);
                    }
                }

                if (!string.IsNullOrEmpty(search))
                {
                    notedetail = notedetail.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.Category.ToLower().Equals(search.ToLower())).ToList();
                }
                if (!string.IsNullOrEmpty(sortBy))
                {
                    if (sortBy == "title")
                        notedetail = notedetail.OrderBy(m => m.Title).ToList();
                    else if (sortBy == "category")
                        notedetail = notedetail.OrderBy(m => m.Category).ToList();
                }
                return View(notedetail.ToPagedList(page ?? 1, 10));
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult DownloadNote(int? page, int? noteId, string search, string sortBy, NoteReview noteReview ,SpamReport spamReport)
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
            if (userprofile != null)
                ViewBag.profilePicture = userprofile.ProfilePicture;

            List<NoteRequest> noteRequests = new List<NoteRequest>();
            List<NoteRequest> noteRequests1 = db.NoteRequests.Where(m => m.BuyerID == userId && m.Status == true).ToList();

            foreach (var notes in noteRequests1)
            {
                NoteRequest noteRequestsObj = new NoteRequest();
                NoteDetail noteDetails = db.NoteDetails.SingleOrDefault(m => m.NoteID == notes.NoteID);
                noteRequestsObj.NoteID = noteDetails.NoteID;
                noteRequestsObj.NoteTitle = noteDetails.Title;
                noteRequestsObj.Category = noteDetails.Category;
                noteRequestsObj.Price = noteDetails.SellPrice;
                noteRequestsObj.ApprovedDate = notes.ApprovedDate;
                noteRequestsObj.BuyerEmailID = db.Users.SingleOrDefault(m => m.UserID == notes.BuyerID).EmailID;

                if (noteRequestsObj.Price == 0)
                    noteRequestsObj.SellType = "Free";
                else
                    noteRequestsObj.SellType = "Paid";

                noteRequests.Add(noteRequestsObj);
            }

            noteRequests = noteRequests.OrderByDescending(m => m.ApprovedDate).ToList();
            if (!string.IsNullOrEmpty(search))
                noteRequests = noteRequests.Where(m => m.NoteTitle.ToLower().StartsWith(search.ToLower()) || m.Category.ToLower().StartsWith(search.ToLower()) || m.BuyerEmailID.ToLower().StartsWith(search.ToLower()) || m.Price.ToString().Equals(search.ToLower())).ToList();
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy.Equals("title"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.NoteTitle).ToList();
                }
                else if (sortBy.Equals("category"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy.Equals("buyer"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.BuyerEmailID).ToList();
                }
                else if (sortBy.Equals("price"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.Price).ToList();
                }
                else if (sortBy.Equals("download"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.ApprovedDate).ToList();
                }
            }

            if(noteId != 0)
            {
                var notedetail = db.NoteDetails.FirstOrDefault(m => m.NoteID == noteId);
                if(notedetail != null)
                {
                    return RedirectToAction("DownloadFile", "Home", new { filename = notedetail.UploadNote });
                }
                
            }

            if(spamReport.NoteID != 0)
            {
                SpamReport spamReport1 = db.SpamReports.SingleOrDefault(m => m.UserID == userId && m.NoteID == spamReport.NoteID);
                if (spamReport1 != null)
                {
                    spamReport1.Remark = spamReport.Remark;
                    spamReport1.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                }
                else
                {
                    spamReport.UserID = userId;
                    spamReport.CreatedDate = DateTime.Now;
                    db.SpamReports.Add(spamReport);
                    db.SaveChanges();   
                }
            }

            if (noteReview.NoteID != 0)
            {
                NoteReview noteReview1 = db.NoteReviews.SingleOrDefault(m => m.UserID == userId && m.NoteID == noteReview.NoteID);
                if(noteReview1 != null)
                {
                    noteReview1.Rating = noteReview.Rating;
                    noteReview1.Comments = noteReview.Comments;
                    noteReview1.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                }
                else
                {
                    noteReview.UserID = userId;
                    noteReview.OwnerID = db.NoteDetails.SingleOrDefault(m => m.NoteID == noteReview.NoteID).OwnerID;
                    noteReview.IsActive = true;
                    noteReview.CreatedDate = DateTime.Now;
                    db.NoteReviews.Add(noteReview);
                    db.SaveChanges();
                }
                
            }

            Download download = new Download();
            download.pagedList = noteRequests.ToPagedList(page ?? 1, 10);
            return View(download);
        }

        public ActionResult SoldNote(int? page, string search, string sortBy, string noteId)
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
            if (userprofile != null)
                ViewBag.profilePicture = userprofile.ProfilePicture;

            if (!string.IsNullOrEmpty(noteId))
            {
                var notedetail = db.NoteDetails.FirstOrDefault(m => m.NoteID.ToString().Equals(noteId));
                if (notedetail != null)
                {
                    return RedirectToAction("DownloadFile", "Home", new { filename = notedetail.UploadNote });
                }
            }

            List<NoteRequest> noteRequests = new List<NoteRequest>();
            List<NoteRequest> noteRequests1 = db.NoteRequests.Where(m => m.SellerID == userId && m.BuyerID != userId && m.Status == true).ToList();

            foreach (var notes in noteRequests1)
            {
                var noteRequestsObj = new NoteRequest();
                NoteDetail noteDetails = db.NoteDetails.SingleOrDefault(m => m.NoteID == notes.NoteID);
                noteRequestsObj.NoteID = noteDetails.NoteID;
                noteRequestsObj.NoteTitle = noteDetails.Title;
                noteRequestsObj.Category = noteDetails.Category;
                noteRequestsObj.Price = noteDetails.SellPrice;
                noteRequestsObj.ApprovedDate = notes.ApprovedDate;
                noteRequestsObj.BuyerEmailID = db.Users.SingleOrDefault(m => m.UserID == notes.BuyerID).EmailID;

                if (noteRequestsObj.Price == 0)
                    noteRequestsObj.SellType = "Free";
                else
                    noteRequestsObj.SellType = "Paid";

                noteRequests.Add(noteRequestsObj);
            }

            noteRequests = noteRequests.OrderByDescending(m => m.ApprovedDate).ToList();
            if (!string.IsNullOrEmpty(search))
                noteRequests = noteRequests.Where(m => m.NoteTitle.ToLower().StartsWith(search.ToLower()) || m.Category.ToLower().StartsWith(search.ToLower()) || m.BuyerEmailID.ToLower().StartsWith(search.ToLower()) || m.Price.ToString().Equals(search.ToLower())).ToList();
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy.Equals("title"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.NoteTitle).ToList();
                }
                else if (sortBy.Equals("category"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy.Equals("buyer"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.BuyerEmailID).ToList();
                }
                else if (sortBy.Equals("price"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.Price).ToList();
                }
                else if (sortBy.Equals("download"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.ApprovedDate).ToList();
                }
            }

            return View(noteRequests.ToPagedList(page ?? 1, 10));
        }

        public ActionResult ChangePassword()
        {
            if(userId == 0)
            {
                return RedirectToAction("Login","Home");
            }
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePassword changePassword)
        {
            if(userId == 0)
            {
                return RedirectToAction("Login","Home");
            }
            User v_user = db.Users.SingleOrDefault(m => m.UserID == userId && m.Password.Equals(changePassword.Password1));
            if (v_user != null)
            {
                v_user.Password = changePassword.Password2;
                v_user.Password2 = changePassword.Password2;
                v_user.UserID = userId;
                v_user.EmailID = v_user.EmailID;
                v_user.FirstName = v_user.FirstName;
                v_user.LastName = v_user.LastName;
                v_user.IsDetailsSubmitted = v_user.IsDetailsSubmitted;
                v_user.IsEmailVerified = v_user.IsEmailVerified;
                v_user.ModifiedDate = DateTime.Now;
                v_user.IsActive = v_user.IsActive;
                db.SaveChanges();
                return RedirectToAction("Login","Home");
            }
            else
            {
                ModelState.AddModelError("Password1","Your password not match!");
                return View();
            }
          
        }

        public FileResult DownloadFile(string filename)
        {
            string path = Path.Combine(Server.MapPath("~/Uploads/Notes/")) + filename;
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
        }

        public ActionResult NoteDetail(string noteId, string download)
        {
            ViewBag.isRegister = false;
            NoteDetail notedetail = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(noteId) && m.IsActive == true);
            if (string.IsNullOrEmpty(noteId))
            {
                return HttpNotFound();
            }
            if (userId != 0)
            {
                ViewBag.isRegister = true;
                var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;

            }
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

                noteReview.Rating = temp.Rating;
                noteReview.Comments = temp.Comments;
                noteReview.FullName = v_user.FirstName + " " + v_user.LastName;

                customerReview.Add(noteReview);
            }
            ViewBag.Customer = customerReview.OrderByDescending(m => m.Rating);



            if (notedetail.SellPrice == 0)
                ViewBag.btnMsg = "Download";
            else
                ViewBag.btnMsg = "Download/$" + notedetail.SellPrice.ToString();

            var found = db.NoteRequests.SingleOrDefault(m => m.BuyerID == userId && m.NoteID == notedetail.NoteID);
            var approved = db.NoteRequests.SingleOrDefault(m => m.BuyerID == userId && m.NoteID == notedetail.NoteID && m.Status == true);

            if (string.IsNullOrEmpty(download))
            {
                if (found != null && approved == null)
                {
                    ViewBag.btnMsg = "Requested";
                }
            }
            else
            {
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Home");
                }
                if (userId == notedetail.OwnerID)
                {
                    return RedirectToAction("DownloadFile", "Home", new { filename = notedetail.UploadNote });
                }

                if (found != null)
                {
                    if (approved != null)
                    {
                        return RedirectToAction("DownloadFile", "Home", new { filename = notedetail.UploadNote });
                    }
                    else
                    {
                        ViewBag.btnMsg = "Requested";
                    }
                }
                else
                {
                    NoteRequest noteRequest = new NoteRequest();
                    noteRequest.BuyerID = userId;
                    noteRequest.SellerID = notedetail.OwnerID;
                    noteRequest.NoteID = notedetail.NoteID;
                    noteRequest.NoteTitle = noteRequest.NoteTitle;
                    noteRequest.Category = noteRequest.Category;
                    noteRequest.Price = noteRequest.Price;
                    noteRequest.BuyerEmailID = noteRequest.BuyerEmailID;
                    noteRequest.BuyerPhoneNumber = noteRequest.BuyerPhoneNumber;
                    noteRequest.SellType = noteRequest.SellType;
                    noteRequest.ApprovedDate = DateTime.Now;

                    if (notedetail.SellPrice == 0)
                    {
                        noteRequest.Status = true;
                        db.NoteRequests.Add(noteRequest);
                        db.SaveChanges();
                        return RedirectToAction("DownloadFile", "Home", new { filename = notedetail.UploadNote }); ;
                    }
                    else if (notedetail.SellPrice != 0)
                    {
                        noteRequest.Status = false;
                        db.NoteRequests.Add(noteRequest);
                        db.SaveChanges();
                        ViewBag.popup = true;
                        ViewBag.btnMsg = "Requested";

                        User user = db.Users.SingleOrDefault(m => m.UserID == notedetail.OwnerID);
                        var username = db.Users.SingleOrDefault(m => m.UserID == userId);
                        var sellername = user.FirstName + " " + user.LastName;
                        ViewBag.username = username.FirstName;
                        ViewBag.sellername = sellername;

                        try
                        {
                            MailMessage mail = new MailMessage("DJpatel0134@gmail.com", "jasoliyadharmik81@gmail.com");//,user.EmailID.ToString());               
                            mail.Subject = username.FirstName + " " + username.LastName + " wants to purchase your notes ";
                            string Body = "Hello " + sellername + ",<br><br>We would like to inform you that, " + username.FirstName + " " + username.LastName + " wants to purchase your notes. Please see <br> Buyer Requests tab and allow download access to Buyer if you have received the payment from<br>him.<br><br>Regards,<br>Notes Marketplace";

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
                            return View(notedetail);
                        }
                    }
                }
            }
           
           return View(notedetail);
        }

        public ActionResult AddNote(string noteId)
        {
            if(userId == 0)
            {
                return RedirectToAction("Login","Home");
            }
            var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
            if (userprofile != null)
                ViewBag.profilePicture = userprofile.ProfilePicture;

            NoteDetail noteDetail = new NoteDetail();
            if (!string.IsNullOrEmpty(noteId))
            {
                noteDetail = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(noteId));
                if (noteDetail.SellPrice != 0)
                    noteDetail.SellFor = "paid";
                else
                    noteDetail.SellFor = "free";

                if (noteDetail.OwnerID  != userId)
                {
                    return HttpNotFound();
                }
            }

            List<string> categories = db.Categories.Select(m => m.Categories).Distinct().ToList();
            List<string> types = db.Types.Select(m => m.TypeName).Distinct().ToList();
            List<string> countries = db.Countries.Select(m => m.CountryName).Distinct().ToList();
            ViewBag.category = categories;
            ViewBag.type = types;
            ViewBag.country = countries;

            return View(noteDetail);
        }

        [HttpPost]
        public ActionResult AddNote(NoteDetail noteDetail, string submit)
        {
               switch (submit)
               {
                   case "save":
                         noteDetail.Status = "draft";
                         break;
                   case "publish":
                         noteDetail.Status = "in review";
                         break;
               }
                var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                   ViewBag.profilePicture = userprofile.ProfilePicture;

                List<string> categories = db.Categories.Select(m => m.Categories).Distinct().ToList();
                List<string> types = db.Types.Select(m => m.TypeName).Distinct().ToList();
                List<string> countries = db.Countries.Select(m => m.CountryName).Distinct().ToList();
                ViewBag.category = categories;
                ViewBag.type = types;
                ViewBag.country = countries;

                noteDetail.OwnerID = userId;
                noteDetail.IsActive = true;
                noteDetail.CreatedDate = DateTime.Now;
                
                if (db.NoteDetails.Where(m => m.Title.ToLower().Equals(noteDetail.Title.ToLower())).Count() != 0 && noteDetail.NoteID == 0)
                {
                   ModelState.AddModelError("Title","This title is already exists!"); 
                   return View(noteDetail);
                }
                if(noteDetail.SellFor == "free")
                {
                    noteDetail.SellPrice = 0;
                }
                if(noteDetail.FileBookPicture != null)
                {
                    if (noteDetail.FileBookPicture.ContentLength > 10 * 1024 * 1024)
                    {
                        ViewBag.fileMsg = "file size is morethan 10 MB.";
                        return View(noteDetail);
                    }
                    else
                    {
                        string extension = Path.GetExtension(noteDetail.FileBookPicture.FileName);
                        string fileName = userId.ToString() + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                        string path = Path.Combine(Server.MapPath("~/Uploads/BookPicture/"), fileName);
                        noteDetail.FileBookPicture.SaveAs(path);
                        noteDetail.BookPicture = fileName;
                    }
                }
                else if(noteDetail.NoteID == 0)
                {
                    noteDetail.BookPicture = "default.jpg";
                }
                if(noteDetail.FileUploadNote != null)
                {
                    string extension = Path.GetExtension(noteDetail.FileUploadNote.FileName);
                    if (extension.ToLower().Equals(".pdf"))
                    {
                        string fileName = userId.ToString() + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                        string path = Path.Combine(Server.MapPath("~/Uploads/Notes/"), fileName);
                        noteDetail.FileUploadNote.SaveAs(path);
                        noteDetail.UploadNote = fileName;
                        noteDetail.NoteSize = (int)((float)noteDetail.FileUploadNote.ContentLength / (1024.0));
                    }
                    else
                    {
                        ViewBag.fileMsg = "File must be in PDF only!";
                        return View(noteDetail);
                    }
                }
                else if(noteDetail.NoteID == 0)
                {
                      ModelState.AddModelError("FileUploadNote","The Upload Notes field is required!.");
                     return View(noteDetail);
                }
            if (noteDetail.SellFor == "paid" && noteDetail.SellPrice == 0)
            {
                ModelState.AddModelError("SellPrice", "Price not be zero!");
                return View(noteDetail);
            }

            if (noteDetail.FileNotePreview != null)
                {
                    string extension = Path.GetExtension(noteDetail.FileNotePreview.FileName);
                    if (extension.Equals(".pdf"))
                    {
                        string fileName = userId.ToString() + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                        string path = Path.Combine(Server.MapPath("~/Uploads/NotePreview"), fileName);
                        noteDetail.FileNotePreview.SaveAs(path);
                        noteDetail.NotePreview = fileName;
                    }
                    else
                    {
                        ViewBag.previewMsg = "File must be in PDF only!";
                        return View(noteDetail);
                    }
                }
                else if(noteDetail.SellFor == "paid" && noteDetail.NoteID == 0)
                {   
                    ViewBag.previewMsg = "Note preview is required!";
                    return View(noteDetail);
                }
                

            try
            { 
                if (noteDetail.Status == "publish")
                {
                    User v_user = db.Users.SingleOrDefault(m => m.UserID == userId);
                    MailMessage mail = new MailMessage("DJpatel0134@gmail.com", "jasoliyadharmik81@gmail.com");//,user.EmailID.ToString());               
                    mail.Subject = v_user.FirstName + " sent his note for review";
                    string Body = "Hello Admins,<br><br>We want to inform you that, " + v_user.FirstName + " sent his note<br>" + noteDetail.Title + " for review. Please look at the notes and take required actions.<br><br>Regards,<br>Notes Marketplace";
                
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

                if(noteDetail.NoteID == 0)
                {
                    db.NoteDetails.Add(noteDetail);
                }
                else
                {
                    NoteDetail noteDetail1 = db.NoteDetails.SingleOrDefault(m => m.NoteID == noteDetail.NoteID);
                   // noteDetail1.NoteID = noteDetail.NoteID;
                   // noteDetail1.OwnerID = noteDetail.OwnerID;
                    noteDetail1.Title = noteDetail.Title;
                    noteDetail1.Category = noteDetail.Category;
                    noteDetail1.NoteType = noteDetail.NoteType;
                    noteDetail1.NumberOfPages = noteDetail.NumberOfPages;
                    noteDetail1.NotesDescription = noteDetail.NotesDescription;
                    noteDetail1.InstitutionName = noteDetail.InstitutionName;
                    noteDetail1.Country = noteDetail.Country;
                    noteDetail1.Course = noteDetail.Course;
                    noteDetail1.CourseCode = noteDetail.CourseCode;
                    noteDetail1.Professor = noteDetail.Professor;
                    noteDetail1.SellPrice = noteDetail.SellPrice;
                    noteDetail1.NoteSize = noteDetail.NoteSize;
                 //   noteDetail1.PublishedDate = noteDetail.PublishedDate;
                    noteDetail1.Status = noteDetail.Status;
                  //  noteDetail1.CreatedDate = noteDetail.CreatedDate;
                    noteDetail1.ModifiedDate = DateTime.Now;
                   // noteDetail1.IsActive = noteDetail.IsActive;
                    noteDetail1.SellFor = noteDetail.SellFor;

                    if(noteDetail.FileBookPicture != null)
                    {
                        noteDetail1.BookPicture = noteDetail.BookPicture;
                    }
                    
                    if (noteDetail.FileUploadNote != null)
                    {
                        noteDetail1.UploadNote = noteDetail.UploadNote;
                    }
                  
                    if(noteDetail.FileNotePreview != null)
                    {
                        noteDetail1.NotePreview = noteDetail.NotePreview;
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Dashboard","Home");
            }catch(Exception)
            {
                return View(noteDetail);
            }
        }
    }
}