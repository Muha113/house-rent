using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace HouseRent.Models
{
    public class Advertise
    {
        public static string StatusConfirmed = "confirmed";
        public static string StatusDeclined = "declined";
        public static string StatusPending = "pending";

        public int ID { get; set; }

        public string Heading { get; set; }

        [Display(Name = "Contact Mail")]
        public string UserMail { get; set; }

        [Display(Name = "Confirmation Status")]
        public string ConfirmationStatus { get; set; }

        [Required]
        [Display(Name = "Contact Number")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Display(Name = "Post Time")]
        public DateTime PostTime { get; set; }

        [Display(Name = "From")]
        [DataType(DataType.Date)]
        public List<RentRange> RentRanges { get; set; } = new List<RentRange>();

        [Required]
        public string Address { get; set; }

        [Display(Name = "Youtube Link")]
        public string YoutubeLink { get; set; }

        [Required]
        [Display(Name = "House Size")]
        public int FlatSize { get; set; }

        [Required]
        [Display(Name = "House Type")]
        public string FlatType { get; set; } 

        [Required]
        [Display(Name = "Available for")]
        public string Category { get; set; } 

        [Required]
        public int Rent { get; set; }

        [Required]
        [Display(Name = "House Details")]
        public string FlatDetails { get; set; }

        [Display(Name = "Utilities Bill")]
        public int UtilitiesBill { get; set; }

        [Display(Name = "Others Bill")]
        public int OtherBill { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<Image> Images { get; set; }

        public virtual ICollection<Review> Reviews { get; set; }

        public virtual ICollection<Compliment> Compliments { get; set; }
    }
}
