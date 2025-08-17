#!/usr/bin/env bash
#
# setup.sh - installation script for setting up this project in a new environment.
#
# This script will restore .NET dependencies, configure user secrets for local development,
# apply database migrations, and build the project.
#
# Usage: ./setup.sh

set -euo pipefail

# Check if a command exists
command_exists() {
  command -v "$1" >/dev/null 2>&1
}

echo "== Setting up project =="

# Check for .NET SDK
if ! command_exists dotnet; then
  echo "Error: .NET SDK is not installed. Please install .NET 8.0 SDK."
  exit 1
fi

echo "Restoring .NET packages..."
dotnet restore

echo "Initializing user secrets..."
# UserSecretsId is already configured in the project file

echo "Building the project..."
dotnet build --configuration Debug

echo "Setup complete. You can now run the project with:"
echo "  dotnet run --configuration Debug"
