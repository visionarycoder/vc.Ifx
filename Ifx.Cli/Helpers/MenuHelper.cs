namespace vc.Ifx.Cli.Helpers;

public static class MenuHelper
{

    public static void ShowIntroduction(string appName, int separateWidth = 72)
    {
        ShowSeparator(separateWidth);
        Console.WriteLine($"--");
        Console.WriteLine($"-- {appName}");
        Console.WriteLine($"--");
        ShowSeparator(separateWidth);
    }

    public static void ShowExit(int separateWidth = 72)
    {
        ShowSeparator();
        Console.WriteLine("Hit [ENTER] to exit.");
        ShowSeparator();
        Console.ReadLine();
    }

    public static void ShowSeparator(int width = 72)
    {
        Console.WriteLine("".PadRight(width, '-'));
    }

}