# Integration Platform


## Setting up Azure Resources and GitHub Secrets

1. Initialize **Azure resources**:
   ```bash
   cd ./shared-infra/
   ./azure_init.sh
   ```

   Follow prompts to create resources. Use `integrations` as project name when asked.
   It generates an `azure-credentials.json` file in the **shared-infra** directory.

2. **Configure secrets** in your GitHub repository:
   - `AZURE_CREDENTIALS`: Content of the `azure-credentials.json`
   - `AZURE_RESOURCE_GROUP`: Your Azure resource group name (default: `rg-integrations`)


## Azure Resources Naming Convention

The standard naming convention for this project is very simple and is based on the following pattern:

`<resource type abbreviation>-<project name>-<integration name>`

The project name is always `integrations` for all resources related to this project.

All resource names should be lowercase. Hyphens are used as separators where supported by the resource type. For resource types with stricter naming rules (such as storage accounts), the same pattern is applied without hyphens.

Examples:

- Todoist azure function app: `func-integrations-todoist`
- Google drive app service plan: `asp-integrations-googledrive`
- YouTube storage account: `stintegrationsyoutube`

For shared resources, the integration name is omitted, resulting in the following pattern:

`<resource type abbreviation>-<project name>`

Examples:

- Shared resource group: `rg-integrations`
- Shared key vault: `kv-integrations`
- Shared storage account: `stintegrations`

The list of resource type abbreviations can be found in the [Abbreviation recommendations for Azure resources](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-abbreviations).


## Shared Key Vault Secrets Naming Convention

To make the project easier to manage, all secrets are stored in a shared Key Vault. That's why keeping a consistent naming convetion is important. All secrets in the shared Key Vault should follow this pattern:

`<IntegrationName>-<SecretPurpose>`

Secret names should be in **PascalCase**, no spaces or special characters, integration names and secret purposes should be seprated by single hyphen.

Examples:

- API Key for Todoist integration: `Todoist-TodoistApiKey`
- Private Key for Google Drive integration: `GoogleDrive-GooglePrivateKey`


## Notes (should be added later to the README content above)

- github workflow files naming convention
- github actions - how it works and how to run them
- kv secrets should be added manually
- github actions to deploy shared resources should be run manually, other on changes in integrations folders or manually when after shared resources deployment
- Update info about shared resource naming convention e.g. `kv-integrations-shared`
- Update info about storage account naming convention - unique postfix
