using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HouseRent.Models
{
    public class AdvertiseRequest
    {
        public static string RequestToPlace = "to_place";
        public static string RequestToBook = "to_book";
        public static string RequestOrderFrom = "Исходящие";
        public static string RequestOrderTo = "Входящие";

        public int ID { get; set; }

        public string Type { get; set; }

        public int From { get; set; }

        public int To { get; set; }

        public string Status { get; set; }

        public int AdvID { get; set; }

        public Advertise Adv { get; set; }
    }
}
