using System.Data.Common;

namespace Eyefinity.Utilty.Security.Auditor;

public class SimpleQueryCommandInterceptor : DbCommandInterceptor
{

    private readonly ConsoleHelper consoleHelper;

    public SimpleQueryCommandInterceptor(ConsoleHelper consoleHelper)
    {
        this.consoleHelper = consoleHelper;
    }

    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        // TODO: Add your business logic
        // var dbName = connection.Database;
        // var commandText = command.CommandText;
        
        consoleHelper.ToggleHighlightOn();
        Console.WriteLine($"Database={command.Connection.Database}|Command={command.CommandText}|");
        consoleHelper.ToggleHighlightOff();

        return result;
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        // TODO: Add your business logic
        // var dbName = connection.Database;
        // var commandText = command.CommandText;

        consoleHelper.ToggleHighlightOn();
        Console.WriteLine($"Database: {command.Connection.Database} Command:{command.CommandText}");
        consoleHelper.ToggleHighlightOff();

        return new ValueTask<InterceptionResult<int>>(result);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        // TODO: Add your business logic
        // var dbName = connection.Database;
        // var commandText = command.CommandText;

        consoleHelper.ToggleHighlightOn();
        Console.WriteLine($"Database: {command.Connection.Database} Command:{command.CommandText}");
        consoleHelper.ToggleHighlightOff();
        
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        // TODO: Add your business logic
        // var dbName = connection.Database;
        // var commandText = command.CommandText;

        consoleHelper.ToggleHighlightOn();
        Console.WriteLine($"Database: {command.Connection.Database} Command:{command.CommandText}");
        consoleHelper.ToggleHighlightOff();
        
        return new ValueTask<InterceptionResult<DbDataReader>>(result);
    }

    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        // TODO: Add your business logic
        // var dbName = connection.Database;
        // var commandText = command.CommandText;

        consoleHelper.ToggleHighlightOn();
        Console.WriteLine($"Database: {command.Connection.Database} Command:{command.CommandText}");
        consoleHelper.ToggleHighlightOff();

        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
    {
        // TODO: Add your business logic
        // var dbName = connection.Database;
        // var commandText = command.CommandText;
        
        consoleHelper.ToggleHighlightOn();
        Console.WriteLine($"Database: {command.Connection.Database} Command:{command.CommandText}");
        consoleHelper.ToggleHighlightOff();
        
        return new ValueTask<InterceptionResult<object>>(result);
    }
}