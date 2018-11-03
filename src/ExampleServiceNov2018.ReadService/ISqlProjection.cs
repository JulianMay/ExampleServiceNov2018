using System.Collections.Generic;

namespace ExampleServiceNov2018.ReadService
{
    /// <summary>
    /// The projection-Subcription is responsible for comitting "Apply" scripts at relevant intervals
    /// The projection itself is responsible for generating the "ApplyScript", as well as provide Schema-management hooks
    /// that the projection-factory can use to validate/teardown/setup appropriate sql-schema
    /// </summary>
    public interface ISqlProjection
    {
        /// <summary>
        /// Returns DML for the event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        string Apply(object @event);
        
        /// <summary>
        /// Running numbers collide, let's just name the schema 'something'
        /// </summary>
        string SchemaIdentifier { get; }
        
        /// <summary>
        /// Teardown script to truncate & drop previous (and older) tables
        /// </summary>
        string SchemaTeardown { get; }
        
        /// <summary>
        /// Setup script for the tables owned by this projection
        /// </summary>
        IEnumerable<string> SchemaSetup { get; }
        
        
    }
}