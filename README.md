# ExampleServiceNov2018
Trying out a design-approach (CQRS with Sql-persisted readmodels) and techstack (.net Core 2.1, Mediatr, SqlStreamStore, MsSql)

## Setup:
Create an empty SQL database, and adjust the connections string in */src/ExampleServiceNov2018.Api/Startup.cs*

## Run:
Run the *ExampleServiceNov2018.Api*-project, it hosts _(TodoReadServiceHost : IHostedService_ which projects events into a readmodel in SQL
Examples provides, ready for import to PostMan, at */ExampleServiceNov2018.postman_collection.json*

