using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MADDPeriodicTable.Models;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace MADDPeriodicTable.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //Random randy = new Random();
            //PeriodicTableEntities1 pte = new PeriodicTableEntities1();
            //int id = randy.Next(39) + 1;

            //Compound selectedCompound = pte.Compounds.Where(compound => compound.ID == id).First();
            //Console.WriteLine(selectedCompound.Compound_Name);
            return View("Dragging"/*selectedCompound*/);
        }
        public ActionResult Elements()
        {
            PeriodicTableEntities1 pte = new PeriodicTableEntities1();
            var data = pte.sp_s_Ele().ToList();
            ViewBag.userdetails = data;
            return View();
        }
        public ActionResult Compounds()
        {
            PeriodicTableEntities1 pte = new PeriodicTableEntities1();
            var data = pte.sp_s_Com().ToList();
            ViewBag.userdetail = data;
            return View();
        }
        public ActionResult ContactUs()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ContactUs(EmailFormModel model)
        {
            if (ModelState.IsValid)
            {
                var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
                var message = new MailMessage();
                message.To.Add(new MailAddress("dynamicweblab@outlook.com"));
                message.Subject = "Your email subject";
                message.Body = string.Format(body, model.FromName, model.FromEmail, model.Message);
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    await smtp.SendMailAsync(message);
                    return RedirectToAction("Sent");
                }
            }
            return View(model);
        }
        public ActionResult Sent()
        {
            return View();
        }
    }
}