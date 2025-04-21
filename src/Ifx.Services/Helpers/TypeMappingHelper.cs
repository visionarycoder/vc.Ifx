using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace vc.Ifx.Services.Helpers;

public static class TypeMappingHelper
{
    public static Dictionary<Type, Type> PopulateTypeMap(Type localType, Type remoteType)
    {
        var localAssembly = localType.Assembly ?? throw new InvalidOperationException("Local type must have an assembly.");
        var localNamespace = localType.Namespace ?? throw new InvalidOperationException("Local type must have a namespace.");
        var remoteAssembly = remoteType.Assembly ?? throw new InvalidOperationException("Remote type must have an assembly.");
        var remoteNamespace = remoteType.Namespace ?? throw new InvalidOperationException("Remote type must have a namespace.");

        return PopulateTypeMap(localAssembly, localNamespace, remoteAssembly, remoteNamespace);
    }

    public static Dictionary<Type, Type> PopulateTypeMap(Assembly localAssembly, string localFolderNamespace, [NotNull] Assembly remoteAssembly, string remoteFolderNamespace)
    {
        var typeMap = new Dictionary<Type, Type>();
        var localTypes = localAssembly.GetTypes().Where(t => t.IsClass && t.Namespace == localFolderNamespace).ToList();
        var remoteTypes = remoteAssembly.GetTypes().Where(t => t.IsClass && t.Namespace == remoteFolderNamespace).ToList();

        foreach (var localType in localTypes)
        {
            var remoteType = remoteTypes.FirstOrDefault(t => t.Name == localType.Name);
            if (remoteType != null)
            {
                typeMap[localType] = remoteType;
            }
        }
        return typeMap;
    }
}
