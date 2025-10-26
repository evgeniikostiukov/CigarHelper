# CigarHelper.Import

Консольное приложение для импорта данных о сигарах из CSV файла в базу данных.

## Описание

Этот проект содержит инструменты для массового импорта данных о сигарах из CSV файлов в базу данных CigarHelper. Приложение автоматически:

- Парсит CSV файлы с данными о сигарах
- Создает бренды на основе названий сигар
- Импортирует сигары в таблицу CigarBase
- Обрабатывает дубликаты
- Парсит размеры сигар

## Структура CSV файла

CSV файл должен содержать следующие колонки:
1. `card__link href` - URL страницы сигары
2. `card__image src` - URL изображения сигары
3. `card__size` - Размер сигары (например: "130 мм × 55 RG")
4. `card__title` - Название сигары
5. `card__button` - Цена сигары

## Запуск

### Способ 1: Через bat файл
```bash
# Использование файла по умолчанию (cigarday.csv)
run-import.bat

# Указание конкретного файла
run-import.bat path/to/your/file.csv
```

### Способ 2: Через dotnet
```bash
# Из директории CigarHelper.Import
dotnet run

# С указанием файла
dotnet run path/to/your/file.csv
```

### Способ 3: Из корневой директории
```bash
dotnet run --project CigarHelper.Import/CigarHelper.Import.csproj
dotnet run --project CigarHelper.Import/CigarHelper.Import.csproj path/to/your/file.csv
```

## Настройка подключения к БД

Убедитесь, что в `appsettings.json` настроена строка подключения:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CigarHelper;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

## Поддерживаемые бренды

Скрипт автоматически распознает следующие бренды:
- Hoyo De Monterrey
- Romeo Y Julieta
- H.Upmann
- Cohiba
- Montecristo
- Partagas
- Trinidad
- Davidoff
- И многие другие...

## Логирование

Приложение выводит подробную информацию о процессе импорта:
- Количество найденных сигар
- Созданные бренды
- Прогресс импорта
- Количество импортированных и пропущенных записей
- Ошибки (если есть)

## Зависимости

- CigarHelper.Data - для доступа к моделям и контексту БД
- Microsoft.EntityFrameworkCore.SqlServer - для работы с SQL Server
- Microsoft.Extensions.Hosting - для DI контейнера
- Microsoft.Extensions.Configuration - для конфигурации 