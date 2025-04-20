#!/bin/bash

# Запуск backend
echo "Starting backend API on port 5184..."
cd CigarHelper.Api
dotnet run --launch-profile "http" &
BACKEND_PID=$!

# Запуск frontend
echo "Starting frontend on port 3000..."
cd ../CigarHelper.Web
npm run dev &
FRONTEND_PID=$!

# Функция для корректного завершения процессов
cleanup() {
    echo "Shutting down services..."
    kill $BACKEND_PID
    kill $FRONTEND_PID
    exit 0
}

# Перехват сигналов для корректного завершения
trap cleanup SIGINT SIGTERM

echo "Services started. Press Ctrl+C to stop all services."
wait 