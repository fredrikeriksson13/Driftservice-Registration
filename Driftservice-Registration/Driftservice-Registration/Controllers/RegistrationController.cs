using DriftService.Context;
using DriftService.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DriftService.Controllers
{
    public class RegistrationController : Controller
    {
        // GET: Contact
        DriftContext db = new DriftContext();

       
        // GET: Contact/Create
        public ActionResult Create()
        {
            ContactViewModel contactViewModel = new ContactViewModel();
            contactViewModel.ServiceTypeList = db.ServiceTypes.ToList();
            contactViewModel.ServiceTypeList.RemoveAll(x => x.PublicServiceType == false);
            contactViewModel.PhoneNumber = "+46";

            if(contactViewModel == null)
            {
                return HttpNotFound();
            }

             return View(contactViewModel);
        }

        //POST: Contact/Create
        [HttpPost]
        public async Task <ActionResult> Create(ContactViewModel contactViewModel, int[] SelectedServiceType)
        {
            try
            {
                if (!ModelState.IsValid || (db.Contacts.Any(x => x.Email == contactViewModel.Email)) || (db.Contacts.Any(x => x.PhoneNumber == contactViewModel.PhoneNumber)) || /*(SelectedServiceType == null) ||*/ (contactViewModel.SelectedSms == false && contactViewModel.SelectedEmail == false))//ha alla vilkor for icke-Godkänd
                {
                    contactViewModel.ServiceTypeList = db.ServiceTypes.ToList();
                    contactViewModel.ServiceTypeList.RemoveAll(x => x.PublicServiceType == false);
                    contactViewModel.SelectedSms = contactViewModel.SelectedSms;
                    contactViewModel.SelectedEmail = contactViewModel.SelectedEmail;
               
                    if(db.Contacts.Any(x => x.Email == contactViewModel.Email))
                    {
                        ModelState.AddModelError("Email","Användare med denna mail är redan registrerad.");
                    }
                    if (db.Contacts.Any(x => x.PhoneNumber == contactViewModel.PhoneNumber))
                    {
                        ModelState.AddModelError("PhoneNumber", "Användare med detta telefonnummer är redan registrerad.");
                    }
                    if(contactViewModel.SelectedSms == false && contactViewModel.SelectedEmail == false)
                    {
                        ViewBag.ErrorMessageNotificationType = "Minst en notifikationstyp måste väljas.";
                    }
                    //If Something dosent validate, return error And selected values

                    if(SelectedServiceType != null)
                    {
                        contactViewModel.SelectedServiceTypeList = SelectedServiceType.ToList();
                    }

                    return View(contactViewModel);
                }

                if (ModelState.IsValid)
                {
                    if (contactViewModel.SelectedEmail == true)
                    {
                        contactViewModel.NotificationType = 1;
                    }
                    if (contactViewModel.SelectedSms == true)
                    {
                        contactViewModel.NotificationType = 2;
                    }
                    if (contactViewModel.SelectedSms == true && contactViewModel.SelectedEmail == true)
                    {
                        contactViewModel.NotificationType = 3;
                    }

                    int ID;
                    if (db.Contacts.Count() != 0) //Gets and sets Contacts ID
                    {
                        ID = (from c in db.Contacts
                              select c.ContactID).Max() + 1;
                    }
                    else // Resets Contact identity 
                    {
                        db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Contacts', RESEED, 0)");
                        db.SaveChanges();
                        ID = 0;
                    }

                    if (SelectedServiceType != null)
                    {
                        foreach (var i in SelectedServiceType)
                        {
                            ContactServiceType contactServiceType = new ContactServiceType();
                            contactServiceType.ContactID = ID;
                            contactServiceType.ServiceTypeID = i;
                            db.ContactServiceTypes.Add(contactServiceType);
                        }
                    }

                    Contact contact = new Contact
                    {
                        FirstName = contactViewModel.FirstName,
                        LastName = contactViewModel.LastName,
                        Business = contactViewModel.Business,
                        Email = contactViewModel.Email,
                        PhoneNumber = contactViewModel.PhoneNumber,
                        NotificationType = contactViewModel.NotificationType,
                        ContactGuid = Guid.NewGuid(),
                        ContactID = ID,
                        RegDate = DateTime.Now,
                    };

                    db.Contacts.Add(contact);
                    db.SaveChanges();
                    await SendEmailToHelpDesk();
                    await SendEmailToNewSubscriber(contact);
                    return View("Succesfull");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                ModelState.AddModelError("", "Det gick inte att spara ändringarna. Vänligen försök igen eller kontakta helpdesk om felet skulle bestå.");
                contactViewModel.ServiceTypeList = db.ServiceTypes.ToList();
                contactViewModel.SelectedSms = contactViewModel.SelectedSms;
                contactViewModel.SelectedEmail = contactViewModel.SelectedEmail;
                if (SelectedServiceType != null)
                {
                    contactViewModel.SelectedServiceTypeList = SelectedServiceType.ToList();
                    contactViewModel.ServiceTypeList.RemoveAll(x => x.PublicServiceType == false);
                }
            }
            return View(contactViewModel);
        }

        private async Task SendEmailToHelpDesk()
        {
            string MailAdressToHelpDesk = System.Configuration.ConfigurationManager.AppSettings["MailAdressToHelpDesk"];
            string LinkToAdminContacts = System.Configuration.ConfigurationManager.AppSettings["LinkToAdminContact"];
            string mailBodyToHelpDesk = "<p>New contact have subscribed for notifications. <br/>Go directly to admin page: " + LinkToAdminContacts + "</p>";

            var messageToHelpDesk = new MailMessage();
            messageToHelpDesk.To.Add(MailAdressToHelpDesk);
            messageToHelpDesk.Subject = "Driftservice: New contact have subscribed";
            messageToHelpDesk.Body = mailBodyToHelpDesk + "<br/><br/><p>Best Regards!<br/> Driftservice<p>";
            messageToHelpDesk.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(messageToHelpDesk);
            }
        }

        private async Task SendEmailToNewSubscriber(Contact contact)
        {
            string unregisterstyle = "'font-size:75%';";
            string queryString = System.Configuration.ConfigurationManager.AppSettings["UnregisterLink"].ToString() + contact.ContactGuid.ToString();
            string unregiserLink = "<a href='" + queryString + "'>";
            string unregister = unregiserLink + "<center><p style=" + unregisterstyle + ">Klicka här för att avregistrera</p></center></a>";

            string singnatur = "<p>Servicedesk +46 (0)660-729 99<br/>Mail:    helpit@coreit.se<br/>Webb: www.coreit.se<br/>Postadress: Box 407, 891 28 Örnsköldsvik<br/>Besöksadress: Hörneborgsvägen 11, 892 50 Domsjö</p>";

            var messageToNewContact = new MailMessage();
            messageToNewContact.To.Add(contact.Email);
            messageToNewContact.Subject = "Driftservice: Tack för din registrering!";
            messageToNewContact.Body = "Hej " + contact.FirstName + "!<br/><br/>" + "<p>Du kommer i fortsättningen bli uppdaterad vid ev. driftstörningar som berör dina tidigare valda områden.<br/><br/></p>" + singnatur + unregister;
            messageToNewContact.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(messageToNewContact);
            }
        }

    }
}
