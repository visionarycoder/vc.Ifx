using System.Globalization;

namespace Ifx.Services.OS.Windows.Cli.Input;

public interface IInputService
{
    (decimal Value, InputResponse InputResponse) GetDecimalInput();
    (int Value, InputResponse InputResponse) GetIntegerInput();
    (string Value, InputResponse InputResponse) GetStringInput();
    (FileInfo Value, InputResponse InputResponse) PromptForInputFile();
    (DirectoryInfo Value, InputResponse InputResponse) PromptForInputFolder();
}

public enum InputResponse
{
    Undefined = 0,
    ExitRequested,
    InvalidInput,
    ValidInput,
}

public class InputService : IInputService
{

    public (decimal Value, InputResponse InputResponse) GetDecimalInput()
    {
        var trimmedInput = GetTrimmedInput();
        var exitRequested = IsExitCommand(trimmedInput);
        if (exitRequested)
        {
            return (0, InputResponse.ExitRequested);
        }
        if (decimal.TryParse(trimmedInput, out var value))
        {
            return (value, InputResponse.ValidInput);
        }
        return (0, InputResponse.InvalidInput);
    }

    public (int Value, InputResponse InputResponse) GetIntegerInput()
    {
        var trimmedInput = GetTrimmedInput();
        var exitRequested = IsExitCommand(input: trimmedInput);
        if (exitRequested)
        {
            return (0, InputResponse.ExitRequested);
        }
        if (int.TryParse(trimmedInput, out var value))
        {
            return (value, InputResponse.ValidInput);
        }
        return (0, InputResponse.InvalidInput);
    }

    public (string Value, InputResponse InputResponse) GetStringInput(bool allowNullOrWhiteSpace)
    {
        var trimmedInput = GetTrimmedInput();
        var exitRequested = IsExitCommand(input: trimmedInput);
        if (exitRequested)
        {
            return (string.Empty, InputResponse.ExitRequested);
        }
        if (allowNullOrWhiteSpace || !string.IsNullOrWhiteSpace(trimmedInput))
        {
            return (trimmedInput, InputResponse.ValidInput);
        }
        return (string.Empty, InputResponse.InvalidInput);
    }

    public (FileInfo Value, InputResponse InputResponse) PromptForFile()
    {
        var path = GetTrimmedInput();
        var fileInfo = new FileInfo(path);
        var responseType = InputResponse.Undefined;
        if (IsExitCommand(path))
        {
            responseType = InputResponse.ExitRequested;
        }
        else if (fileInfo.Exists)
        {
            responseType = InputResponse.ValidInput;
        }
        else
        {
            responseType = InputResponse.InvalidInput;
        }
        return (fileInfo, responseType);
    }

    public (DirectoryInfo Value, InputResponse InputResponse) PromptForFolder()
    {
        var path = GetTrimmedInput();
        var directoryInfo = new DirectoryInfo(path);
        var responseType = InputResponse.Undefined;
        if (IsExitCommand(path))
        {
            responseType = InputResponse.ExitRequested;
        }
        else if (directoryInfo.Exists)
        {
            responseType = InputResponse.ValidInput;
        }
        else
        {
            responseType = InputResponse.InvalidInput;
        }
        return (directoryInfo, responseType);
    }

    private static bool IsExitCommand(string input) => !string.IsNullOrEmpty(input) && input.ToLower(CultureInfo.CurrentCulture) is "exit" or "x" or "q" or "quit";

    private static string? GetTrimmedInput() => Console.ReadLine()?.Trim();

}