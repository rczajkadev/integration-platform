#!/bin/bash

root_dir=$(git rev-parse --show-toplevel)

cd $root_dir

dotnet new install ./templates/IntegrationTemplate
dotnet new install ./templates/NuGetTemplate
