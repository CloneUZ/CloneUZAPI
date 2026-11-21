# CloneUZ API: бронирование мест в вагоне

Монолит на ASP.NET Core (.NET 10) + PostgreSQL + Redis.
Что делает: интерактивная схема мест вагона, временный резерв места на 15 минут, защита от двойного бронирования, автоочистка просроченных резервов.

## Архитектура

```
HTTP ─► Controllers ─► Services ─► Repositories ─► PostgreSQL (источник правды, xmin)
                          │
                          └──► RedisSeatLockService ─► Redis (быстрый барьер, TTL 15 мин)
```

| Слой | Файлы |
|---|---|
| Controllers | `Controllers/SeatMapController.cs`, `Controllers/HoldsController.cs` |
| Services | `HoldService`, `ReleaseService`, `SeatMapService`, `ExpiredHoldCleaner` + `ExpiredHoldSweeper` (BackgroundService) |
| Redis | `Services/SeatLockService.cs`: Lua-скрипты захвата и освобождения |
| Repositories | `Data/SeatRepository.cs`, `Data/AppDbContext.cs` (EF Core, миграции) |
| Dev-авторизация | `Infrastructure/DevelopmentAuthHandler.cs`: пользователь берётся из заголовка `X-Debug-User` |

### Как работает защита от двойного бронирования (два уровня)

1. **Redis, быстрый отказ.** Один Lua-скрипт атомарно проверяет все выбранные места и ставит `SET key userId PX 900000` по принципу «всё или ничего». Redis выполняет скрипт однопоточно, поэтому между проверкой и записью никто не вклинится. Если 1000 человек нажмут на одно место, 999 получат `409` ещё в Redis, и до Postgres эти запросы не дойдут.
   Ключ: `seat-lock:{tripId:wagon}:seat`. Hash-tag `{...}` держит все места вагона в одном слоте, поэтому скрипт работает и в Redis Cluster.
2. **PostgreSQL, источник правды.** Строки мест блокируются в одном порядке (без дедлоков), конкурентные изменения отсекаются оптимистичной проверкой `xmin`. Если БД отказала, только что поставленная блокировка в Redis откатывается.
3. **Если Redis упал**, бронирование продолжает работать только через Postgres: в лог пишется warning, корректность не страдает.
4. **Снятие резерва:** сначала БД, потом Redis. Redis-скрипт удаляет ключ, только если он принадлежит этому же пользователю (compare-and-delete).
5. **Просроченные резервы** через 15 минут снимает `ExpiredHoldSweeper` в БД, а ключи в Redis истекают сами по TTL.
6. **Rate limiting:** на `POST /holds` действует лимит 10 запросов за 10 секунд на пользователя. При превышении ответ `429`.

### Живая схема мест (SignalR)

- Хаб: `/hubs/seat-map` (`Realtime/SeatMapHub.cs`). Клиент, открывший вагон, вызывает `JoinWagon(tripId, wagonNumber)` и попадает в группу этого вагона, поэтому события получают только те, кто смотрит этот вагон.
- Событие `SeatsChanged { tripId, wagonNumber, seats, status }` уходит после бронирования (`Reserved`) и после снятия брони (`Available`). Если `seats = null`, клиент перечитывает схему целиком.
- Событие `SeatMapsChanged`: фоновая очистка сняла просроченные брони, клиенты перечитывают схему.
- **Redis backplane:** при нескольких экземплярах API событие публикуется через Redis pub/sub (каналы `cloneuz*`) и доходит до клиентов, подключённых к любому экземпляру.
- Ошибка отправки уведомления не ломает бронирование: событие — побочный эффект, правда хранится в БД.
- Демо: откройте `http://localhost:5042/seat-map.html` в двух вкладках. Клик по месту в одной вкладке мгновенно окрашивает его в другой.

## Запуск

Нужно: .NET 10 SDK, запущенный Docker Desktop, по желанию [k6](https://k6.io).

```bash
# 1. Postgres + Redis
docker compose -f deploy/docker-compose.yml up -d

# 2. API (в Development миграции и демо-данные применяются автоматически)
dotnet run --project src/CloneUZ.Api --launch-profile http
# -> http://localhost:5042
```

Демо-рейс: `11111111-1111-1111-1111-111111111111`, вагоны: 1 (Lux), 2 (Compartment), 3 (Platzkart).

## Ручная проверка (curl / Postman)

```bash
TRIP=11111111-1111-1111-1111-111111111111
ALICE=aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa
BOB=bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb

# Схема мест вагона 2
curl http://localhost:5042/api/trips/$TRIP/wagons/2/seat-map

# Alice резервирует места 5 и 6 -> 201 + expiresAt (+15 мин)
curl -i -X POST http://localhost:5042/api/trips/$TRIP/wagons/2/holds \
  -H "Content-Type: application/json" -H "X-Debug-User: $ALICE" -d '{"seats":[5,6]}'

# Bob пытается взять место 6 -> 409 seats_unavailable
curl -i -X POST http://localhost:5042/api/trips/$TRIP/wagons/2/holds \
  -H "Content-Type: application/json" -H "X-Debug-User: $BOB" -d '{"seats":[6]}'

# Без заголовка X-Debug-User -> 401
# Несуществующее место (999) -> 400; несуществующий вагон -> 404

# Alice снимает резерв -> 204, после этого Bob может забрать место 6
curl -i -X DELETE "http://localhost:5042/api/trips/$TRIP/wagons/2/holds?seats=6" -H "X-Debug-User: $ALICE"
```

Посмотреть блокировки в Redis:

```bash
docker exec -it $(docker ps -qf ancestor=redis:7-alpine) redis-cli --scan --pattern 'seat-lock:*'
docker exec -it $(docker ps -qf ancestor=redis:7-alpine) redis-cli PTTL "seat-lock:{11111111111111111111111111111111:2}:5"
```

Проверка отказоустойчивости: `docker stop` контейнера Redis, затем снова `POST`. Бронирование работает, в логе API появляется warning `Redis seat lock unavailable`.

## Тесты

```bash
dotnet test        # xUnit + Moq, 15 тестов
```

Тесты покрывают валидацию, конфликт в Redis (без похода в БД), откат блокировки Redis при конфликте в БД, деградацию при падении Redis, идемпотентное снятие резерва и очистку просроченных резервов.

## Нагрузочный тест: 1000 одновременных кликов по одному месту

```bash
k6 run load/hold-same-seat.js            # место 7 вагона 2
k6 run -e SEAT=8 load/hold-same-seat.js  # повторный прогон на свободном месте
```

Ожидаемый результат: `hold_201 = 1` (порог в скрипте валит тест, если мест продано больше одного), остальные ответы `409`.
