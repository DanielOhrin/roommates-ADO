using Microsoft.Data.SqlClient;
using Roommates.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roommates.Repositories
{
    internal class RoommateRepository : BaseRepository
    {
        public RoommateRepository(string connectionString) : base(connectionString) { }
        public List<Roommate> GetAll()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Roommate";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Roommate> roommates = new List<Roommate>();

                        while (reader.Read())
                        {
                            roommates.Add(
                                new Roommate
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    RentPortion = reader.GetInt32(reader.GetOrdinal("RentPortion")),
                                    MovedInDate = reader.GetDateTime(reader.GetOrdinal("MoveInDate")),
                                });
                        }

                        return roommates;
                    }
                }
            }
        }
        public Roommate GetById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT m.*, r.Name AS RoomName
                                        FROM Roommate m
                                        LEFT JOIN Room r ON r.Id = m.RoomId
                                        WHERE m.Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        Roommate roommate = null;

                        if (reader.Read())
                        {
                            roommate = new Roommate
                            {
                                Id = id,
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                RentPortion = reader.GetInt32(reader.GetOrdinal("RentPortion")),
                                MovedInDate = reader.GetDateTime(reader.GetOrdinal("MoveInDate")),
                                RoomName = reader.GetString(reader.GetOrdinal("RoomName"))
                            };
                        }
                        return roommate;
                    }
                }
            }
        }
        public string GetByChore(int id) // Returns the FirstName property
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT r.FirstName
                                        FROM Roommate r
                                        LEFT JOIN RoommateChore rC ON r.Id = rC.RoommateId
                                        WHERE ChoreId = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    return (string)cmd.ExecuteScalar(); // The (string) tells it what we are getting back. Otherwise cmd.ExecuteScalar() is seen as an Object
                }
            }
        }
        public List<Roommate> ChoreReport()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT rM.FirstName Name, c.Name Chore
                                        FROM RoommateChore rC 
                                        LEFT JOIN Roommate rM ON rM.Id = rC.RoommateId
                                        LEFT JOIN Chore c ON c.Id = rC.ChoreId";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Roommate> roommates = new List<Roommate>();

                        while (reader.Read())
                        {
                            int nameCol = reader.GetOrdinal("Name");
                            int choreCol = reader.GetOrdinal("Chore");

                            string name = reader.GetString(nameCol);
                            string chore = reader.GetString(choreCol); 

                            // If List<Roommate> (to be returned) does not contain an Object for the current Roommate's name, instantiate one.
                            if (roommates.FirstOrDefault(x => x.FirstName == name) != null)
                            {
                                Chore newChore = new Chore { Name = chore };
                                roommates.FirstOrDefault(x => x.FirstName == name).Chores.Add(newChore);
                            }
                            else
                            {
                                Roommate roommate = new Roommate
                                {
                                    FirstName = name,
                                    Chores = new List<Chore> { new Chore { Name = chore } }
                                };
                                roommates.Add(roommate);
                            }
                        }
                        return roommates;
                    }
                }
            }
        }
        public List<Roommate> Former()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT FirstName First, LastName Last, MoveInDate FROM Roommate WHERE isCurrent = 0";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Roommate> roommates = new List<Roommate>();

                        while (reader.Read())
                        {
                            int firstCol = reader.GetOrdinal("First");
                            int lastCol = reader.GetOrdinal("Last");
                            int moveInCol = reader.GetOrdinal("MoveInDate");

                            Roommate newRoommate = new Roommate
                            {
                                FirstName = reader.GetString(firstCol),
                                LastName = reader.GetString(lastCol),
                                MovedInDate = reader.GetDateTime(moveInCol)
                            };

                            roommates.Add(newRoommate);
                        }

                        return roommates;
                    }
                }
            }
        }
        public Dictionary<string, int> DaysOfResidency()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT FirstName Name,
                                               MoveInDate MoveIn, 
                                               MoveOutDate MoveOut, 
                                               isCurrent 
                                        FROM Roommate";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        Dictionary<string, int> daysByPerson = new Dictionary<string, int>();

                        while (reader.Read())
                        {
                            int nameCol = reader.GetOrdinal("Name");
                            int moveInCol = reader.GetOrdinal("MoveIn");
                            int moveOutCol = reader.GetOrdinal("MoveOut");
                            int isCurrentCol = reader.GetOrdinal("isCurrent");

                            string name = reader.GetString(nameCol);
                            DateTime moveIn = reader.GetDateTime(moveInCol);
                            bool isCurrent = reader.GetInt32(reader.GetOrdinal("isCurrent")) == 1 ? true : false;

                            if (!isCurrent)
                            {
                                DateTime moveOut = reader.GetDateTime(moveOutCol);
                                int days = Convert.ToInt32(Math.Round((moveOut - moveIn).TotalDays)); // Amount of days between moveIn and moveOut (length of residency)
                                daysByPerson.Add(name, days);
                            }
                            else
                            {
                                int days = Convert.ToInt32(Math.Round((DateTime.Now - moveIn).TotalDays)); // Amount of days since moving in (Since they still reside there)
                                daysByPerson.Add(name, days);
                            }
                        }

                        return daysByPerson;
                    }
                }
            }
        }
        public List<Roommate> MonthlyCost()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT FirstName First, 
                                               CONVERT(int, 1000 * (CONVERT(float, RentPortion) / 100)) Rent
                                        FROM Roommate
                                        WHERE isCurrent = 1";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Roommate> roommates = new List<Roommate>();

                        while (reader.Read())
                        {
                            int firstCol = reader.GetOrdinal("First");
                            int rentCol = reader.GetOrdinal("Rent");

                            Roommate newRoommate = new Roommate
                            {
                                FirstName = reader.GetString(firstCol),
                                RentPortion = reader.GetInt32(rentCol)
                            };

                            roommates.Add(newRoommate);
                        }

                        return roommates;
                    }
                }
            }
        }
        public List<Roommate> TotalCost()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT FirstName Name,
                                               MoveInDate MoveIn, 
                                               MoveOutDate MoveOut, 
                                               isCurrent,
											   CONVERT(int, 1000 * (CONVERT(float, RentPortion) / 100)) Rent
                                        FROM Roommate";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Roommate> roommates = new List<Roommate>();

                        while (reader.Read())
                        {
                            int nameCol = reader.GetOrdinal("Name");
                            int moveInCol = reader.GetOrdinal("MoveIn");
                            int moveOutCol = reader.GetOrdinal("MoveOut");
                            int isCurrentCol = reader.GetOrdinal("isCurrent");

                            string name = reader.GetString(nameCol);
                            DateTime moveIn = reader.GetDateTime(moveInCol);
                            DateTime moveOut = reader.GetInt32(isCurrentCol) == 1 ? default(DateTime) : reader.GetDateTime(moveOutCol);

                            Roommate newRoommate = new Roommate
                            {
                                FirstName = name,
                                MovedInDate = moveIn,
                                MovedOutDate = moveOut,
                                RentPortion = reader.GetInt32(reader.GetOrdinal("Rent"))
                            };

                            roommates.Add(newRoommate);
                        }

                        return roommates;
                    }
                }
            }
        }
    }
}
