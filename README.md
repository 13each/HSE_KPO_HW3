# HW3 Шелков Андрей Михайлович БПИ2310

## Запуск

```bash
docker-compose up --build
```

После старта поднимутся контейнеры:

| Сервис              | Порт   |
|---------------------|--------|
| Orders Service      | 7001   |
| Payments Service    | 7002   |
| API Gateway (YARP)  | 7000   |
| RabbitMQ (UI)       | 15672  |
| PostgreSQL Orders   | 5432   |
| PostgreSQL Payments | 5433   |

---

## Swagger / Документация

- **Gateway Swagger UI**:  
  http://localhost:7000/swagger

- **Swagger JSON через Gateway**:  
  - http://localhost:7000/swagger/orders/swagger.json  
  - http://localhost:7000/swagger/payments/swagger.json  

---

## HTTP-эндпоинты

### Orders Service (через API Gateway)

| Метод | URL                       | Описание            |
|-------|---------------------------|---------------------|
| GET   | `/orders/api/orders`      | Список заказов      |
| GET   | `/orders/api/orders/{id}` | Детали заказа по ID |
| POST  | `/orders/api/orders`      | Создать заказ       |

### Payments Service (через API Gateway)

| Метод | URL                                         | Описание                  |
|-------|---------------------------------------------|---------------------------|
| POST  | `/accounts/api/accounts`                    | Создать счёт пользователя |
| POST  | `/accounts/api/accounts/{userId}/deposit`   | Пополнить баланс          |
| GET   | `/accounts/api/accounts/{userId}`           | Получить баланс           |

---

## Схема взаимодействия

```text
┌─────────────┐    outbox     ┌───────────┐   events    ┌────────────────┐
│ Orders API  │──────────────>│ RabbitMQ  │────────────>│ Payments API   │
│  (Outbox)   │               │           │  commands   │ (Inbox/Outbox) │
└─────────────┘               └───────────┘             └────────────────┘
      │ HTTP                                           │ HTTP
      ▼                                                ▼
┌─────────────┐                                    ┌───────────┐
│ API Gateway │                                    │ PostgreSQL│
│ YARP+Swagger│                                    │           │
└─────────────┘                                    └───────────┘
```

- **Outbox-pattern**: оба сервиса пишут события в таблицу `OutboxMessages`, фоновые воркеры читают и публикуют их в RabbitMQ.  
- **Inbox-pattern**: фоновые воркеры подписываются на очереди и сохраняют команды/события в свою БД.

---

## Миграции БД

Миграции лежат в:
- `OrdersService.Infrastructure/Migrations`
- `PaymentsService.Infrastructure/Migrations`

Авто-применение при старте (Program.cs):
```csharp
dbContext.Database.Migrate();
```

Ручное обновление:
```bash
cd OrdersService.Api
dotnet ef database update

cd PaymentsService.Api
dotnet ef database update
```

---

## Структура репозитория

```
├── ApiGateway/
├── Common.Messages/
│   ├── Commands
│   └── Events
├── OrdersService/
│   ├── OrdersService.Api/
│   ├── OrdersService.Infrastructure/
│   └── OrdersService.Domain/
├── PaymentsService/
│   ├── PaymentsService.Api/
│   ├── PaymentsService.Infrastructure/
│   └── PaymentsService.Domain/
├── docker-compose.yml
└── README.md
```
