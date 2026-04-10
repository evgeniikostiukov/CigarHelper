# Чеклист безопасности перед выкатом (CigarHelper API)

Источник правды по уже сделанному рефакторингу: [../security-refactor-memory-bank.md](../security-refactor-memory-bank.md) (шаги 1–9 в коде закрыты).  
Этот файл — **операционный список** на момент первого (и последующих) выкатов в прод.

## Конфигурация и секреты

- [ ] `ASPNETCORE_ENVIRONMENT=Production`
- [ ] `ConnectionStrings__DefaultConnection` — реальная строка PostgreSQL (не из репозитория)
- [ ] `Jwt__Key` — криптостойкий секрет (env / secret store), при необходимости `Jwt__Issuer`, `Jwt__Audience`, `Jwt__AccessTokenDays`
- [ ] `AllowedHosts` — реальные публичные хосты API (через env или переопределение JSON), не `*`
- [ ] `Cors:Origins` — явные HTTPS-origins фронта (`Cors__Origins__0`, …), без wildcard при `AllowCredentials`

## За обратным прокси

- [ ] `ForwardedHeaders:Enabled=true` только за доверенным ingress
- [ ] `ForwardedHeaders:KnownProxyAddresses` — **внутренние IP** прокси (не пусто, если прокси не на loopback)
- [ ] Проверить: `RemoteIpAddress`, схема `https`, `Host` в логах/health совпадают с клиентом

## Приложение

- [ ] Заполнить плейсхолдеры в `appsettings.Production.json` или полностью переопределить через env (без коммита секретов)
- [ ] Убедиться, что HSTS и security-заголовки включены в прод-пайплайне (см. `Program.cs`)
- [ ] После выката: smoke-тест логина, CORS с боевого origin, один защищённый эндпоинт

## Связанные эндпоинты наблюдаемости

- `GET /health` — liveness
- `GET /health/ready` — готовность + БД
