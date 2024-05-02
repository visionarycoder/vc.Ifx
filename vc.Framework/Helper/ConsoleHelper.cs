namespace vc.Framework.Helper;

public static class ConsoleHelper
{

    public static decimal GetDecimalInput()
    {

        do
        {
            var rawInput = Console.ReadLine();
            var trimmedInput = rawInput?.Trim();
            if (decimal.TryParse(trimmedInput, out var value))
            {
                return value;
            }
            Console.WriteLine("Invalid input.  Please try again.");
        } while (true);

    }

    public static int GetIntegerInput()
    {

        do
        {
            var rawInput = Console.ReadLine();
            var trimmedInput = rawInput?.Trim();
            if (int.TryParse(trimmedInput, out var value))
            {
                return value;
            }
            Console.WriteLine("Invalid input.  Please try again.");
        } while (true);

    }

    public static string GetStringInput()
    {

        do
        {
            var rawInput = Console.ReadLine();
            var trimmedInput = rawInput?.Trim().ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(trimmedInput))
            {
                return trimmedInput;
            }
            Console.WriteLine("Invalid input.  Please try again.");
        } while (true);

    }

    public static void ShowIntroduction(string appName, int separateWidth = 72)
    {
        PrintVisualSeparator(separateWidth);
        Console.WriteLine($"--");
        Console.WriteLine($"-- {appName}");
        Console.WriteLine($"--");
        PrintVisualSeparator(separateWidth);
    }

    public static void ShowExit(int separateWidth = 72)
    {
        PrintVisualSeparator();
        Console.WriteLine("Hit [ENTER] to exit.");
        PrintVisualSeparator();
        Console.ReadLine();
    }

    public static void PrintVisualSeparator(int width = 72)
    {
        Console.WriteLine("".PadRight(width, '-'));
    }

}