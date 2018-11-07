using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace ExampleServiceNov2018.ReadService
{
    internal class SqlSubscriptionPersistence
    {
        /// <summary>
        /// The last handled position, at initialization-time
        /// </summary>
        internal readonly long? InitialReadPosition;
        private readonly string _commitConnectionstring;
        private readonly ISqlProjection _projection;

        public SqlSubscriptionPersistence(string commitConnectionstring, long? initialReadPosition, ISqlProjection projection)
        {
            InitialReadPosition = initialReadPosition;
            _commitConnectionstring = commitConnectionstring;
            _projection = projection;
        }


        internal async Task<long> CommitToPersistence(StringBuilder dmlCollector, long? position)
        {
            if(position.HasValue == false)
                throw new ArgumentException("Commit's must have a position", nameof(position));
            
            using (var readDb = SqlExecution.OpenWriteConnection(_commitConnectionstring))
            {
                //Consider a write-lock around _dmlCollector, is it necessary?
                //Consider pre-pending a "BEGIN TRANSACTION" and have a all-or-nothing write
                dmlCollector.PrependLine("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; BEGIN TRANSACTION;");
                dmlCollector.AppendLine(
                    $"UPDATE Inf_ReadSubscriptions SET ReadPosition = {position.Value} WHERE SchemaName = '{_projection.SchemaIdentifier.Name}';");
                dmlCollector.AppendLine("COMMIT TRANSACTION;");
                
                var dml = new SqlCommand(dmlCollector.ToString(), readDb);
                var effect = await dml.ExecuteNonQueryAsync();
                
                if (effect == 0)
                    throw new InvalidOperationException(
                        "Something went wrong while updating the state of a readservice. SQL:\r\n" + dmlCollector);

                //todo: Error handling?

                return position.Value;
            }
        
        }
        
        
        
    }

    public static class StringBuilderExtensions
    {
        public static StringBuilder PrependLine(this StringBuilder sb, string content)
        {
            return sb.Insert(0, $"{content}{Environment.NewLine}");
        }
    }
}