using System;

using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Helpers
{

    /// <summary>
    /// Helper class to analyze project names and determine their architectural characteristics.
    /// This analyzer uses naming conventions and layer prefixes to classify projects and enforce
    /// volatility-based dependency rules across the codebase.
    /// </summary>
    public static class ProjectAnalyzer
    {
        /// <summary>
        /// Determines the project type from its name using standard naming conventions.
        /// 
        /// Naming conventions:
        /// - *.Contract → Contract (Stable)
        /// - *.Service → Service (Volatile)
        /// - *.Orm → ORM data models (Moderate)
        /// - Ifx.* or *.Ifx.* → Infrastructure (Very Stable)
        /// - *.WebApi → Web API endpoints (Very Volatile)
        /// - *.AzureFunctions → Azure Functions (Very Volatile)
        /// - *.Benchmarks → Performance benchmarks (Excluded from analysis)
        /// - *Test* or *.Tests.* → Test project (Excluded from analysis)
        /// 
        /// Projects not matching any convention return Unknown.
        /// </summary>
        /// <param name="projectName">The project assembly name to classify</param>
        /// <returns>The determined ProjectType, or Unknown if classification cannot be determined</returns>
        public static ProjectType GetProjectType(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                return ProjectType.Unknown;

            if (projectName.EndsWith(".Contract", StringComparison.OrdinalIgnoreCase))
                return ProjectType.Contract;

            if (projectName.EndsWith(".Service", StringComparison.OrdinalIgnoreCase))
                return ProjectType.Service;

            if (projectName.EndsWith(".Orm", StringComparison.OrdinalIgnoreCase) || projectName.Contains(".Orm.", StringComparison.OrdinalIgnoreCase))
                return ProjectType.Orm;

            if (projectName.StartsWith("Ifx", StringComparison.OrdinalIgnoreCase) || projectName.Contains(".Ifx.", StringComparison.OrdinalIgnoreCase))
                return ProjectType.Infrastructure;

            if (projectName.EndsWith(".WebApi", StringComparison.OrdinalIgnoreCase))
                return ProjectType.WebApi;

            if (projectName.EndsWith(".AzureFunctions", StringComparison.OrdinalIgnoreCase))
                return ProjectType.AzureFunctions;

            if (projectName.EndsWith(".Benchmarks", StringComparison.OrdinalIgnoreCase))
                return ProjectType.Benchmarks;

            if (projectName.Contains("Test", StringComparison.OrdinalIgnoreCase) || projectName.Contains(".Tests.", StringComparison.OrdinalIgnoreCase))
                return ProjectType.Test;

            return ProjectType.Unknown;
        }

        /// <summary>
        /// Determines the architectural layer from the project name using layer prefixes.
        /// 
        /// Layer prefixes:
        /// - Access.* → Access layer (data access and persistence)
        /// - Engine.* → Engine layer (business logic)
        /// - Manager.* → Manager layer (orchestration)
        /// - Client.* → Client layer (presentation and external APIs)
        /// - Ifx.* or *.Ifx.* → Infrastructure layer (cross-cutting concerns)
        /// 
        /// Layers enforce dependency direction: Client → Manager → Engine → Access, with Infrastructure available to all.
        /// </summary>
        /// <param name="projectName">The project assembly name to classify</param>
        /// <returns>The determined Layer, or Unknown if classification cannot be determined</returns>
        public static Layer GetLayer(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                return Layer.Unknown;

            if (projectName.StartsWith("Access.", StringComparison.OrdinalIgnoreCase))
                return Layer.Access;

            if (projectName.StartsWith("Engine.", StringComparison.OrdinalIgnoreCase))
                return Layer.Engine;

            if (projectName.StartsWith("Manager.", StringComparison.OrdinalIgnoreCase))
                return Layer.Manager;

            if (projectName.StartsWith("Client.", StringComparison.OrdinalIgnoreCase))
                return Layer.Client;

            if (projectName.StartsWith("Ifx", StringComparison.OrdinalIgnoreCase) || projectName.Contains(".Ifx.", StringComparison.OrdinalIgnoreCase))
                return Layer.Infrastructure;

            return Layer.Unknown;
        }

        /// <summary>
        /// Gets the volatility level associated with a project type.
        /// Volatility determines which project types can depend on which other types.
        /// Rule: More volatile projects can depend on less volatile (stable) projects,
        /// but not vice versa.
        /// 
        /// Volatility levels (stable to volatile):
        /// - VeryStable (Infrastructure)
        /// - Stable (Contracts)
        /// - Moderate (ORM models, Test, Benchmarks)
        /// - Volatile (Services)
        /// - VeryVolatile (Web APIs, Azure Functions)
        /// </summary>
        /// <param name="projectType">The project type to evaluate</param>
        /// <returns>The volatility level of the project type</returns>
        public static VolatilityLevel GetVolatilityLevel(ProjectType projectType)
        {
            return projectType switch
            {
                ProjectType.Infrastructure => VolatilityLevel.VeryStable,
                ProjectType.Contract => VolatilityLevel.Stable,
                ProjectType.Orm => VolatilityLevel.Moderate,
                ProjectType.Service => VolatilityLevel.Volatile,
                ProjectType.WebApi => VolatilityLevel.VeryVolatile,
                ProjectType.AzureFunctions => VolatilityLevel.VeryVolatile,
                ProjectType.Test => VolatilityLevel.Moderate,
                ProjectType.Benchmarks => VolatilityLevel.Moderate,
                _ => VolatilityLevel.Moderate
            };
        }

        /// <summary>
        /// Checks if a dependency from one project type to another is allowed
        /// based on volatility-based decomposition (VBD) rules.
        /// 
        /// Rules:
        /// - Test and Benchmark projects are exempt and can depend on any project type
        /// - Unknown project types are allowed by default
        /// - General rule: More volatile projects can depend on less volatile (stable) projects
        /// - The more volatile project's volatility must be >= the target project's volatility
        /// </summary>
        /// <param name="from">The project type declaring the dependency</param>
        /// <param name="to">The project type being depended upon</param>
        /// <returns>True if the dependency is allowed, false otherwise</returns>
        public static bool IsDependencyAllowed(ProjectType from, ProjectType to)
        {
            // Test and Benchmark projects can depend on anything
            if (from is ProjectType.Test or ProjectType.Benchmarks)
                return true;

            // Can't determine - allow it
            if (from == ProjectType.Unknown || to == ProjectType.Unknown)
                return true;

            var fromVolatility = GetVolatilityLevel(from);
            var toVolatility = GetVolatilityLevel(to);

            // Volatile can depend on stable
            return fromVolatility >= toVolatility;
        }

        /// <summary>
        /// Checks if a layer dependency is allowed according to the architecture's layer flow rules.
        /// 
        /// Dependency rules:
        /// - Same layer dependencies are allowed (e.g., Access.Advantage → Access.Storage)
        /// - Infrastructure can be used by any layer
        /// - Client can depend on: Manager, Engine, Access
        /// - Manager can depend on: Engine, Access
        /// - Engine can depend on: Access only
        /// - Access cannot depend on Engine or Manager (unidirectional dependency)
        /// 
        /// General flow: Client → Manager → Engine → Access (with Infrastructure available to all)
        /// </summary>
        /// <param name="from">The layer declaring the dependency</param>
        /// <param name="to">The layer being depended upon</param>
        /// <returns>True if the layer dependency is allowed, false otherwise</returns>
        public static bool IsLayerDependencyAllowed(Layer from, Layer to)
        {
            // Same layer is allowed (e.g., Access.Advantage.Service -> Access.Advantage.Contract)
            if (from == to)
                return true;

            // Infrastructure can be used by anyone
            if (to == Layer.Infrastructure)
                return true;

            // Client can depend on Manager, Engine, Access
            if (from == Layer.Client)
                return to is Layer.Manager or Layer.Engine or Layer.Access;

            // Manager can depend on Engine and Access
            if (from == Layer.Manager)
                return to is Layer.Engine or Layer.Access;

            // Engine can depend on Access
            if (from == Layer.Engine)
                return to == Layer.Access;

            // Access should not depend on Engine or Manager
            if (from == Layer.Access)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a human-readable name for a project type that includes its volatility level.
        /// Used in diagnostic messages and reporting.
        /// </summary>
        /// <param name="projectType">The project type to name</param>
        /// <returns>A friendly name with volatility designation</returns>
        public static string GetProjectTypeName(ProjectType projectType)
        {
            return projectType switch
            {
                ProjectType.Contract => "Contract (Stable)",
                ProjectType.Service => "Service (Volatile)",
                ProjectType.Orm => "ORM (Moderate)",
                ProjectType.Infrastructure => "Infrastructure (Very Stable)",
                ProjectType.WebApi => "Web API (Very Volatile)",
                ProjectType.AzureFunctions => "Azure Functions (Very Volatile)",
                ProjectType.Test => "Test",
                ProjectType.Benchmarks => "Benchmarks",
                _ => "Unknown"
            };
        }
    }
}
