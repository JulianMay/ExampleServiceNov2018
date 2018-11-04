using System.Collections.Generic;
using ExampleServiceNov2018.Domain.Events;

namespace ExampleServiceNov2018.ReadService
{
    internal class TodoLists : ISqlProjection
    {
        //See TotoReadSchema.sql
        
        public string Apply(object @event)
        {
            switch (@event)
            {
                case TodoListNamed tln: //Using a SP in schema to do 'Upsert' (don't want too much duplication in payload to sql)
                    return $"EXEC R_ChangeTodolistName @aggregateId = '{tln.AggregateId}', @name = '{tln.Name}';";                
                
                //Invariant guaranteed by write: "Only new (numbered) items can be added" &
                //"Only existing(named) todolist can have numbers added" 
                case TodoItemAdded tia:
                    return $@"INSERT INTO R_TodoItem (AggregateId, Number, Text, Checked)
                                    VALUES ('{tia.AggregateId}', {tia.Number},'{tia.Text}', 0);";                
                
                //Invariant guaranteed by write: "Only added items can be checked"
                case TodoItemChecked chkd:
                    return
                        $"UPDATE R_TodoItem SET Checked = 1 WHERE AggregateId = '{chkd.AggregateId}' AND Number = '{chkd.Number}';";
                
                //Invariant guaranteed by write: "Only added items can be checked"
                case TodoItemUnchecked unckd:
                    return
                        $"UPDATE R_TodoItem SET Checked = 0 WHERE AggregateId = '{unckd.AggregateId}' AND Number = '{unckd.Number}';";
            }

            return string.Empty;
        }

        public string SchemaIdentifier => "Initial readmodel, nov. 3rd 2018";
        public string SchemaTeardown => TodoReadSchema.TearDownSchema;
        public IEnumerable<string> SchemaSetup => TodoReadSchema.SetupSchema;
    }
}