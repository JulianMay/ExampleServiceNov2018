using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediatR;

namespace ExampleServiceNov2018.ReadService
{
    public class TodoReadService :
        IRequestHandler<ListAllItems, TodoListCollectionDTO>,
        IRequestHandler<GetTodoListById, TodoListDTO>
    {
        private readonly string _sqlConnectionstring;

        public TodoReadService(ReadConnection sqlConnection)
        {
            _sqlConnectionstring = sqlConnection.SqlConnectionString;
        }

        public Task<TodoListDTO> Handle(GetTodoListById request, CancellationToken cancellationToken)
        {
            return Task.FromResult(GetByAggregateId(request.AggregateId));
        }


        public Task<TodoListCollectionDTO> Handle(ListAllItems request, CancellationToken cancellationToken)
        {
            return Task.FromResult(ListAll());
        }

        public TodoListCollectionDTO ListAll()
        {
            using (var db = new SqlConnection(_sqlConnectionstring))
            {
                var qry = db.QueryMultiple(
                    @"select AggregateId, Name from R_TodoList order by AggregateId
                      select AggregateId, Number, Text, Checked from R_TodoItem order by AggregateId, Number");
                var listInfo = qry.Read().Select(x => new ListData(((string)x.AggregateId).Trim(), x.Name));
                var itemInfo = qry.Read();

                return MapAllLists(listInfo, itemInfo);
            }
        }

        public TodoListDTO GetByAggregateId(string aggregateId)
        {
            using (var db = new SqlConnection(_sqlConnectionstring))
            {
                var qry = db.QueryMultiple(
                    @"select AggregateId, Name from R_TodoList where AggregateId = @aggregateId
                      select AggregateId, Number, Text, Checked from R_TodoItem where AggregateId = @aggregateId order by Number",
                    new {aggregateId});
                var listInfo = qry.Read().Select(x => new ListData(x.AggregateId.Trim(), x.Name)).SingleOrDefault();
                if (listInfo.Equals(default(ListData)))
                    throw new ArgumentException($"Readmodel has no notion of a todolist with id '{aggregateId}'");

                var itemInfo = qry.Read();

                return MapTodoList(listInfo, aggId => itemInfo);
            }
        }

        private TodoListCollectionDTO MapAllLists(IEnumerable<ListData> listsInfo, IEnumerable<dynamic> itemInfo)
        {
            var itemsByAggregate = itemInfo.ToLookup(i => ((string) i.AggregateId).Trim());
            var lists = listsInfo.Select(l => MapTodoList(l, aggId => itemsByAggregate[aggId])).ToArray();

            return new TodoListCollectionDTO
            {
                Collection = lists
            };
        }

        private TodoListDTO MapTodoList(ListData listData, Func<string, IEnumerable<dynamic>> itemsResolver)
        {
            var aggId = listData.AggregateId;
            var items = itemsResolver(aggId);
            return new TodoListDTO
            {
                AggregateId = aggId.Trim(),
                Name = listData.Name,
                Items = items.Select(MapItem).ToArray()
            };
        }

        private TodoItemDTO MapItem(dynamic itemData)
        {
            return new TodoItemDTO
            {
                Text = itemData.Text,
                Checked = itemData.Checked
            };
        }


        private struct ListData
        {
            public readonly string AggregateId;
            public readonly string Name;

            public ListData(string aggregateId, string name)
            {
                AggregateId = aggregateId;
                Name = name;
            }
        }
    }
}