namespace Ifx;

public interface ISerializer
{
    string Serialize<T>(T value);
    T Deserialize<T>(string json);
}
