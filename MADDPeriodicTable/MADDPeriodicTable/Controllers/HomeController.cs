using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MADDPeriodicTable.Models;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MADDPeriodicTable.Controllers
{
    public class HomeController : Controller
    {
        static Compound CompoundToPick;

        [HttpPost]
        public ActionResult CheckAnswer(string answerText)
        {
            answerText = Regex.Replace(answerText, @"\s+", " ");
            PeriodicTableEntities pte = new PeriodicTableEntities();
            String currUser = User.Identity.Name;
            UserProgress up = pte.UserProgresses.Where(progress => progress.Id.Equals(currUser)).FirstOrDefault();

            if (up.HotStreakBadge)
            {
                ViewBag.HotStreakBadge = "";
            }
            else if (up.NoviceChemistBadge)
            {
                ViewBag.NoviceChemistBadge = "";
            } else if (up.ChemistsExplosionBadge)
            {
                ViewBag.ChemistsExplosionBadge = "";
            }

            if (string.IsNullOrEmpty(Request.Form["answerText"]))
            {
                answerText = Request.Form["answerText"];
            }
            var matchesCompound = pte.Compounds.Where(compound => compound.Formula.Equals(answerText)).FirstOrDefault();
            if (matchesCompound == null)
            {
                ViewBag.ResultMessage = "Sorry! You answered incorrectly :'(";
                ViewBag.image = "/Content/Images/sadface.gif";
                Tuple<UserProgress, Compound> tuple = new Tuple<UserProgress, Compound>(up, CompoundToPick);
                return View("PointsEarned", tuple);
            }

            if (!matchesCompound.Formula.Equals(answerText))
            {

                ViewBag.ResultMessage = "Sorry! You answered incorrectly :'(";


                //No Match, no Points
                up = pte.UserProgresses.Where(progress => progress.Id.Equals(currUser)).FirstOrDefault();
                up.CompoundsInARow = 0;
                if(up.ChemistsExplosionBadge)
                {
                    ViewBag.ChemistsExplosionBadge = "";
                }
               
                
                Tuple<UserProgress, Compound> tuple = new Tuple<UserProgress, Compound>(up, CompoundToPick);
                return View("PointsEarned", tuple);
            }

             var matchesCompound2 = pte.Compounds.Where(compound => compound.Formula.Equals(answerText)).FirstOrDefault();

            

            if (matchesCompound.Formula.Equals(answerText))
            {
                //Earn Points
                ViewBag.ResultMessage = "Congratulations! You have successfully earned: " + (CompoundToPick.CompoundDifficulty * 10) + " points!";
                up = pte.UserProgresses.Where(progress => progress.Id.Equals(currUser)).FirstOrDefault();
                up.CurrentPoints += 10 * matchesCompound.CompoundDifficulty;
                up.CompoundsInARow += 1;
                up.CompoundsCorrect += 1;
                ViewBag.image = "/Content/Images/Congrats.gif";



                if (up.CompoundsCorrect >= 1 && up.NoviceChemistBadge == false)
                {
                    up.NoviceChemistBadge = true;
                    ViewBag.NoviceChemistBadge = "Congratulations! You have earned the novice chemist badge!";
                }

                if(up.CompoundsInARow == 3 && up.HotStreakBadge == false)
                {
                    up.HotStreakBadge = true;
                    ViewBag.HotStreakBadge = "Congratulations! You have earned the Hot Streak badge!";
                }
                
                //Level up?
                int level = up.CurrentPoints / 100;
                if(level == 0)
                {
                    //If the user has less than 100 points, don't want to break things: )
                    level = 1;
                }

                up.CurrentLevel = level;
                pte.SaveChanges();
            }

             return View("PointsEarned");
        }

        public ActionResult Learn()
        {
            String currUser = User.Identity.Name;
            PeriodicTableEntities pte = new PeriodicTableEntities();
            UserProgress up = pte.UserProgresses.Where(progress => progress.Id.Equals(currUser)).First();
            int levelRange = up.CurrentLevel * 10;
            Random Randy = new Random();
            int RandyNum = Randy.Next(levelRange + 2);

            CompoundToPick = pte.Compounds.Where(Compound => Compound.ID == RandyNum).FirstOrDefault();
            while(CompoundToPick == null)
            {
                RandyNum = Randy.Next(levelRange + 2);
                CompoundToPick = pte.Compounds.Where(Compound => Compound.ID == RandyNum).FirstOrDefault();
            }


            Tuple<UserProgress , Compound> tuple = new Tuple<UserProgress, Compound>(up, CompoundToPick);
            return View(tuple);
        }

        public ActionResult Index()
        {
            PeriodicTableEntities pte = new PeriodicTableEntities();
            var data = pte.sp_s_Ele().ToList();
            ViewBag.userdetails = data;
            return View("Dragging"/*selectedCompound*/);
        }
        public ActionResult Elements()
        {
            PeriodicTableEntities pte = new PeriodicTableEntities();
            var data = pte.sp_s_Ele().ToList();
            ViewBag.userdetails = data;
            return View();
        }

        public ActionResult Compounds()
        {
            PeriodicTableEntities pte = new PeriodicTableEntities();
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