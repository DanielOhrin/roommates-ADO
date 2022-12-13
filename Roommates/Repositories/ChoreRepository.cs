using Microsoft.Data.SqlClient;
using Roommates.Models;
using System;
using System.Collections.Generic;

namespace Roommates.Repositories
{
    internal class ChoreRepository : BaseRepository
    {
        // When the new RoomRepository is instantiated, pass the connection string along to the BaseRepository??
        public ChoreRepository(string connectionString) : base(connectionString){ }
        public List<Chore> GetAll()
        {
            // You are creating a connection tunnel here
            using (SqlConnection conn = Connection)
            {
                // OPENING the tunnel
                conn.Open();

                // Creating a command to query your DB
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Chore";

                    // Need a READER to execute the command AND *READ* the response
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Chore> chores = new List<Chore>();

                        // Read() returns true if there's more data to read!
                        while (reader.Read())
                        {
                            // The ordinal is the numeric position of the column in the query results.
                            // For our query, "Id" has an ordinal value of 0 and "Name" is 1
                            int idColumnPosition = reader.GetOrdinal("Id");

                            // We user the reader's GetXXX methods to get the values for a particular ordinal
                            int idValue = reader.GetInt32(idColumnPosition);

                            int nameColumnPosition = reader.GetOrdinal("Name");
                            string nameValue = reader.GetString(nameColumnPosition);

                            Chore chore = new Chore
                            {
                                Id = idValue,
                                Name = nameValue
                            };

                            chores.Add(chore);
                        }
                        return chores;
                    }
                }
            }
        }
        public Chore GetById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Chore WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        Chore chore = null;

                        // If we only expect a single row back from the database, we do NOT need a while loop
                        if (reader.Read())
                        {
                            chore = new Chore
                            {
                                Id = id,
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            };
                        }
                        return chore;
                    }
                }
            }
        }
        public void Insert(Chore chore)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Chore (Name)
                                                OUTPUT INSERTED.Id
                                                VALUES (@name)";

                    cmd.Parameters.AddWithValue("@name", chore.Name);
                    int id = (int)cmd.ExecuteScalar();

                    chore.Id = id;
                }
            }
        }
        public List<Chore> GetUnassigned()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT DISTINCT c.*
                                        FROM Chore c
                                        WHERE c.Id NOT IN(
                                            SELECT DISTINCT ChoreId
                                            FROM RoommateChore
                                        )";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Chore> chores = new List<Chore>();

                        while (reader.Read())
                        {
                            int idPosition = (int)reader.GetOrdinal("Id");
                            int namePosition = (int)reader.GetOrdinal("Name");

                            chores.Add(
                            new Chore
                            {
                                Id = reader.GetInt32(idPosition),
                                Name = reader.GetString(namePosition)
                            });
                        }
                        return chores;
                    }
                }
            }
        }
        public List<Chore> GetAssigned()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT DISTINCT c.Id, c.Name
                                       FROM Chore c
                                       WHERE c.Id IN (
                                            SELECT DISTINCT ChoreId
                                            FROM RoommateChore
                                       )";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Chore> chores = new List<Chore>();

                        while (reader.Read())
                        {
                            int idCol = reader.GetOrdinal("Id");
                            int nameCol = reader.GetOrdinal("Name");

                            chores.Add(
                                new Chore
                                {
                                    Id = reader.GetInt32(idCol),
                                    Name = reader.GetString(nameCol)
                                }
                            );
                        }
                        return chores;
                    }
                }
            }
        }
        public bool AssignChore(int choreId, int roommateId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO RoommateChore (RoommateId, ChoreId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@roommateId, @choreId)";

                    cmd.Parameters.AddWithValue("@roommateId", roommateId);
                    cmd.Parameters.AddWithValue("@choreId", choreId);

                    int? id = (int)cmd.ExecuteScalar();
                    return id == null ? false : true;
                }
            }
        }
        public Dictionary<string, int> GetChoreCounts()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT r.FirstName, COUNT(ChoreId) AS Chores
                                        FROM Roommate r
                                        LEFT JOIN RoommateChore rC ON rC.RoommateId = r.Id
                                        GROUP BY r.FirstName, r.Id";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        Dictionary<string, int> choresByName = new Dictionary<string, int>();

                        int nameCol = (int)reader.GetOrdinal("FirstName");
                        int countCol = (int)reader.GetOrdinal("Chores");
                        
                        while (reader.Read()) 
                        {

                            choresByName.Add(reader.GetString(nameCol), reader.GetInt32(countCol));
                        }

                        return choresByName;
                    }
                }
            }
        }
        public void Update(Chore chore)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Chore
                                        SET Name = @name
                                        WHERE Id = @id";

                    cmd.Parameters.AddWithValue("@name", chore.Name);
                    cmd.Parameters.AddWithValue("@id", chore.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"DELETE Chore
                                        WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
