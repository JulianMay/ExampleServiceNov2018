using System;
using ExampleServiceNov2018.Domain.Commands;
using MediatR;

namespace ExampleServiceNov2018.Application
{
    /// <summary>
    /// Wrapper for command-requests, to keep Mediatr-dependdency out of Domain
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Cmd<T> : IRequest<CommandResult> where T : class
    {
        public readonly T Command;

        public Cmd(T command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <summary>
        /// Because constructors can't infer generic type of argument...
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static Cmd<T> Envelope(T cmd)
        {
            return new Cmd<T>(cmd);
        }
    }
}