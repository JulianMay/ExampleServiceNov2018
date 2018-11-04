using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ExampleServiceNov2018.Domain.Commands;
using static ExampleServiceNov2018.Domain.TodoList;

namespace ExampleServiceNov2018.Application
{
    /// <summary>
    /// Unwraps routing-commandse
    /// </summary>
    public class TodoCommandHandler : 
        IRequestHandler<Cmd<NameTodoList>,CommandResult>,
        IRequestHandler<Cmd<AddTodoItem>,CommandResult>,
        IRequestHandler<Cmd<CheckTodoItem>,CommandResult>,
        IRequestHandler<Cmd<UncheckTodoItem>,CommandResult>
    {
        private readonly ITodoListRepository _lists;

        public TodoCommandHandler(ITodoListRepository lists)
        {
            _lists = lists;
        }

        public Task<CommandResult> Handle(Cmd<NameTodoList> request, CancellationToken cancellationToken)
            => Execute(request.Command);

        public Task<CommandResult> Handle(Cmd<AddTodoItem> request, CancellationToken cancellationToken)
            => Execute(request.Command);

        public Task<CommandResult> Handle(Cmd<CheckTodoItem> request, CancellationToken cancellationToken)
            => Execute(request.Command);

        public Task<CommandResult> Handle(Cmd<UncheckTodoItem> request, CancellationToken cancellationToken)
            => Execute(request.Command);


        private async Task<CommandResult> Execute(Command cmd)
        {
            var aggregate = await _lists.Load(cmd.AggregateId);
            aggregate = Write(cmd, aggregate);
            var newRevision = await _lists.Save(aggregate);
            return new CommandResult.Handled
            {
                AggregateId = aggregate.Id,
                Revision = newRevision
            };
        }
        
    }
}