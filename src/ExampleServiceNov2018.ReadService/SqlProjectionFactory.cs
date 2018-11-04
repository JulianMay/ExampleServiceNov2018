using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace ExampleServiceNov2018.ReadService
{
    
    /// <summary>
    /// Responsible for checking SQL-schema's and composing subscription->projection
    /// </summary>
    public class SqlProjectionFactory
    {
        private string _connectionString;
        private readonly IStreamStore _sqlStreamStore;

        public SqlProjectionFactory(string connectionString, IStreamStore sqlStreamStore)
        {
            _connectionString = connectionString;
            _sqlStreamStore = sqlStreamStore;
        }

        public IEnumerable<SqlProjectionSubscription> WakeReadProjections(ISqlProjection[] projections)
        {
            // Subscriptions are scoped as: 1 instance per scema per database (connectionstring becomes the partition-key)
            using (var connection = SqlExecution.OpenWriteConnection(_connectionString))
            {
                var existingSubscriptions = connection.Query(
                        "SELECT SchemaIdentifier, ReadPosition FROM Inf_ReadSubscriptions")
                    .Select(x => new
                    {
                        SchemaIdentifier = (string) x.SchemaIdentifier,
                        ReadPosition = (long) x.ReadPosition
                    }).ToDictionary(x => x.SchemaIdentifier);
                

                foreach (var projection in projections)
                {
                    if (existingSubscriptions.TryGetValue(projection.SchemaIdentifier, out var state))
                        yield return WakeReadProjection(projection, new SubscriptionState
                        {
                            AlreadyExists = true,
                            ReadPosition = state.ReadPosition
                        }, connection);
                    
                    //it not already existing, register it
                    yield return WakeReadProjection(projection, new SubscriptionState
                    {
                        AlreadyExists = false,
                        ReadPosition = Position.Start
                    }, connection);
                }
                
            }
            
            
        }



        private SqlProjectionSubscription WakeReadProjection(ISqlProjection projection,
            SubscriptionState subscriptionState, SqlConnection connection)
        {
            if(subscriptionState.AlreadyExists == false)
                RegisterSubscriber(projection.SchemaIdentifier, connection);
            
            var subscriber = new SqlProjectionSubscription(_sqlStreamStore, projection, subscriptionState.ReadPosition, _connectionString);
            subscriber.Subscribe();
            return subscriber;
        }

        
        private class SubscriptionState
        {
            public bool AlreadyExists;
            public long ReadPosition;
        }
        
        public static SqlProjectionFactory Prepare(string sqlConnectionString, IStreamStore sqlStreamStore)
        {
            PrepareSubscriptionSchema(sqlConnectionString);
            return new SqlProjectionFactory(sqlConnectionString, sqlStreamStore);
        }

        private static void PrepareSubscriptionSchema(string sqlConnectionString)
        {
            using (SqlConnection connection = SqlExecution.OpenWriteConnection(sqlConnectionString))
                SqlExecution.Run(_subscriberSchemaSetup, connection);
        }

        private void RegisterSubscriber(string schemaIdentifier, SqlConnection conn)
        {
            conn.Execute("INSERT INTO Inf_ReadSubscriptions (SchemaIdentifier, ReadPosition) VALUES (@sch, @rdp)",
                new {sch = schemaIdentifier, rdp = Position.Start});
        }
        
        /// <summary>
        /// Subscriptions are scoped as: 1 instance per scema per database (connectionstring becomes the partition-key)
        /// </summary>
        private const string _subscriberSchemaSetup = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Inf_ReadSubscriptions')
                BEGIN
                    CREATE TABLE dbo.Inf_ReadSubscriptions(
                        SchemaIdentifier  NVARCHAR(250)     NOT NULL,
                        ReadPosition      BIGINT            NOT NULL
                        PRIMARY KEY (SchemaIdentifier)
                    )
                END
        ";
        
    }
}