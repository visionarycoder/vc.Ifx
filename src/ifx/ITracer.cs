namespace Ifx;

public interface ITracer
{
    ISpan StartSpan(string name);
}