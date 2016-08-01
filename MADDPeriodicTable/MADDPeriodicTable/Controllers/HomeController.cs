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
        public ActionResult Index()
        {
            Random randy = new Random();
            PeriodicTableEntities1 pte = new PeriodicTableEntities1();
            int id = randy.Next(39) + 1;

            Compound selectedCompound = pte.Compounds.Where(compound => compound.ID == id).First();
            Console.WriteLine(selectedCompound.Compound_Name);
            return View(selectedCompound);
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
    }
}