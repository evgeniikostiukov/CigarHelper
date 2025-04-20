@echo off
echo Starting development environment...

:: Запуск backend
echo Starting backend API on port 5184...
start cmd /c "cd CigarHelper.Api && dotnet run --launch-profile http"

:: Даем время API запуститься
timeout /t 5

:: Запуск frontend
echo Starting frontend on port 3000...
start cmd /c "cd CigarHelper.Web && npm run dev"

echo Services started. Close the terminal windows to stop the services. 