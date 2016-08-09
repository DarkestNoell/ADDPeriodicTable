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
    }
}