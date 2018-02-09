using DriftService.Context;
using DriftService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Driftservice_Registration.Controllers
{
    public class UnregistrationController : Controller
    {
        DriftContext db = new DriftContext();
        ContactViewModel ContactViewModel = new ContactViewModel();

        // GET: Unregistration
        public ActionResult Index()
        {
            if (Request.QueryString["Contact"] != null)
            {
                ContactViewModel.ContactGuidAsString = Request.QueryString["Contact"];
            }
            return View(ContactViewModel);
        }

        public ActionResult Unregister(string guid)
        {
            try
            {
                if (guid != null)
                {
                    foreach (var i in db.Contacts.ToList())
                    {
                        if (i.ContactGuid.ToString() == guid)
                        {
                            db.Contacts.Remove(i);
                            db.ContactServiceTypes.RemoveRange(db.ContactServiceTypes.Where(x => x.ContactID == i.ContactID));
                            db.SaveChanges();
                            break;
                        }
                    }
                }
                else
                {
                    ViewBag.Unregister = "Oj något gick fel, vänligen försök igen. Om problemet är kvar kan du alltid maila oss på xxx@xxx.se.";
                }
            }
            catch (Exception e)
            {
                ViewBag.Unregister = "Oj något gick fel, vänligen försök igen. Om problemet är kvar kan du alltid maila oss på xxx @xxx.se";
                return View();
            }

            return View("UnregistrationComplete");
        }
    }
}