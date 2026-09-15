namespace Ifx.Abstractions;

public interface ITracer
{
    ISpan StartSpan(string name);
}