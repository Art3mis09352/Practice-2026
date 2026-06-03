# Wayfarer Backend

Backend-часть проекта Wayfarer, разработанная на ASP.NET Core Web API. Проект закрывает основные сценарии сервиса маршрутов и точек интереса: аутентификацию, роли пользователей, каталог точек, модерацию, конструктор маршрутов, хранение фото и документов, интеграцию с 2ГИС и мониторинг инфраструктуры.

## Основные возможности

- регистрация, авторизация и подтверждение email
- роли `User`, `Owner` и `Admin`
- работа с маршрутами, днями маршрута и лайками
- добавление в маршрут как каталожных точек, так и кастомных route-only точек
- каталог точек с модерацией через `Pending / Approved / Rejected`
- загрузка фото и документов в MinIO
- proxy-эндпоинт к 2ГИС `GET /api/Places/nearby`
- мониторинг PostgreSQL через `postgres-exporter`, Prometheus и Grafana

## Технологический стек

| Компонент | Что используется |
|---|---|
| Backend | ASP.NET Core Web API (.NET) |
| База данных | PostgreSQL |
| ORM | Entity Framework Core |
| Аутентификация | ASP.NET Identity + JWT |
| Документация API | Swagger / OpenAPI |
| Хранение файлов | MinIO |
| Внешние интеграции | 2ГИС API, SMTP |
| Контейнеризация | Docker Compose |
| Мониторинг | postgres-exporter, Prometheus, Grafana |
| Тесты | xUnit, unit tests, integration tests |

## Архитектура и структура проекта

Проект разделен по слоям, чтобы HTTP-логика, бизнес-правила и инфраструктура не смешивались друг с другом.

| Папка / проект | Назначение |
|---|---|
| `Domain` | сущности, enum-ы и предметная модель |
| `Application` | DTO, контракты и сервисные абстракции |
| `Infrastructure` | EF Core, Identity, MinIO, SMTP, route services, миграции |
| `Practise` | API-слой, контроллеры, Swagger, DI, конфигурация |
| `Tests.Unit` | unit-тесты бизнес-логики |
| `Tests.Integration` | интеграционные тесты API и сценариев |

## Как скачать проект

```bash
git clone https://github.com/<your-org-or-user>/Practice-2026.git
cd Practice-2026
```

## Переменные окружения

Проект использует `.env` для Docker Compose и переменных backend-сервиса. Ниже перечислены основные группы переменных и их назначение.

### База данных

| Переменная | Обязательна | Пример | Назначение |
|---|---|---|---|
| `POSTGRES_DB` | да | `practice` | имя базы данных |
| `POSTGRES_USER` | да | `postgres02` | пользователь PostgreSQL |
| `POSTGRES_PASSWORD` | да | `0000` | пароль PostgreSQL |

### MinIO и файловое хранилище

`MINIO_ROOT_*` нужны самому контейнеру MinIO, а `STORAGE_*` использует backend для доступа к bucket-ам.

| Переменная | Обязательна | Пример | Назначение |
|---|---|---|---|
| `MINIO_ROOT_USER` | да | `minioadmin` | root user для MinIO |
| `MINIO_ROOT_PASSWORD` | да | `minioadmin123` | root password для MinIO |
| `STORAGE_ENDPOINT` | да | `minio:9000` | внутренний адрес MinIO для backend |
| `STORAGE_ACCESS_KEY` | да | `minioadmin` | access key для backend |
| `STORAGE_SECRET_KEY` | да | `minioadmin123` | secret key для backend |
| `STORAGE_USE_SSL` | да | `false` | использовать ли SSL при внутреннем подключении |
| `STORAGE_PUBLIC_PHOTOS_BUCKET` | да | `block-photos` | bucket публичных фото точек |
| `STORAGE_PRIVATE_DOCUMENTS_BUCKET` | да | `documents` | bucket приватных документов |
| `STORAGE_PUBLIC_PHOTOS_BASE_URL` | да | `http://localhost:9000/block-photos` | базовый URL публичных фото |
| `STORAGE_PUBLIC_ENDPOINT` | да | `localhost:9000` | публичный endpoint MinIO для внешних ссылок |
| `STORAGE_PUBLIC_USE_SSL` | да | `false` | использовать ли SSL в публичных ссылках |

### JWT и безопасность

| Переменная | Обязательна | Пример | Назначение |
|---|---|---|---|
| `JWT_KEY` | да | `my-very-strong-jwt-secret-key-2026-abcdef123456` | секрет для подписи JWT |
| `JWT_ISSUER` | да | `PracticeAPI` | issuer токена |
| `JWT_AUDIENCE` | да | `PracticeAPIClient` | audience токена |

### SMTP и подтверждение email

| Переменная | Обязательна | Пример | Назначение |
|---|---|---|---|
| `SMTP_HOST` | да | `smtp-relay.brevo.com` | SMTP host |
| `SMTP_PORT` | да | `587` | SMTP port |
| `SMTP_USERNAME` | да | `user@smtp-provider.com` | SMTP username |
| `SMTP_PASSWORD` | да | `secret` | SMTP password |
| `SMTP_FROM_EMAIL` | да | `wayfarertechnical@outlook.com` | адрес отправителя |
| `SMTP_FROM_NAME` | да | `Practice` | имя отправителя |
| `PUBLIC_API_BASE_URL` | да | `http://localhost:10000` | публичный базовый URL backend |
| `FRONTEND_RESULT_URL` | желательно | `http://localhost:5173/auth-result` | URL фронта для результата подтверждения email |
| `OWNER_INVITE_TOKEN` | да | `super-secret-owner-token` | токен для регистрации owner-пользователя |

### CORS

Если фронтенд запускается с нескольких origin, их удобно задавать через индексированные ключи.

| Переменная | Обязательна | Пример |
|---|---|---|
| `Cors__AllowedOrigins__0` | да | `http://localhost:3000` |
| `Cors__AllowedOrigins__1` | желательно | `http://localhost:5173` |
| `Cors__AllowedOrigins__2` | желательно | `http://127.0.0.1:3000` |
| `Cors__AllowedOrigins__3` | желательно | `http://127.0.0.1:5173` |

### Внешние интеграции

| Переменная | Обязательна | Пример | Назначение |
|---|---|---|---|
| `DGIS_API_KEY` | желательно | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` | ключ для `GET /api/Places/nearby` |

### Мониторинг

| Переменная | Обязательна | Пример | Назначение |
|---|---|---|---|
| `GRAFANA_ADMIN_USER` | да | `admin` | логин администратора Grafana |
| `GRAFANA_ADMIN_PASSWORD` | да | `strong-password` | пароль администратора Grafana |

### Пример `.env`

Ниже пример минимальной конфигурации для локального Docker-запуска.

```env
POSTGRES_DB=practice
POSTGRES_USER=postgres02
POSTGRES_PASSWORD=0000

MINIO_ROOT_USER=minioadmin
MINIO_ROOT_PASSWORD=minioadmin123

STORAGE_ENDPOINT=minio:9000
STORAGE_ACCESS_KEY=minioadmin
STORAGE_SECRET_KEY=minioadmin123
STORAGE_USE_SSL=false
STORAGE_PUBLIC_PHOTOS_BUCKET=block-photos
STORAGE_PRIVATE_DOCUMENTS_BUCKET=documents
STORAGE_PUBLIC_PHOTOS_BASE_URL=http://localhost:9000/block-photos
STORAGE_PUBLIC_ENDPOINT=localhost:9000
STORAGE_PUBLIC_USE_SSL=false

JWT_KEY=my-very-strong-jwt-secret-key-2026-abcdef123456
JWT_ISSUER=PracticeAPI
JWT_AUDIENCE=PracticeAPIClient

SMTP_HOST=smtp-relay.brevo.com
SMTP_PORT=587
SMTP_USERNAME=your-smtp-user
SMTP_PASSWORD=your-smtp-password
SMTP_FROM_EMAIL=wayfarertechnical@outlook.com
SMTP_FROM_NAME=Wayfarer

PUBLIC_API_BASE_URL=http://localhost:10000
FRONTEND_RESULT_URL=http://localhost:5173/auth-result
OWNER_INVITE_TOKEN=super-secret-owner-token

Cors__AllowedOrigins__0=http://localhost:3000
Cors__AllowedOrigins__1=http://localhost:5173
Cors__AllowedOrigins__2=http://127.0.0.1:3000
Cors__AllowedOrigins__3=http://127.0.0.1:5173

DGIS_API_KEY=your-dgis-api-key

GRAFANA_ADMIN_USER=admin
GRAFANA_ADMIN_PASSWORD=strong-password
```

## Запуск через Docker Compose

Основной способ запуска проекта — через Docker Compose.

```bash
docker compose up -d --build
```

После старта будут доступны:

| Сервис | URL / порт | Назначение |
|---|---|---|
| Backend API | `http://localhost:10000` | основной API |
| Swagger UI | `http://localhost:10000/swagger` | интерактивная документация API |
| PostgreSQL | `localhost:5433` | подключение к БД с хоста |
| MinIO API | `http://localhost:9000` | файловое хранилище |
| MinIO Console | `http://localhost:9001` | админ-панель MinIO |
| Prometheus | `http://localhost:9090` | сбор метрик |
| Grafana | `http://localhost:3010` | дашборды и мониторинг |

> В compose уже настроены отдельные volumes для PostgreSQL, MinIO, Prometheus и Grafana, а также инициализация bucket-ов MinIO через `minio-init`.

## API и группы эндпоинтов

Полную документацию и возможность протестировать ручки можно посмотреть в Swagger:

- [Swagger UI](http://localhost:10000/swagger)

В README имеет смысл ориентироваться не на полный список ручек, а на ключевые группы API.

| Группа | Базовый префикс | Назначение |
|---|---|---|
| `AuthController` | `api/auth` | регистрация, логин, logout, confirm-email, resend-confirmation |
| `UserController` | `api/user` | профиль пользователя, смена пароля, удаление аккаунта, suggestion flow |
| `RoutesController` | `api/routes` | создание маршрутов, дни маршрута, точки в днях, кастомные точки, лайки, share links |
| `BlocksController` | `api/blocks` | общий список точек каталога |
| `UnauthorizedPointController` | `api/unauthorizedpoint` | публичный просмотр approved-точек |
| `UnauthorizedRouteController` | `api/unauthorizedroute` | публичный просмотр маршрутов и shared-маршрутов |
| `OwnerController` | `api/owner` | CRUD owner-точек, фото, документы, preview-photo |
| `GetMyPointsController` | `api/getmypoints` | список точек текущего owner |
| `CheckStatsController` | `api/checkstats` | статистика owner |
| `AdminController` | `api/admin` | модерация точек, управление пользователями, admin update/delete flow |
| `PlacesController` | `api/places` | proxy к 2ГИС, `GET /api/Places/nearby` |

## Ключевые бизнес-сценарии

Ниже несколько базовых сценариев, которые закрывает backend.

### 1. Пользовательский маршрут

- пользователь регистрируется и подтверждает email
- создает маршрут
- добавляет дни маршрута
- наполняет день обычными каталожными точками и кастомными route-only точками
- при необходимости создает share-link и делится маршрутом

### 2. Owner flow

- owner создает и редактирует свои точки
- загружает фото и документы
- управляет preview-фото
- получает статистику и список своих точек

## Хранение файлов

Для хранения файлов используется MinIO.

- фото точек хранятся в публичном bucket-е
- документы хранятся в приватном bucket-е
- для документов backend выдает временные ссылки на скачивание
- у точки может быть несколько фото и одно preview-фото
- backend автоматически работает с отдельными bucket-ами для фото и документов

## Мониторинг

В проект уже встроен базовый monitoring stack для PostgreSQL.

- `postgres-exporter` собирает метрики Postgres
- `Prometheus` хранит и агрегирует метрики
- `Grafana` визуализирует состояние базы данных

Конфигурационные файлы мониторинга лежат в репозитории:

- [monitoring/prometheus/prometheus.yml](monitoring/prometheus/prometheus.yml)
- [monitoring/grafana/provisioning/datasources/prometheus.yml](monitoring/grafana/provisioning/datasources/prometheus.yml)

После запуска можно:

- открыть Prometheus на `http://localhost:9090`
- открыть Grafana на `http://localhost:3010`
- подключить или импортировать готовый PostgreSQL dashboard для `postgres-exporter`

## Тесты

В проекте есть два отдельных набора тестов:

- `Tests.Unit` — unit-тесты бизнес-логики и сервисов
- `Tests.Integration` — интеграционные тесты API-сценариев

Запуск:

```bash
dotnet test Tests.Unit/Tests.Unit.csproj
dotnet test Tests.Integration/Tests.Integration.csproj
```

Если нужно прогнать все вместе:

```bash
dotnet test
```

## Что важно помнить

- для suggestion, email confirmation, share-links и document download flow важны корректные `PUBLIC_API_BASE_URL`, SMTP и storage-настройки
- `DGIS_API_KEY` нужен только для proxy-ручки `GET /api/Places/nearby`
- при публикации Grafana наружу лучше поменять дефолтные креды и ограничить доступ по сети

