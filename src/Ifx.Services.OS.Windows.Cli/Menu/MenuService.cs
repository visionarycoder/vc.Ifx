using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ifx.Services.OS.Windows.Cli.Menu;

public interface IMenuService
{
    void ShowExit();
    void ShowIntroduction(string appName);
    void ShowOptions(string title, IEnumerable<string> options, string prompt = "Select an item: ");
    void ShowSeparator();
}

public class MenuService : IMenuService
{

    private const char Separator = '-';
    private const string Prefix = "--";

    private readonly ILogger<MenuService> logger = NullLogger<MenuService>.Instance;

    public MenuService(ILogger<MenuService> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public MenuService(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory?.CreateLogger<MenuService>() ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public void ShowIntroduction(string appName)
    {
        LogInformation("Showing introduction for {AppName}", appName);
        ShowSeparator();
        Console.WriteLine($"{Prefix}");
        Console.WriteLine($"{Prefix} {appName}");
        Console.WriteLine($"{Prefix}");
        ShowSeparator();

    }

    public void ShowOptions(string title, IEnumerable<string> options, string prompt = "Select an item: ")
    {
        const int offset = 1;
        var columnCount = options.Count() + offset;
        var spacer = new string(' ', columnCount);
        var index = offset;

        Console.WriteLine(title);
        options.ToList().ForEach(options => Console.WriteLine($"{spacer}{index++}: {options}"));
        Console.Write($"{prompt} ");
    }

    public void ShowExit()
    {
        ShowSeparator();
        Console.WriteLine("Hit [ENTER] to exit.");
        ShowSeparator();
        Console.ReadLine();
    }

    public void ShowSeparator()
    {
        Console.WriteLine("".PadRight(Console.WindowWidth - 1, Separator));
    }

}