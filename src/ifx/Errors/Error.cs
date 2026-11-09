using Ifx.Primitives;

namespace Ifx.Errors;

public record Error(string Name, Code<Error> Code, string Description, IsRecoverableError IsRecoverableError);
