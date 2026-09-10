namespace VisionaryCoder.Framework.Pipeline;

public static class Correlation
{

    private static readonly AsyncLocal<string?> id = new();
    public static string? CurrentId
    {
        get => id.Value;
        set => id.Value = value;
    }
}
