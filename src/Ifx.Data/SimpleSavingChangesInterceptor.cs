namespace Eyefinity.Utilty.Security.Auditor;


public class SimpleSavingChangesInterceptor : SaveChangesInterceptor
{

    private readonly ConsoleHelper consoleHelper;

    public SimpleSavingChangesInterceptor(ConsoleHelper consoleHelper)
    {
        this.consoleHelper = consoleHelper;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = new CancellationToken())
    {

        consoleHelper.ToggleHighlightOn();
        Console.WriteLine(eventData.Context.ChangeTracker.DebugView.LongView);
        consoleHelper.ToggleHighlightOff();

        return new ValueTask<InterceptionResult<int>>(result);
    }

}
