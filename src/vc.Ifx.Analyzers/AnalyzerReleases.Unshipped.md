; Unshipped analyzer release
; IFX1000 and IFX1001 descriptor releases are tracked in vc.Ifx.Roslyn, their defining assembly.

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
IFX1100 | Maintainability | Warning | Ordinary method control-flow nesting exceeds the configured limit; ifx-control-nesting-v1.
CQ100 | CodeQuality | Warning | Legacy compatibility policy: at most 80 immediate body statements, not physical source lines.
CQ101 | CodeQuality | Warning | Legacy compatibility policy: at most 5 parameters.
CQ102 | CodeQuality | Warning | Legacy compatibility policy: at most 4 enclosing blocks; method body counts as one.
CQ103 | CodeQuality | Warning | Legacy compatibility policy: at most 10 variable declarators.
CQ104 | CodeQuality | Warning | Legacy compatibility syntax metric: at most 10; upstream CA1502 remains the active cyclomatic rule.
IFX001 | Design | Error | Proxy contracts must expose asynchronous signatures.
IFX002 | Refactoring | Warning | Members marked with refactor indicators should be addressed.
IFX003 | Design | Warning | Unused private members should be removed.
IFX004 | Design | Warning | Unused types should be removed.
IFX005 | Design | Info | Controller attributes should follow the framework ordering convention.
IFX006 | Design | Warning | Source files should contain one primary class.
SEC001 | Security | Error | SQL operations should avoid injection-prone construction.
SEC002 | Security | Error | Recognized controller input at logging sinks requires review; not SQL-safety certification.
VBD100 | Architecture.Volatility | Warning | A vault should represent one volatility boundary.
VBD101 | Architecture.Volatility | Warning | Vault types should not leak across boundaries.
VBD102 | Architecture.Volatility | Error | Infrastructure projects should not depend on domain projects.
VBD200 | Architecture | Warning | Manager layer methods should not contain business logic.
VBD201 | Architecture | Warning | Managers should not call other managers.
VBD202 | Architecture.Layers | Warning | Managers should not depend directly on Access projects.
VBD300 | Architecture | Warning | Engines should remain stateless.
VBD301 | Architecture | Error | Engines should not depend on managers.
VBD400 | Architecture | Warning | Access projects should not depend on other Access projects.
VBD500 | Architecture | Warning | Contracts should be immutable.
VBD501 | Architecture.Volatility | Error | Contract projects should not depend on Service projects.
VBD600 | Architecture.Volatility | Warning | Stable components should not depend on more volatile components.
VBD601 | Architecture.Layers | Warning | Layer dependencies should follow the approved direction.
VBD602 | Architecture | Warning | Forbidden dependency directions should be reported.
VBD700 | Architecture.Volatility | Warning | Service projects should not depend on other Service projects.
VBD800 | Architecture.Volatility | Warning | ORM projects should only be referenced by their Service project.
