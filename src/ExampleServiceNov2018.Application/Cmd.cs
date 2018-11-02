﻿using System;
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
        public T Command { get; set; }
    }
}