#!/bin/bash

root_dir=$(git rev-parse --show-toplevel)

cd $root_dir

dotnet new uninstall ./templates/IntegrationTemplate
dotnet new uninstall ./templates/NuGetTemplate
