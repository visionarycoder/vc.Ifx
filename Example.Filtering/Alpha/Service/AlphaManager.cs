using Example.Filtering.Alpha.Contact;
using Example.Filtering.Beta.Contract;
using Example.Filtering.Gamma.Contract;

namespace Example.Filtering.Alpha.Service
{
    public class AlphaManager(IBetaEngine betaEngine) : IAlphaManager
    {

    }

}
