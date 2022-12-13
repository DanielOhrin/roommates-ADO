using Microsoft.Data.SqlClient;

namespace Roommates.Repositories
{
    public class RoommateChoreRepository : BaseRepository
    {
        public RoommateChoreRepository(string connectionString) : base(connectionString) { }

        public void Update(int roommateId, int choreId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE RoommateChore
                                        SET RoommateId = @roommateId
                                        WHERE ChoreId = @choreId";
                    cmd.Parameters.AddWithValue("@roommateId", roommateId);
                    cmd.Parameters.AddWithValue("@choreId", choreId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
