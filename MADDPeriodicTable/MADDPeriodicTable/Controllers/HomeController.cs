using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MADDPeriodicTable.Models;

namespace MADDPeriodicTable.Controllers
{
    public class HomeController : Controller
    {
        static Compound CompoundToPick;

        [HttpPost]
        public ActionResult CheckAnswer(string answerText)
        {
            PeriodicTableEntities pte = new PeriodicTableEntities();
            String currUser = User.Identity.Name;
            UserProgress up = pte.UserProgresses.Where(progress => progress.Id.Equals(currUser)).FirstOrDefault();

            if (string.IsNullOrEmpty(Request.Form["answerText"]))
            {
                answerText = Request.Form["answerText"];
            }

            if (pte.Compounds.Where(compound => compound.Formula.Equals(answerText)).FirstOrDefault() == null)
            {
                //No Match, no Points
                up = pte.UserProgresses.Where(progress => progress.Id.Equals(currUser)).FirstOrDefault();
                up.CompoundsInARow = 0;
                if(up.ChemistsExplosionBadge)
                {
                    ViewBag.ChemistsExplosionBadge = "";
                }
                if(up.ChemistsExplosionBadge == false)
                {
                    ViewBag.ChemistsExplosionBadge = "Congratulations! You have earned the Hot Streak badge!";
                    up.ChemistsExplosionBadge = true;
                }

                Tuple<UserProgress, Compound> tuple = new Tuple<UserProgress, Compound>(up, CompoundToPick);
                return View("Learn", tuple);
            }

             var matchesCompound = pte.Compounds.Where(compound => compound.Formula.Equals(answerText)).First();

            if (matchesCompound.Formula.Equals(CompoundToPick.Formula))
            {
                //Earn Points
                up = pte.UserProgresses.Where(progress => progress.Id.Equals(currUser)).FirstOrDefault();
                up.CurrentPoints += 10 * matchesCompound.CompoundDifficulty;
                up.CompoundsInARow += 1;
                up.CompoundsCorrect += 1;

                if (up.HotStreakBadge)
                {
                    ViewBag.HotStreakBadge = "";
                }else if(up.NoviceChemistBadge)
                {
                    ViewBag.NoviceChemistBadge = "";
                }
        

                if(up.CompoundsCorrect >= 1 && up.NoviceChemistBadge == false)
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
            int RandyNum = Randy.Next(levelRange + 1);

            CompoundToPick = pte.Compounds.Where(Compound => Compound.ID == RandyNum).FirstOrDefault();
            while(CompoundToPick == null)
            {
                CompoundToPick = pte.Compounds.Where(Compound => Compound.ID == RandyNum).FirstOrDefault();
            }


            Tuple<UserProgress , Compound> tuple = new Tuple<UserProgress, Compound>(up, CompoundToPick);
            return View(tuple);
        }

        public ActionResult Index()
        {
            //Random randy = new Random();
            //PeriodicTableEntities1 pte = new PeriodicTableEntities1();
            //int id = randy.Next(39) + 1;

            //Compound selectedCompound = pte.Compounds.Where(compound => compound.ID == id).First();
            //Console.WriteLine(selectedCompound.Compound_Name);
            return View(/*selectedCompound*/);
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
    }
}