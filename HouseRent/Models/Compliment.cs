using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HouseRent.Models
{
    public class Compliment
    {
        public int ID { get; set; }

        public int AdvertiseID { get; set; }

        public string Reviewer { get; set; }

        public int Cleanness { get; set; }

        public int Comfort { get; set; }

        public int PriceQuality { get; set; }

        public int Staff { get; set; }
    }
}
