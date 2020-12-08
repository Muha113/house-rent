using System;

namespace HouseRent.Models
{
    public enum RentStatus : byte
    {
        Pending = 0, // Ожидается подтверждение от владельца
        Rented = 1 // Забронирован
    }

    public class RentRange
    {
        public int ID { get; set; }

        public int AdvertiseID { get; set; }

        public Advertise Advertise { get; set; }

        public DateTime RentFrom { get; set; }

        public DateTime RentTo { get; set; }

        public RentStatus Status { get; set; }
    }
}
