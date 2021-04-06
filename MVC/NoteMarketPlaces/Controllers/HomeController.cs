using NoteMarketPlaces.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace NoteMarketPlaces.Controllers
{
    public class HomeController : Controller
    {
        static int userId = 0;
        NoteMarketPlaceEntities db = new NoteMarketPlaceEntities();

        public ActionResult Home()
        {
            ViewBag.isRegister = false;
            if(userId != 0)
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
               // user.UserID = 7;
                user.IsActive = true;
                user.IsDetailsSubmitted = false;
                user.IsEmailVerified = false;
                user.CreatedDate = DateTime.Now.Date;
                user.ModifiedDate = DateTime.Now.Date;
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

            } catch (Exception)
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
                cookie.Expires = DateTime.Now.AddMonths(2);
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
                            return RedirectToAction("Home", "Home");
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
            }
            else
            {
                ViewBag.auth_msg = "Enter valid username or password!";
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

                } catch (Exception)
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
            if(userId != 0)
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
            if (!ModelState.IsValid)
            {
                return View();
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
            } catch (Exception)
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
                var userprofile =  db.UserDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;
            }
            List<NoteDetail> notedetail = db.NoteDetails.ToList();
            if (!string.IsNullOrEmpty(search))
            {
                notedetail = notedetail.Where(m => m.Title.ToLower().StartsWith(search.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(type))
            {
                notedetail = notedetail.Where(m => m.NoteType.ToLower().Equals(type)).ToList();
            }
            if (!string.IsNullOrEmpty(category))
            {
                notedetail = notedetail.Where(m => m.Category.ToLower() == category.ToLower()).ToList();
            }
            if (!string.IsNullOrEmpty(university))
            {
                notedetail = notedetail.Where(m => m.InstitutionName.ToLower() == university.ToLower()).ToList();
            }
            if (!string.IsNullOrEmpty(course))
            {
                notedetail = notedetail.Where(m => m.NoteType.ToLower() == course.ToLower()).ToList();
            }
            if (!string.IsNullOrEmpty(country))
            {
                notedetail = notedetail.Where(m => m.Country.ToLower() == country.ToLower()).ToList();
            }
            if (!string.IsNullOrEmpty(rating))
            { }

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
                userdetail.User = v_user;
                List<Country> country = db.Countries.ToList();
                ViewBag.country = country;
                return View(userdetail);
            }
            return RedirectToAction("Login","Home");
        }

        [HttpPost]
        public ActionResult UserProfile(UserDetail userdetail)
        {
            List<Country> country = db.Countries.ToList();
            ViewBag.country = country;

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
                        string fileName = userId.ToString() + "abcd" + extension;
                        string path = Path.Combine(Server.MapPath("~/Uploads/ProfilePicture/"), fileName);
                        userdetail.File.SaveAs(path);
                        userdetail.ProfilePicture = fileName;
                    }
                }

                db.UserDetails.Add(userdetail);
                db.SaveChanges();

                return RedirectToAction("SearchNote", "Home");
            } catch (Exception)
            {
                return View(userdetail);
            }
        }

        public ActionResult LogOut()
        {
            userId = 0;
            return RedirectToAction("Login", "Home");
        }

        public ActionResult BuyerRequest(int? page, string search, string sortBy)
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            var userprofile = db.UserDetails.SingleOrDefault(m => m.UserID == userId);
            if (userprofile != null)
                ViewBag.profilePicture = userprofile.ProfilePicture;

            List<NoteRequest> noteRequests = new List<NoteRequest>();
            List<NoteRequest> noteRequests1 = db.NoteRequests.Where(m => m.SellerID == userId && m.Status == false).ToList();
            
            foreach (var notes in noteRequests1)
            {
                var noteRequestsObj = new NoteRequest();
                NoteDetail noteDetails = db.NoteDetails.SingleOrDefault(m => m.NoteID == notes.NoteID);
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

           noteRequests =  noteRequests.OrderByDescending(m => m.ApprovedDate).ToList();
            if (!string.IsNullOrEmpty(search))
                noteRequests = noteRequests.Where(m => m.NoteTitle.ToLower().StartsWith(search.ToLower()) || m.Category.ToLower().StartsWith(search.ToLower()) || m.BuyerEmailID.ToLower().StartsWith(search.ToLower()) || m.BuyerPhoneNumber.ToLower().StartsWith(search.ToLower()) || m.Price.ToString().Equals(search.ToLower())).ToList();
            if(!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy.Equals("title"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.NoteTitle).ToList();
                }
                else if(sortBy.Equals("category"))
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
                count2 += db.NoteDetails.SingleOrDefault(m => m.OwnerID == userId && m.NoteID == noteReq.NoteID).SellPrice;
            }
            
            ViewBag.SoldNotes = db.NoteRequests.Where(m => m.SellerID == userId && m.Status == true).Count();
            ViewBag.moneyEarned = count2;
            ViewBag.downloads = db.NoteRequests.Where(m => m.BuyerID == userId && m.Status == true).Count();
            ViewBag.rejected = count;
            ViewBag.buyerRequest = db.NoteRequests.Where(m => m.SellerID == userId && m.Status ==false).Count();

            return View();
        }

        
        public ActionResult InProgress(int? page, string search, string sortBy, string noteId)
        {
            List<NoteDetail> notedetails = db.NoteDetails.Where(m => m.OwnerID == userId && m.IsActive == true && m.Status != "published").OrderByDescending(m => m.CreatedDate).ToList();
            if(!string.IsNullOrEmpty(noteId))
            {
                NoteDetail notes = notedetails.FirstOrDefault(m => m.NoteID.ToString().Equals(noteId.ToString()));
                if (notes != null)
                {
                    notes.IsActive = false;
                }
                db.SaveChanges();
            }
            if (!string.IsNullOrEmpty(search))
            {
                notedetails = notedetails.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.Category.ToLower().StartsWith(search.ToLower()) || m.Status.ToLower().Equals(search.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                if(sortBy == "date")
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
            if(!string.IsNullOrEmpty(search2))
            {
                notedetails = notedetails.Where(m => m.Title.ToLower().StartsWith(search2.ToLower()) || m.Category.ToLower().StartsWith(search2.ToLower()) || m.SellPrice.ToString().Equals(search2.ToString())).ToList();
            }

            if(!string.IsNullOrEmpty(sortBy2))
            {
                if(sortBy2 == "date")
                {
                    notedetails = notedetails.OrderBy(m => m.PublishedDate).ToList();
                }
                else if(sortBy2 == "title")
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
    }
}