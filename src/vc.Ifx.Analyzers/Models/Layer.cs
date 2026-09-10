namespace vc.Ifx.Analyzers.Models
{

    /// <summary>
    /// Logical layer within the WSDOT Financial IDL architecture.
    /// Layers enforce dependency direction: dependencies must flow from Client → Manager → Engine → Access,
    /// with Infrastructure available to all layers.
    /// </summary>
    public enum Layer
    {
        /// <summary>Unknown or unclassified layer</summary>
        Unknown,

        /// <summary>Data access layer - project prefix "Access." - handles database queries and data persistence</summary>
        Access,

        /// <summary>Business logic layer - project prefix "Engine." - contains transformation, validation, and computation logic</summary>
        Engine,

        /// <summary>Orchestration layer - project prefix "Manager." - coordinates between layers and manages workflow</summary>
        Manager,

        /// <summary>Presentation layer - project prefix "Client." - contains UI, controllers, and external API endpoints</summary>
        Client,

        /// <summary>Infrastructure layer - project prefix "Ifx." or contains ".Ifx." - provides cross-cutting utilities available to all layers</summary>
        Infrastructure
    }
}
