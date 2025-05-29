#!/bin/bash
set -e

echo "pgadmin setup started"
echo "Sourcing environment variables..."

echo "Generating files..."
SCRIPT_DIR=$(dirname "$0")
OUTPUT_DIR="${SCRIPT_DIR}/output"

mkdir -p "${OUTPUT_DIR}"
export $(grep -v '^#' .env | grep -v '^$' | xargs)
envsubst < "${SCRIPT_DIR}/servers.json.template" > "${OUTPUT_DIR}/servers.json"

echo "pgadmin setup complete"
