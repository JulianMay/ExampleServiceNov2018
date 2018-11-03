using System.Collections.Generic;

namespace ExampleServiceNov2018.ReadService
{
    public static class TodoReadSchema
    {
        public static IEnumerable<string> SetupSchema = new[]{
	//Todo: use different schema than 'dbo', rather than prepending 'R_' to tablenames...
	@"
CREATE TABLE dbo.R_TodoList(
        AggregateId         CHAR(42)            NOT NULL,
        Name          NVARCHAR(1000)            NOT NULL,        
        PRIMARY KEY (AggregateId)
    );",@"
CREATE TABLE dbo.R_TodoItem(
		AggregateId         CHAR(42)            NOT NULL,
		Number				INT					NOT NULL,
		[Text]		  NVARCHAR(1000)			NOT NULL,
		Checked				BIT					NOT NULL,
		PRIMARY KEY (AggregateId, Number)
);",@"
CREATE PROCEDURE R_ChangeTodolistName
@aggregateId CHAR(42),
@name NVARCHAR(1000)
AS
BEGIN
	IF (NOT EXISTS(SELECT * FROM R_TodoList WHERE AggregateId = @aggregateId))
		BEGIN 
			INSERT INTO R_TodoList(AggregateId, Name) 
			VALUES(@aggregateId, @name) 
		END 
		ELSE 
		BEGIN 
			UPDATE R_TodoList 
			SET Name = @name
			WHERE AggregateId = @aggregateId
		END 
END"};

	    public const string TearDownSchema = @"
IF EXISTS (SELECT * FROM sysobjects WHERE name = 'R_ChangeTodolistName' AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE R_ChangeTodolistName
END

IF EXISTS (SELECT * FROM sysobjects WHERE name = 'R_TodoItem')
BEGIN
	DROP TABLE R_TodoItem
END

IF EXISTS (SELECT * FROM sysobjects WHERE name = 'R_TodoList')
BEGIN
	DROP TABLE R_TodoLists
END

";
    }
}