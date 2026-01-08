#!/bin/bash

set -e

root_dir=$(git rev-parse --show-toplevel)
output_dir="$root_dir/artifacts/nuget"

mkdir -p "$output_dir"

for project in "$root_dir"/libraries/*/*.csproj; do
  dotnet pack "$project" -c Release -o "$output_dir"
done
