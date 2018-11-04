using System.Data.SqlClient;

namespace ExampleServiceNov2018.ReadService
{
    internal class SqlExecution
    {
        public static void Run(string script, SqlConnection connection)
        {
            var cmd = new SqlCommand(script, connection);
            cmd.ExecuteNonQuery();
        }


        /// <summary>
        ///     Remember to call this in a 'using' decleration
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static SqlConnection OpenWriteConnection(string connectionString)
        {
            var conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }
    }
}