# Integration Platform

[![Shared](https://img.shields.io/github/actions/workflow/status/rczajkadev/integration-platform/Shared.deploy-infra.yml?label=Shared)](https://github.com/rczajkadev/integration-platform/actions/workflows/Shared.deploy-infra.yml)
[![Google Drive](https://img.shields.io/github/actions/workflow/status/rczajkadev/integration-platform/Gmail.deploy.yml?label=Gmail)](https://github.com/rczajkadev/integration-platform/actions/workflows/Gmail.deploy.yml)
[![Google Drive](https://img.shields.io/github/actions/workflow/status/rczajkadev/integration-platform/GoogleDrive.deploy.yml?label=Google%20Drive)](https://github.com/rczajkadev/integration-platform/actions/workflows/GoogleDrive.deploy.yml)
[![Todoist](https://img.shields.io/github/actions/workflow/status/rczajkadev/integration-platform/Todoist.deploy.yml?label=Todoist)](https://github.com/rczajkadev/integration-platform/actions/workflows/Todoist.deploy.yml)

> Platform for running automated workflows between external services.

## Repository setup

```bash
./init.sh
```

## Add a new integration

From a bash shell:

```bash
./scripts/create_integration.sh <integration-name>
```

## Local NuGet packages

Local restores use `nuget.config` with `artifacts/nuget` as a package source.
Build local packages with:

```bash
./scripts/build_local_nuget.sh
```

For production restores, use `nuget.prod.config`:

```bash
dotnet restore --configfile nuget.prod.config
```

## License

MIT - see [LICENSE](LICENSE).
