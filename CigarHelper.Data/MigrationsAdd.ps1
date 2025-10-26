param (
    [string]$MigrationName,
    [string]$ContextName = "SmartContractContext",
    [string]$Environment = "Debug"
)

# Проверка, что название миграции было передано
if (-not $MigrationName) {
    Write-Host "Ошибка: Необходимо указать название миграции."
    exit 1
}

$OldEnv=$env:ASPNETCORE_ENVIRONMENT
$env:ASPNETCORE_ENVIRONMENT=$Environment
Write-Host "Old Env: $OldEnv; New Env: $Environment"

# Определение пути к проекту в зависимости от контекста
$projectPath = switch ($ContextName) {
    "SmartContractContext" { ".\JTI.Mos.SmartContract.Core.DAL\" }
    "TezisContext" { ".\JTI.Mos.SmartContract.Core.DAL.Tezis\" }
    default {
        Write-Host "Ошибка: Неизвестный контекст '$ContextName'. Допустимые значения: 'SmartContractContext', 'TezisContext'."
        exit 1
    }
}

# Формирование команды для выполнения
$command = "dotnet ef migrations add $MigrationName --project $projectPath --context $ContextName"

# Выполнение команды
Write-Host "Выполнение команды: $command"
Invoke-Expression $command

# Проверка результата выполнения команды
if ($LASTEXITCODE -eq 0) {
    Write-Host "Миграция '$MigrationName' успешно создана."
    $env:ASPNETCORE_ENVIRONMENT=$OldEnv
} else {
    Write-Host "Ошибка при создании миграции '$MigrationName'."
    exit $LASTEXITCODE
}