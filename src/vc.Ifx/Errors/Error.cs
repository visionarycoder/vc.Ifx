namespace Wa.Wsdot.Fin.Idl.Ifx.Errors;

public record Error(string Name, string Code, string Description, IsRecoverableError IsRecoverableError);