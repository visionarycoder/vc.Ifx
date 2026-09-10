# Azure Infrastructure

No Azure resources are required for the current framework CI/CD workflow.
[The authoritative GitHub workflow](../../.github/workflows/publish.yml) uses
ephemeral GitHub-hosted runners, GitHub artifacts, GitHub Packages, and NuGet.org.

There are no Bicep deployments, resource groups, storage accounts, Key Vaults,
managed identities, Azure build agents, or cloud credentials to provision for this
pipeline. This directory intentionally contains documentation only. No deployment
was run or requested.

If a future requirement introduces private Azure feeds or self-hosted runners,
design and review that concrete infrastructure separately before adding templates.
The current pipeline and its permissions are documented under
[.infra/yaml](../yaml/README.md).
