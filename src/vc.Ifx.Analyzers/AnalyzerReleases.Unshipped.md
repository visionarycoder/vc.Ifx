; Unshipped analyzer release

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
IFX1000 | Maintainability | Info | Diagnostic references in comments require a disposition marker.
IFX1001 | Maintainability | Warning | CA#### and CS#### pragma suppressions require an explicit justification.
CQ100 | CodeQuality | Warning | Methods should stay within the configured line-count threshold.
CQ101 | CodeQuality | Warning | Methods should stay within the configured parameter-count threshold.
CQ102 | CodeQuality | Warning | Methods should stay within the configured nesting-depth threshold.
CQ103 | CodeQuality | Warning | Methods should stay within the configured local-variable threshold.
CQ104 | CodeQuality | Warning | Methods should stay within the configured cyclomatic-complexity threshold.
IFX001 | Framework | Error | Proxy contracts must expose asynchronous signatures.
IFX002 | Framework | Warning | Members marked with refactor indicators should be addressed.
IFX003 | Framework | Info | Unused private members should be removed.
IFX004 | Framework | Info | Unused types should be removed.
IFX005 | Framework | Warning | Controller attributes should follow the framework ordering convention.
IFX006 | Framework | Warning | Source files should contain one primary class.
SEC001 | Security | Warning | SQL operations should avoid injection-prone construction.
SEC002 | Security | Warning | Controller logging should not introduce SQL-injection vectors.
VBD100 | Architecture | Warning | A vault should represent one volatility boundary.
VBD101 | Architecture | Warning | Vault types should not leak across boundaries.
VBD102 | Architecture | Warning | Infrastructure projects should not depend on domain projects.
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
