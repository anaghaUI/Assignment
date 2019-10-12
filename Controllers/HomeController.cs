using Assignment.Models;
using FIT5032_Week08A.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Assignment.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Discussion()
        {
            return View();
        }


        [Authorize(Roles = "Staff")]
        public ActionResult Newsletter()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff")]
        public ActionResult Newsletter([Bind(Include = "Id,Subject")] Newsletter @newsletter, HttpPostedFileBase postedFile)
        {
            EventEntity db = new EventEntity();
            var myUniqueFileName = string.Format(@"{0}", Guid.NewGuid());
            @newsletter.Path = myUniqueFileName;
            if (ModelState.IsValid)
            {
                string serverPath = Server.MapPath("~/Uploads/");
                string fileExtension = Path.GetExtension(postedFile.FileName);
                string filePath = @newsletter.Path + fileExtension;
                @newsletter.Path = filePath;
                var letterPath = serverPath + @newsletter.Path;
                postedFile.SaveAs(letterPath);

                EmailSender es = new EmailSender();
                var contents = "<div><img src=" + letterPath + " /></div>";
                var userList = db.AspNetUsers.ToList();
                var emailList = new List<string>();
                foreach (AspNetUser user in userList)
                {
                    emailList.Add(user.Email);
                }
                es.SendBulkEmail(emailList, "Be Our Guest Newsletter", contents);
                ViewBag.Result = "Email has been send.";
            }
            return View();
        }
    }
}