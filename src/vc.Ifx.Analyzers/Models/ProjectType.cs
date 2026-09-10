namespace vc.Ifx.Analyzers.Models
{

    /// <summary>
    /// Project classification based on naming conventions and volatility characteristics.
    /// Used to enforce architecture rules and dependency constraints.
    /// </summary>
    public enum ProjectType
    {
        /// <summary>Unknown project type - cannot determine project classification</summary>
        Unknown,

        /// <summary>Projects ending with .Contract - contain interfaces and DTOs (Stable volatility)</summary>
        Contract,

        /// <summary>Projects ending with .Service - contain service implementations (Volatile volatility)</summary>
        Service,

        /// <summary>Projects ending with .Orm - contain database models and data access (Moderate volatility)</summary>
        Orm,

        /// <summary>Projects starting with Ifx.* or containing .Ifx. - infrastructure/cross-cutting concerns (Very Stable volatility)</summary>
        Infrastructure,

        /// <summary>Projects ending with .WebApi - web API endpoints and controllers (Very Volatile volatility)</summary>
        WebApi,

        /// <summary>Projects ending with .AzureFunctions - Azure Functions implementations (Very Volatile volatility)</summary>
        AzureFunctions,

        /// <summary>Test projects (excluded from architecture analysis) - contain unit or integration tests</summary>
        Test,

        /// <summary>Projects ending with .Benchmarks - performance benchmarks (excluded from architecture analysis)</summary>
        Benchmarks
    }
}
