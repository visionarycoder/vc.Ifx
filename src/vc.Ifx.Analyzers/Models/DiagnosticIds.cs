namespace vc.Ifx.Analyzers.Models
{

    /// <summary>
    /// Diagnostic IDs for architecture analyzers
    /// VBD = Volatility-Based Decomposition
    /// </summary>
    public static class DiagnosticIds
    {
        // Infrastructure/Vault rules (VBD100-199)
        public const string Vbd100VaultSingleVolatility = "VBD100";
        public const string Vbd101VaultNoLeakage = "VBD101";
        public const string Vbd102InfrastructureCannotDependOnDomain = "VBD102";

        // Manager layer rules (VBD200-299)
        public const string Vbd200ManagerNoBusinessLogic = "VBD200";
        public const string Vbd201ManagerNoManagerCalls = "VBD201";
        public const string Vbd202ManagerCannotDependOnAccess = "VBD202";

        // Engine rules (VBD300-399)
        public const string Vbd300EngineStateless = "VBD300";
        public const string Vbd301EngineCannotDependOnManager = "VBD301";

        // Interface rules (VBD400-499)
        public const string Vbd400NoAccessToAccessCalls = "VBD400";

        // Contract rules (VBD500-599)
        public const string Vbd500ContractImmutable = "VBD500";
        public const string Vbd501ContractCannotDependOnService = "VBD501";

        // General dependency direction rules (VBD600-699)
        public const string Vbd600StableDependsOnVolatile = "VBD600";
        public const string Vbd601InvalidLayerDependency = "VBD601";
        public const string Vbd602ForbiddenDependencyDirection = "VBD602";

        // Service implementation rules (VBD700-799)
        public const string Vbd700ServiceCannotDependOnOtherService = "VBD700";

        // ORM/Data access rules (VBD800-899)
        public const string Vbd800OrmMustBeReferencedByServiceOnly = "VBD800";

        public const string IncorrectNamespaceForProject = "VBD900";

        // Security rules (SEC001-099)
        public const string Sec001SqlInjectionVector = "SEC001";
        public const string Sec002LoggingSqlInjectionVector = "SEC002";

        // IFX rules
        public const string Ifx001ProxyContractSignature = "IFX001";
        public const string Ifx002RefactorRequired = "IFX002";
        public const string Ifx003UnusedPrivateMember = "IFX003";
        public const string Ifx004UnusedType = "IFX004";
        public const string Ifx005ControllerAttributeOrder = "IFX005";
        public const string Ifx006SingleClassPerFile = "IFX006";

        // Code Quality
        public const string Cq100MethodTooLong = "CQ100";
        public const string Cq101TooManyParameters = "CQ101";
        public const string Cq102TooDeepNesting = "CQ102";
        public const string Cq103TooManyLocals = "CQ103";
        public const string Cq104TooComplex = "CQ104";

    }
}
