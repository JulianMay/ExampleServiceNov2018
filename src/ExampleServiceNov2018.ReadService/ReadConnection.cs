namespace ExampleServiceNov2018.ReadService
{
    public class ReadConnection
    {
        public readonly string SqlConnectionString;

        public ReadConnection(string sqlConnectionString)
        {
            SqlConnectionString = sqlConnectionString;
        }
    }
}