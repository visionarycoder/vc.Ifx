using Example.Filtering.Beta.Contract;
using Example.Filtering.Gamma.Contract;

namespace Example.Filtering.Beta.Service;

internal class BetaEngine(IGammaAccess access) : IBetaEngine
{
}