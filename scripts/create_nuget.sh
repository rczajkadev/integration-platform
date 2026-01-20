#!/bin/bash

name=$1
root_dir=$(git rev-parse --show-toplevel)

if [ -z "$name" ]; then
  echo "Usage: $0 <package-name>"
  exit 1
fi

cd $root_dir

mkdir ./libraries/Public/$name

dotnet new integration-package -n $name -o ./libraries/Public/$name
dotnet sln add ./libraries/Public/$name/*.csproj -s libraries

mv ./libraries/Public/$name/.github/workflows/* ./.github/workflows
rm -rf ./libraries/Public/$name/.github
