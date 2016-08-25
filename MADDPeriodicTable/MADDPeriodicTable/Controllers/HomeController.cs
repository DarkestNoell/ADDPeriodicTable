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
using System.Web.Script.Serialization;

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
                    ViewBag.image = "/Content/Images/chemistry.gif";
                }

                if(up.CompoundsInARow == 3 && up.HotStreakBadge == false)
                {
                    up.HotStreakBadge = true;
                    ViewBag.HotStreakBadge = "Congratulations! You have earned the Hot Streak badge!";
                    ViewBag.image = "/Content/Images/onfire.gif";
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

        public ActionResult EditUserProfileInfo()
        {
            return View();
        }

        [HttpPost]
        public ActionResult EditUserProfile(UserProfileInfo upi)
        {
            PeriodicTableEntities pte = new PeriodicTableEntities();
            String currUser = User.Identity.Name;
            UserProfileInfo upiToEdit = pte.UserProfileInfoes.Where(progress => progress.Id.Equals(currUser)).First();
            UserProgress up = pte.UserProgresses.Where(progress => progress.Id.Equals(currUser)).First();


            if (upi.UserBio != null)
            {
                upiToEdit.UserBio = upi.UserBio;
            }

            if(upi.UserProfileImage != null)
            {
                upiToEdit.UserProfileImage = upi.UserProfileImage;
            }

            pte.SaveChanges();

            Tuple<UserProgress, UserProfileInfo> tuple = new Tuple<UserProgress, UserProfileInfo>(up, upiToEdit);

            return View("CustomUserProfile", tuple);
        }


        public ActionResult CustomUserProfile()
        {
            String currUser = User.Identity.Name;
            PeriodicTableEntities pte = new PeriodicTableEntities();
            UserProgress up = pte.UserProgresses.Where(progress => progress.Id.Equals(currUser)).First();
            UserProfileInfo upi = pte.UserProfileInfoes.Where(profileInfo => profileInfo.Id.Equals(currUser)).First();

            Tuple<UserProgress, UserProfileInfo> tuple = new Tuple<UserProgress, UserProfileInfo>(up, upi);
            return View(tuple);
        }

        public ActionResult Index()
        {
            //var data = pte.sp_s_Com().ToList();
            //ViewBag.details = data;


            PeriodicTableEntities pte = new PeriodicTableEntities();

            var AllCompounds = from Compound in pte.Compounds
                               where  Compound.ID != 0
                               select Compound;

            var AllElements = from Element in pte.Elements
                              where Element.ID != 0
                              select Element;



            //List we pass into dragging, contains all compound formulas
            List <String> CompoundFormulas = new List<String>();

            foreach (Compound c in AllCompounds)
            {
                CompoundFormulas.Add(c.Formula);
            }

            foreach(Compound c in AllCompounds)
            {
                CompoundFormulas.Add(c.Formula);
            }


            String[] CompoundFormulaArray = CompoundFormulas.ToArray();

            List<Element> Elements = new List<Element>();
            Elements = AllElements.ToList();
            Element[] ElementArray = Elements.ToArray();

            //String[] - Contains all Compound Names
            //Element - Contains all Elements in the database
            Tuple<String[], Element[]> tuple = new Tuple<String[], Element[]>(CompoundFormulaArray, ElementArray);


            return View("Dragging", tuple);
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

        public ActionResult Relief()
        {
            return View();
        }

        public ActionResult LearningVideos()
        {
            return View();
        }
        public bool CheckCompound(string[] s)
        {


            return true;
        }

    }
    public static class HtmlExtensions
    {
        public static string JsonSerialize(this HtmlHelper htmlHelper, object value)
        {
            return new JavaScriptSerializer().Serialize(value);
        }
    }
}