param (
    [string]$MigrationName = "",
    [string]$ContextName = "SmartContractContext",
    [string]$Environment = "Debug"
)

# Определение пути к проекту в зависимости от контекста
$projectPath = switch ($ContextName) {
    "SmartContractContext" { ".\JTI.Mos.SmartContract.Core.DAL\" }
    "TezisContext" { ".\JTI.Mos.SmartContract.Core.DAL.Tezis\" }
    default {
        Write-Host "Ошибка: Неизвестный контекст '$ContextName'. Допустимые значения: 'SmartContractContext', 'TezisContext'."
        exit 1
    }
}

$OldEnv=$env:ASPNETCORE_ENVIRONMENT
$env:ASPNETCORE_ENVIRONMENT=$Environment
Write-Host "Old Env: $OldEnv; New Env: $Environment"

# Формирование команды для выполнения
$command = "dotnet ef database update --project $projectPath --context $ContextName $MigrationName"

# Выполнение команды
Write-Host "Выполнение команды: $command"
Invoke-Expression $command

# Проверка результата выполнения команды
if ($LASTEXITCODE -eq 0) {
    $MigrationMessage = $MigrationName

    if(-not $MigrationName) {
        $MigrationMessage = "последней версии"
    }

    Write-Host "База данных успешно обновлена до '$MigrationMessage'."
} else {
    Write-Host "Ошибка при создании миграции '$MigrationMessage'."
    exit $LASTEXITCODE
}

$env:ASPNETCORE_ENVIRONMENT=$OldEnv