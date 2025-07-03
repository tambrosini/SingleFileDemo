#!/bin/bash

# Script to run all microservices concurrently
# Make sure all .cs files are executable before running this script

echo "Starting all microservices..."
echo "================================"

# Array to store background process PIDs
pids=()

# Function to cleanup background processes on script exit
cleanup() {
    echo ""
    echo "Stopping all services..."
    for pid in "${pids[@]}"; do
        if kill -0 "$pid" 2>/dev/null; then
            echo "Stopping process $pid"
            kill "$pid" 2>/dev/null
        fi
    done
    echo "All services stopped."
    exit 0
}

# Set up trap to cleanup on Ctrl+C or script exit
trap cleanup SIGINT SIGTERM EXIT

# Start each service in the background
echo "Starting fixture-service..."
./fixture-service.cs &
pids+=($!)

echo "Starting match-dashboard-service..."
./match-dashboard-service.cs &
pids+=($!)

echo "Starting officials-service..."
./officials-service.cs &
pids+=($!)

echo "Starting teamsheet-service..."
./teamsheet-service.cs &
pids+=($!)

echo ""
echo "All services started! PIDs: ${pids[*]}"
echo "Press Ctrl+C to stop all services"
echo "================================"

# Wait for all background processes
wait
