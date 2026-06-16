using System;

namespace Waiting_Reservation.classs
{
    public class TimeItem
    {
        public string   Text       { get; set; }
        public DateTime ExpireTime { get; set; }
        public TimeItem(string text, TimeSpan duration)
        {
            Text       = text;
            ExpireTime = DateTime.Now.Add(duration);
        }
        public override string ToString() => Text;
    }
}
