@echo off
echo Запуск скрипта импорта сигар из CSV файла...
echo.

REM Проверяем, передан ли путь к CSV файлу
if "%~1"=="" (
    echo Используется файл по умолчанию: cigarday.csv
    set CSV_FILE=cigarday.csv
) else (
    echo Используется файл: %1
    set CSV_FILE=%1
)

REM Проверяем существование файла
if not exist "%CSV_FILE%" (
    echo Ошибка: Файл %CSV_FILE% не найден!
    echo Убедитесь, что CSV файл находится в текущей директории или укажите полный путь.
    pause
    exit /b 1
)

echo.
echo Начинаем импорт...
echo.

REM Запускаем скрипт
dotnet run --project CigarHelper.Import/CigarHelper.Import.csproj "%CSV_FILE%"

echo.
echo Импорт завершен!
pause 