using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using ExampleServiceNov2018.Application.Queries;

namespace ExampleServiceNov2018.ReadService
{
    public class TodoListReadService : ITodoListReadService
    {
        private readonly string _sqlConnectionstring;

        public TodoListReadService(string sqlConnectionstring)
        {
            _sqlConnectionstring = sqlConnectionstring;
        }

        public Application.Queries.TodoLists ListAll()
        {
            using (var db = new SqlConnection(_sqlConnectionstring))
            {
                var qry = db.QueryMultiple(
                    @"select AggregateId, Name from R_TodoList order by AggregateId
                      select AggregateId, Number, Text, Checked from R_TodoItem order by AggregateId, Number");
                var listInfo = qry.Read().Select(x=>new ListData(x.AggregateId.Trim(),x.Name));
                var itemInfo = qry.Read();

                return MapAllLists(listInfo, itemInfo);
            }
        }
        
        public Application.Queries.TodoLists.List GetByAggregateId(string aggregateId)
        {
            using (var db = new SqlConnection(_sqlConnectionstring))
            {
                var qry = db.QueryMultiple(
                    @"select AggregateId, Name from R_TodoList where AggregateId = @aggregateId
                      select AggregateId, Number, Text, Checked from R_TodoItem where AggregateId = @aggregateId order by Number",
                    new {aggregateId});
                var listInfo = qry.Read().Select(x=>new ListData(x.AggregateId.Trim(),x.Name)).SingleOrDefault();
                if(listInfo.Equals(default(ListData)))
                    throw new ArgumentException($"Readmodel has no notion of a todolist with id '{aggregateId}'");
                
                var itemInfo = qry.Read();

                return MapTodoList(listInfo, (aggId) => itemInfo );
            }
        }

        
        private struct ListData
        {
            public string AggregateId;
            public string Name;

            public ListData(string aggregateId, string name)
            {
                AggregateId = aggregateId;
                Name = name;
            }
        }
        private Application.Queries.TodoLists MapAllLists(IEnumerable<ListData> listsInfo, IEnumerable<dynamic> itemInfo)
        {
            var itemsByAggregate = itemInfo.ToLookup(i => ((string)i.AggregateId).Trim());
            var lists = listsInfo.Select(l=>MapTodoList(l, (aggId) => itemsByAggregate[aggId])).ToArray();
            
            return new Application.Queries.TodoLists
            {
                Collection = lists
            };
        }
        
        Application.Queries.TodoLists.List MapTodoList(ListData listData, Func<string,IEnumerable<dynamic>> itemsResolver)
        {
            var aggId = listData.AggregateId;
            var items = itemsResolver(aggId);
            return new Application.Queries.TodoLists.List
            {
                AggregateId = aggId.Trim(),
                Name = listData.Name,
                Items = items.Select(MapItem).ToArray()
            };
        }

        private Application.Queries.TodoLists.Item MapItem(dynamic itemData)
        {
            return new Application.Queries.TodoLists.Item
            {
                Text = itemData.Text,
                Checked = itemData.Checked
            };
        }

        
        
        
    }
    
    
}