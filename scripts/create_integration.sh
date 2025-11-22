#!/bin/bash

name=$1
root_dir=$(git rev-parse --show-toplevel)

if [ -z "$name" ]; then
  echo "Usage: $0 <integration-name>"
  exit 1
fi

cd $root_dir

mkdir ./integrations/$name

dotnet new integration -n $name -o ./integrations/$name
dotnet sln add ./integrations/$name/**/*.csproj -s integrations

mv ./integrations/$name/.github/workflows/* ./.github/workflows
rm -rf ./integrations/$name/.github
