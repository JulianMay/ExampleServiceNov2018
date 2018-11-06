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
    internal class SqlProjectionFactory
    {
        private string _connectionString;
        private readonly IStreamStore _sqlStreamStore;

        public SqlProjectionFactory(string connectionString, IStreamStore sqlStreamStore)
        {
            _connectionString = connectionString;
            _sqlStreamStore = sqlStreamStore;
        }

        internal IEnumerable<SqlProjectionSubscription> WakeReadProjections(ISqlProjection[] projections)
        {
            // Subscriptions are scoped as: 1 instance per 'SchemaName' per database (connectionstring becomes the partition-key)
            using (var connection = SqlExecution.OpenWriteConnection(_connectionString))
            {
                var existingSubscriptions = connection.Query(
                        "SELECT SchemaName, SchemaRevision, ReadPosition FROM Inf_ReadSubscriptions")
                    .Select(x => new
                    {
                        SchemaName = (string) x.SchemaName,
                        SchemaRevision = (string) x.SchemaRevision,
                        ReadPosition = (long?) x.ReadPosition
                    }).ToDictionary(x => x.SchemaName);
                

                foreach (var projection in projections)
                {
                    if (existingSubscriptions.TryGetValue(projection.SchemaIdentifier.Name, out var state)
                            && projection.SchemaIdentifier.Revision.Equals(state.SchemaRevision))
                        yield return WakeReadProjection(projection, new SubscriptionState
                        {
                            AlreadyExists = true,
                            ReadPosition = state.ReadPosition
                        }, connection);
                    else
                        //if not already existing (or existing in another revision), (re)register it
                        yield return WakeReadProjection(projection, new SubscriptionState
                        {
                            AlreadyExists = false,
                            ReadPosition = null
                        }, connection);
                }                
            }                        
        }



        private SqlProjectionSubscription WakeReadProjection(ISqlProjection projection,
            SubscriptionState subscriptionState, SqlConnection connection)
        {
            if(subscriptionState.AlreadyExists == false)
                RegisterSubscriber(projection, connection);
            
            var persistence = new SqlSubscriptionPersistence(_connectionString, subscriptionState.ReadPosition, projection);            
            var subscriber = new SqlProjectionSubscription(_sqlStreamStore, projection, persistence);
            
            subscriber.Subscribe();
            return subscriber;
        }

        
        private class SubscriptionState
        {
            public bool AlreadyExists;
            public long? ReadPosition;
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

        private void RegisterSubscriber(ISqlProjection projection, SqlConnection conn)
        {
            conn.Execute(projection.SchemaTeardown);
            foreach (var setupStep in projection.SchemaSetup)
            {
                conn.Execute(setupStep);
            }
            
            conn.Execute("INSERT INTO Inf_ReadSubscriptions (SchemaName, SchemaRevision, ReadPosition) VALUES (@name, @rev, null)",
                new {name = projection.SchemaIdentifier.Name, rev = projection.SchemaIdentifier.Revision});
        }
        
        /// <summary>
        /// Subscriptions are scoped as: 1 instance per scema per database (connectionstring becomes the partition-key)
        /// </summary>
        private const string _subscriberSchemaSetup = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Inf_ReadSubscriptions')
                BEGIN
                    CREATE TABLE dbo.Inf_ReadSubscriptions(
                        SchemaName  NVARCHAR(250)     NOT NULL,
                        SchemaRevision  NVARCHAR(250)     NOT NULL,
                        ReadPosition      BIGINT            NULL
                        PRIMARY KEY (SchemaName)
                    )
                END
        ";
        
    }
}