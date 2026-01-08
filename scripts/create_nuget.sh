#!/bin/bash

name=$1
root_dir=$(git rev-parse --show-toplevel)

if [ -z "$name" ]; then
  echo "Usage: $0 <package-name>"
  exit 1
fi

cd $root_dir

mkdir ./libraries/$name

dotnet new integration-package -n $name -o ./libraries/$name
dotnet sln add ./libraries/$name/*.csproj -s libraries

mv ./libraries/$name/.github/workflows/* ./.github/workflows
rm -rf ./libraries/$name/.github
