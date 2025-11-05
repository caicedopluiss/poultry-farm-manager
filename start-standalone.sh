#!/bin/sh
# Note: Line endings must be in LF format
echo "Starting Poultry Farm Manager (Standalone Mode)"
echo "Starting WebAPI..."
cd /webapi
dotnet PoultryFarmManager.WebAPI.dll &
WEBAPI_PID=$!
cleanup() {
    echo "Shutting down WebAPI..."
    kill $WEBAPI_PID 2>/dev/null
}
trap cleanup EXIT TERM INT
# ---
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
