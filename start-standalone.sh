#!/bin/sh
# Note: Line endings must be in LF format
echo "Starting Poultry Farm Manager (Standalone Mode)"

if [ $# -gt 0 ]; then
    echo "Running WebAPI with arguments: $@"
else
    echo "Running WebAPI with no arguments"
fi

# Check if running in migration-only mode (migrate --exit)
MIGRATE_ONLY=false
if echo "$@" | grep -q "migrate" && echo "$@" | grep -q "\-\-exit"; then
    MIGRATE_ONLY=true
    echo "Running in migration-only mode (will exit after completion)"
fi

echo "Starting WebAPI..."
cd /webapi
dotnet PoultryFarmManager.WebAPI.dll "$@" &
WEBAPI_PID=$!
cleanup() {
    echo "Shutting down WebAPI service (instance args: $@)..."
    kill $WEBAPI_PID 2>/dev/null
}
trap cleanup EXIT TERM INT

# If migration-only mode, wait for process to finish and exit
if [ "$MIGRATE_ONLY" = true ]; then
    wait $WEBAPI_PID
    EXIT_CODE=$?
    echo "Migration completed with exit code: $EXIT_CODE"
    exit $EXIT_CODE
fi

# Normal mode: check if WebAPI started successfully
sleep 5
if ! kill -0 $WEBAPI_PID 2>/dev/null; then
    echo "ERROR: WebAPI failed to start"
    exit 1
fi
echo "WebAPI started successfully (PID: $WEBAPI_PID)"
# # Wait for WebAPI to be ready by polling its health endpoint ---
# max_attempts=30
# attempt=0
# while [ $attempt -lt $max_attempts ]; do
#     if curl -f http://127.0.0.1:5000/health > /dev/null 2>&1; then
#         echo "WebAPI started successfully (PID: $WEBAPI_PID)"
#         break
#     fi
#     attempt=$((attempt + 1))
#     sleep 1
# done
# if [ $attempt -eq $max_attempts ]; then
#     echo "ERROR: WebAPI failed to start within 30 seconds"
#     exit 1
# fi
echo "Starting nginx..."
exec nginx -g "daemon off;"
