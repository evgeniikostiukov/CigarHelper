param (
    [string]$ContextName = "SmartContractContext",
    [string]$Environment = "Debug",
    [bool]$Force = 0
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
$command = "dotnet ef migrations remove --project $projectPath --context $ContextName $(if ($Force -eq 1) {'--force'} Else {''})"

# Выполнение команды
Write-Host "Выполнение команды: $command"
Invoke-Expression $command

# Проверка результата выполнения команды
if ($LASTEXITCODE -eq 0) {
    Write-Host "Миграция успешно удалена."
    $env:ASPNETCORE_ENVIRONMENT=$OldEnv
} else {
    Write-Host "Ошибка при удалении миграции."
    exit $LASTEXITCODE
}