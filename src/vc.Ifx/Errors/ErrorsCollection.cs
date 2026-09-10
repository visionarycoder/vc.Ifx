using Wa.Wsdot.Fin.Idl.Ifx.Generics;

namespace Wa.Wsdot.Fin.Idl.Ifx.Errors;

public sealed class ErrorsReadOnlyCollection : GenericReadOnlyCollection<Error>
{
    public bool HasErrors => items.Count > 0;
    public IsRecoverableError IsRecoverableError => items.All(i => i.IsRecoverableError == IsRecoverableError.Yes) ? IsRecoverableError.Yes : IsRecoverableError.No;

}