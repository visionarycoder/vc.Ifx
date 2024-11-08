using System.Globalization;

namespace vc.Ifx.Cli.Helpers;

public static class InputHelper
{
    private const string INVALID_INPUT_MESSAGE = "Invalid input.  Please try again.";
    private const string FILE_PROMPT_MESSAGE = "Please enter the path to your file (or type 'exit' to quit):";
    private const string FILE_EMPTY_ERROR_MESSAGE = "File path cannot be empty.";
    private const string FILE_NOT_EXIST_ERROR_MESSAGE = "File does not exist.";
    private const string FOLDER_PROMPT_MESSAGE = "Please enter the path to folder (or x|q|exit to return to the previous menu):";
    private const string FOLDER_EMPTY_ERROR_MESSAGE = "Input Error: Input cannot be empty.";
    private const string FOLDER_NOT_EXIST_ERROR_MESSAGE = "Folder does not exist.";

    public static decimal GetDecimalInput()
    {
        do
        {
            var trimmedInput = GetTrimmedInput();
            if (decimal.TryParse(trimmedInput, out var value))
            {
                return value;
            }
            Console.WriteLine(INVALID_INPUT_MESSAGE);
        } while (true);
    }

    public static int GetIntegerInput()
    {
        do
        {
            var trimmedInput = GetTrimmedInput();
            if (int.TryParse(trimmedInput, out var value))
            {
                return value;
            }
            Console.WriteLine(INVALID_INPUT_MESSAGE);
        } while (true);
    }

    public static string GetStringInput()
    {
        do
        {
            var trimmedInput = GetTrimmedInput()?.ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(trimmedInput))
            {
                return trimmedInput;
            }
            Console.WriteLine(INVALID_INPUT_MESSAGE);
        } while (true);
    }

    public static FileInfo? PromptForInputFile()
    {
        return PromptForPath(FILE_PROMPT_MESSAGE, FILE_EMPTY_ERROR_MESSAGE, FILE_NOT_EXIST_ERROR_MESSAGE, path => new FileInfo(path).Exists ? new FileInfo(path) : null);
    }

    public static DirectoryInfo? PromptForInputFolder()
    {
        return PromptForPath(FOLDER_PROMPT_MESSAGE, FOLDER_EMPTY_ERROR_MESSAGE, FOLDER_NOT_EXIST_ERROR_MESSAGE, path => new DirectoryInfo(path).Exists ? new DirectoryInfo(path) : null);
    }

    private static string? GetTrimmedInput()
    {
        var rawInput = Console.ReadLine();
        return rawInput?.Trim();
    }

    private static T? PromptForPath<T>(string promptMessage, string emptyErrorMessage, string notExistErrorMessage, Func<string, T?> getPathInfoFunc) where T : class
    {
        while (true)
        {
            Console.WriteLine(promptMessage);
            var path = GetTrimmedInput();

            if (IsNullOrEmpty(path))
            {
                Console.WriteLine(emptyErrorMessage);
                continue;
            } 
            
            if (IsExitCommand(path!))
            {
                return null;
            }

            var pathInfo = getPathInfoFunc(path!);
            if (pathInfo != null)
            {
                return pathInfo;
            }
            Console.WriteLine(notExistErrorMessage);
        }
    }

    private static bool IsExitCommand(string input)
    {
        return input.ToLower(CultureInfo.CurrentCulture) is "exit" or "x" or "q";
    }

    private static bool IsNullOrEmpty(string? input)
    {
        return string.IsNullOrEmpty(input);
    }
}