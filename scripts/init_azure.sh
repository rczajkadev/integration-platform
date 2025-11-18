#!/bin/bash

az login --use-device-code

CREDENTIALS_FILE="azure-credentials.json"
SUBSCRIPTION_ID=$(az account show --query id --output tsv)

read -p "Enter project name (allowed chars: a-z, 0-9, -, _). Prefixes 'rg-' and 'sp-' will be added automatically: " PROJECT_NAME

while [[ ! $PROJECT_NAME =~ ^[a-zA-Z0-9_-]+$ ]]; do
  echo "Invalid name! Only letters, numbers, hyphens, and underscores allowed."
  read -p "Enter valid resource group name: " PROJECT_NAME
done

RESOURCE_GROUP_NAME="rg-$PROJECT_NAME"
SERVICE_PRINCIPAL_NAME="sp-$PROJECT_NAME"

az group create \
  --name "$RESOURCE_GROUP_NAME" \
  --location polandcentral

echo "Creating service principal..."
SP_INFO=$(az ad sp create-for-rbac \
  --name "$SERVICE_PRINCIPAL_NAME" \
  --role Contributor \
  --scopes "subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP_NAME" \
  --query "{clientId: appId, clientSecret: password, tenantId: tenant, subscriptionId: '$SUBSCRIPTION_ID', resourceGroup: '$RESOURCE_GROUP_NAME'}" \
  --output json)

echo "$SP_INFO" > "$CREDENTIALS_FILE"
chmod 600 "$CREDENTIALS_FILE"

echo -e "\n\033[1;32mAzure resources created successfully!\033[0m"
echo "Resource Group: $RESOURCE_GROUP_NAME"
echo "Service Principal: $SERVICE_PRINCIPAL_NAME"
echo "Credentials saved to: $CREDENTIALS_FILE"
echo -e "\n\033[1;33mWARNING: Keep credentials file secure! Do not commit to version control.\033[0m"
