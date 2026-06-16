using System;
using System.Collections.Generic;

namespace Waiting_Reservation.classs
{
    public class ReservationItem
    {
        public int            Id             { get; set; }
        public RestaurantType Restaurant     { get; set; }
        public string         RestaurantName { get; set; } = "";
        public string         Name           { get; set; } = "";
        public string         Phone          { get; set; } = "";
        public DateTime       Date           { get; set; }
        public string         Time           { get; set; } = "";
        public int            PartySize      { get; set; }
        public string         Request        { get; set; } = "";
        public DateTime       CreatedAt      { get; set; } = DateTime.Now;
    }

    public static class ReservationStore
    {
        private static int _nextId = 1;
        public static List<ReservationItem> Items { get; } = new List<ReservationItem>();
        public static void Add(ReservationItem item) { item.Id = _nextId++; Items.Add(item); }
        public static void Remove(int id) => Items.RemoveAll(x => x.Id == id);
    }
}
