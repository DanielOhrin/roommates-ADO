using System;
using System.Collections.Generic;

namespace Roommates.Models
{
    // C# representation of the Roommate table
    public class Roommate
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int RentPortion { get; set; }
        public DateTime MovedInDate { get; set; }
        public DateTime MovedOutDate { get; set; }
        public Room Room { get; set; }
        public string RoomName { get; set; }
        public List<Chore> Chores { get; set; }
        public int MonthlyRent => Convert.ToInt32(Convert.ToDouble(RentPortion) * 1000);
        public int TotalRent
        {
            get
            {
                if (!(default(DateTime) == MovedOutDate))
                {
                    return Convert.ToInt32(Math.Round((MovedOutDate - MovedInDate).TotalDays)) / 30 * MonthlyRent; 
                }
                else
                {
                    return Convert.ToInt32(Math.Round((DateTime.Now - MovedInDate).TotalDays)) / 30 * MonthlyRent;
                }
            }
        }
    }
}
