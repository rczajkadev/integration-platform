#!/bin/bash

set -e

root_dir=$(git rev-parse --show-toplevel)
cache_dir="$root_dir/artifacts/nuget-cache"
local_packages_dir="$root_dir/artifacts/nuget"

rm -rf "$cache_dir"
rm -rf "$local_packages_dir"
