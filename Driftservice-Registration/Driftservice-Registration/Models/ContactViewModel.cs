using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DriftService.Models
{
    public class ContactViewModel
    {
        public int ContactID { get; set; }
        [StringLength(20, ErrorMessage = "Namn kan inte vara längrea än 20 tecken.")]
        [Required(ErrorMessage = "Vänligen ange förnamn.")]
        [Display(Name = "Förnamn")]
        public string FirstName { get; set; }

        [StringLength(20, ErrorMessage = "Namn kan inte vara längrea än 20 tecken.")]
        [Required(ErrorMessage = "Vänligen ange efternamn.")]
        [Display(Name = "Efternamn")]
        public string LastName { get; set; }

        [StringLength(20, ErrorMessage = "Namn kan inte vara längrea än 20 tecken.")]
        [Required(ErrorMessage = "Vänligen ange företag.")]
        [Display(Name = "Företag")]
        public string Business { get; set; }

        [StringLength(30, ErrorMessage = "Namn kan inte vara längrea än 30 tecken.")]
        [Required(ErrorMessage = "Vänligen ange Email.")]
        [EmailAddress(ErrorMessage = "Ogiltig email adress.")]
        [Display(Name = "E-post")]
        public string Email { get; set; }

        [StringLength(20, ErrorMessage = "Mobilnummer kan inte vara längrea än 20 tecken.")]
        [Required(ErrorMessage = "Vänligen ange mobilnummer.")]
        [RegularExpression(@"^\+[0-9]*$", ErrorMessage = "Telefonnumret måste vara numeriskt och börja med en landskod (Ex. +46)")]
        [Display(Name = "Mobilnummer")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Notifikation")]
        public int NotificationType { get; set; }
        public List<ServiceType> ServiceTypeList { get; set; }

        public List<ContactServiceType> ContactServiceTypeList { get; set; }

        //Validation propertys, returns contacts selected value if the value dont pass the validation
        public bool SelectedSms { get; set; }
        public bool SelectedEmail { get; set; }
        public List<int> SelectedServiceTypeList { get; set; }

        public string ContactGuidAsString { get; set; }
        public int Language { get; set; }

    }
}