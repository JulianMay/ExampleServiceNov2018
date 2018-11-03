using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ExampleServiceNov2018.ReadService
{
    
    /// <summary>
    /// Responsible for checking SQL-schema's and composing subscription->projection
    /// </summary>
    public class SqlProjectionFactory
    {
        private string _connectionString;

        public SqlProjectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<SqlProjectionSubscription> WakeReadProjections(ISqlProjection[] projections)
        {
            using (var connection = SqlExecution.OpenWriteConnection(_connectionString))
            {
                throw new NotImplementedException();
            }
            
            
        }



        private SqlProjectionSubscription WakeReadProjection(ISqlProjection projection,
            SubscriptionState subscribtionState)
        {
            throw new NotImplementedException();
        }

        
        private class SubscriptionState
        {
            public bool AlreadyExists;
            public long ReadPosition;
        }
        
        public static SqlProjectionFactory Prepare(string sqlConnectionString)
        {
            PrepareSubscriptionSchema(sqlConnectionString);
            return new SqlProjectionFactory(sqlConnectionString);
        }

        private static void PrepareSubscriptionSchema(string sqlConnectionString)
        {
            using (SqlConnection connection = SqlExecution.OpenWriteConnection(sqlConnectionString))
                SqlExecution.Run(_subscriberSchemaSetup, connection);
        }
        
        
        
        /// <summary>
        /// Subscriptions are scoped as: 1 instance per scema per database (connectionstring becomes the partition-key)
        /// </summary>
        private const string _subscriberSchemaSetup = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'R_ReadSubscriptions')
                BEGIN
                    CREATE TABLE dbo.R_TodoList(
                        SchemaIdentifier  NVARCHAR(250)     NOT NULL,
                        ReadPosition      BIGINT            NOT NULL
                        PRIMARY KEY (SchemaIdentifier)
                    )
                END
        ";
        
    }
}