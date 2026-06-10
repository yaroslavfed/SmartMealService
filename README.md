# SmartMealService

Тестовое задание на позицию Middle Fullstack C# Developer в компании «ООО СМАРТ МИЛ СЕРВИС»

Проект состоит из двух частей:

1. Библиотеки и консольное приложение для обмена с SMS-сервером по HTTP/gRPC, сохранения меню в PostgreSQL и отправки заказа.
2. WPF-приложение для чтения и изменения пользовательских переменных среды.

Разработка велась по TDD-подходу: сначала тест на ожидаемое поведение, затем минимальная реализация, затем рефакторинг. Для проверки используются xUnit, FluentAssertions, Moq, WireMock.Net и EF Core InMemory.

## Архитектурные решения

Понимаю, что для данного тестового задания выбранная архитектура выглядит несколько избыточной.

Большинство требований можно было реализовать значительно меньшим количеством кода и проектов. Однако при выполнении задания мне было интересно показать не только конечный результат, но и профессиональный подход к разработке: проектирование структуры решения, работу через абстракции, покрытие тестами, использование DI, логирования и конфигурации.

Поэтому некоторые решения были приняты не из необходимости, а для демонстрации навыков и практик, которые я использую в повседневной работе. Если бы задача решалась исключительно с целью минимизации объема кода, итоговая реализация была бы заметно проще.

## Структура

```text
SmartMealService/
├── SmartMealService.slnx
├── docker-compose.yml
├── README.md
├── src/
│   ├── SmartMealService.Shared/
│   ├── SmartMealService.Http/
│   ├── SmartMealService.Grpc/
│   ├── SmartMealService.Console/
│   └── SmartMealService.Wpf/
└── tests/
    ├── SmartMealService.Shared.Tests/
    ├── SmartMealService.Http.Tests/
    ├── SmartMealService.Grpc.Tests/
    ├── SmartMealService.Console.Tests/
    ├── SmartMealService.Wpf.Tests/
    └── SmartMealService.FakeSmsServer/
```

## Проекты

### SmartMealService.Shared

Общий проект с доменными моделями и контрактом SMS-клиента.

Основные файлы:

- `Models/MenuItem.cs` - блюдо из меню SMS.
- `Models/Order.cs` - заказ.
- `Models/OrderItem.cs` - позиция заказа.
- `Abstractions/ISmsClient.cs` - общий интерфейс для HTTP и gRPC клиентов.
- `Exceptions/SmsApiException.cs` - ошибка бизнес-ответа SMS-сервера, когда `Success = false`.

### SmartMealService.Http

DLL-библиотека для работы с SMS-сервером по HTTP.

Что делает:

- отправляет `GetMenu` на общий endpoint;
- использует Basic Auth;
- парсит успешный ответ в `List<MenuItem>`;
- выбрасывает `SmsApiException`, если сервер вернул `Success = false`;
- отправляет `SendOrder`;
- сериализует дробные количества через invariant culture, поэтому `0.408` отправляется как `"0.408"`, а не `"0,408"`.

Основные файлы:

- `SmsHttpClient.cs` - публичный клиент библиотеки.
- `Transport/JsonPostEndpointClient.cs` - общий JSON POST-клиент.
- `Contracts/GetMenu/*` - DTO для запроса и ответа `GetMenu`.
- `Contracts/SendOrder/*` - DTO для запроса и ответа `SendOrder`.
- `Mapping/MenuItemMapper.cs` - mapping ответа HTTP в доменную модель.
- `Mapping/SendOrderRequestFactory.cs` - создание HTTP-тела `SendOrder`.

### SmartMealService.Grpc

DLL-библиотека для работы с SMS-сервером по gRPC.

Что делает:

- использует proto-контракт из ТЗ;
- вызывает `GetMenu(google.protobuf.BoolValue)`;
- вызывает `SendOrder(Order)`;
- не использует авторизацию, как указано в ТЗ;
- мапит gRPC-модели в общие доменные модели;
- выбрасывает `SmsApiException`, если сервер вернул `success = false`.

Основные файлы:

- `Protos/sms.proto` - proto-файл из Приложения №3.
- `SmsGrpcClient.cs` - публичный gRPC-клиент.
- `Mapping/GrpcMenuItemMapper.cs` - mapping блюда.
- `Mapping/GrpcOrderMapper.cs` - mapping заказа.

### SmartMealService.Console

Консольное приложение .NET 8.

Что делает при запуске:

1. Читает конфигурацию из `Properties/appsettings.json`.
2. Инициализирует PostgreSQL через EF Core migrations.
3. Получает меню через HTTP DLL.
4. Сохраняет меню в таблицу `menu_items`.
5. Выводит меню в формате:

```text
Название – Код (артикул) – Цена за единицу
```

6. Создает экземпляр `Order`, который будет заполнен позициями заказа.
7. Просит ввести заказ одной строкой:

```text
Код1:Количество1;Код2:Количество2
```

8. Проверяет, что коды есть в меню, а количество является числом больше нуля.
9. При ошибке ввода показывает сообщение и просит ввести заказ заново.
10. Отправляет заказ на сервер.
11. При успехе выводит `УСПЕХ`, при ошибке выводит текст ошибки сервера.

Основные файлы:

- `Program.cs` - входная точка приложения.
- `Startup/ConsoleAppFactory.cs` - сборка конфигурации и DI-контейнера Autofac.
- `Ordering/OrderConsoleRunner.cs` - runner для консольного приложения.
- `Input/OrderInputParser.cs` - парсинг и валидация пользовательского ввода.
- `Persistence/EfCore/MenuDbContext.cs` - EF Core DbContext.
- `Persistence/EfCore/EfMenuRepository.cs` - инициализация БД и сохранение меню.
- `Persistence/EfCore/Migrations/*` - миграция таблицы `menu_items`.
- `ConsoleIO/LoggingConsoleIO.cs` - вывод в консоль и логирование всего содержимого консоли.
- `Properties/appsettings.json` - строка подключения и настройки HTTP-клиента.

Логи пишутся в:

```text
logs/test-sms-console-app-yyyyMMdd.log
```

### SmartMealService.Wpf

WPF-приложение .NET 8 для чтения и изменения переменных среды.

Что делает:

- читает список переменных из `Properties/appsettings.json`;
- при запуске читает значения user environment variables;
- если переменной нет, создает ее со значением по умолчанию из `EnvironmentVariables:Defaults`;
- показывает переменные в таблице;
- позволяет менять текстовые значения без ограничения длины на уровне приложения;
- сохраняет значения в `EnvironmentVariableTarget.User`;
- отправляет Windows `WM_SETTINGCHANGE`, чтобы другие приложения могли увидеть изменения;
- логирует факты изменения переменных через NLog;
- использует MVVM, Autofac, ReactiveUI, Rx и Fody.

Структура WPF-проекта:

```text
SmartMealService.Wpf/
├── App.xaml
├── App.xaml.cs
├── Properties/
│   └── appsettings.json
├── Installers/
│   ├── IContainerInstaller.cs
│   ├── ServiceInstaller.cs
│   ├── ViewModelInstaller.cs
│   └── WindowInstaller.cs
├── Startup/
│   ├── WpfAppFactory.cs
│   ├── ReactiveUiBootstrapper.cs
│   └── Configuration/
├── Windows/
│   └── MainWindow/
├── Controls/
│   └── EnvironmentVariablesPanel/
├── Services/
│   └── EnvironmentVariables/
└── Styles/
```

Основные файлы:

- `Windows/MainWindow/MainWindow.xaml` - главное окно.
- `Windows/MainWindow/MainWindowViewModel.cs` - ViewModel окна.
- `Controls/EnvironmentVariablesPanel/EnvironmentVariablesPanel.xaml` - таблица переменных.
- `Controls/EnvironmentVariablesPanel/EnvironmentVariablesPanelViewModel.cs` - логика чтения, создания и сохранения переменных.
- `Controls/EnvironmentVariablesPanel/CommitTextOnLostFocusBehavior.cs` - commit значения при потере фокуса.
- `Services/EnvironmentVariables/EnvironmentVariableStore/UserEnvironmentVariableStore.cs` - чтение и запись user environment variables.
- `Services/EnvironmentVariables/EnvironmentVariableChangeNotifier/WindowsEnvironmentVariableChangeNotifier.cs` - уведомление Windows об изменении переменных.
- `Services/EnvironmentVariables/EnvironmentVariableLogging/NLogEnvironmentVariableChangeLogger.cs` - логирование изменений.
- `Styles/*` - стили WPF-приложения.

Логи пишутся в:

```text
logs/test-sms-wpf-app-yyyyMMdd.log
```

### SmartMealService.FakeSmsServer

Вспомогательный fake HTTP-сервер для ручной проверки.

Не является частью ТЗ как production-компонент.
Лежит в `tests`, потому что нужен только для локальной ручной проверки консольного приложения и HTTP-интеграции.

Сервер слушает:

```text
http://localhost:5000
```

И возвращает примеры меню из ТЗ.

## Конфигурация

### PostgreSQL

PostgreSQL поднимается через Docker Compose.

Файл:

```text
docker-compose.yml
```

Порт на хосте:

```text
5433
```

Внутренний порт контейнера:

```text
5432
```

Порт `5433` выбран специально, чтобы не конфликтовать с локальным PostgreSQL на Windows, который часто занимает стандартный `5432`.

### Console appsettings

Файл:

```text
src/SmartMealService.Console/Properties/appsettings.json
```

Содержит:

- строку подключения к PostgreSQL;
- URL fake SMS HTTP-сервера;
- username/password для Basic Auth.

Текущая строка подключения:

```json
"DefaultConnection": "Host=localhost;Port=5433;Database=smart_meal_service;Username=postgres;Password=postgres"
```

### WPF appsettings

Файл:

```text
src/SmartMealService.Wpf/Properties/appsettings.json
```

Содержит массив имен переменных среды, значения по умолчанию и комментарии:

```json
{
  "EnvironmentVariables": {
    "Names": [
      "SMS_HTTP_BASE_URL",
      "SMS_HTTP_USERNAME",
      "SMS_HTTP_PASSWORD"
    ],
    "Defaults": {
      "SMS_HTTP_BASE_URL": "http://localhost:5000/",
      "SMS_HTTP_USERNAME": "testuser",
      "SMS_HTTP_PASSWORD": "testpass"
    }
  }
}
```

`Names` - обязательный массив из ТЗ. 
`Defaults` - дополнительная секция, которая делает значения по умолчанию явными. 
Если для переменной нет значения в `Defaults`, приложение использует пустую строку.

Сейчас при сборке WPF этот файл копируется в output как `appsettings.json`, потому что приложение читает конфигурацию из папки запуска.

## Запуск

### 1. Поднять PostgreSQL

```powershell
docker compose up -d
```

Проверить контейнер:

```powershell
docker ps
```

Ожидаемый порт:

```text
0.0.0.0:5433->5432/tcp
```

### 2. Запустить fake SMS HTTP-сервер

```powershell
dotnet run --project tests/SmartMealService.FakeSmsServer
```

### 3. Запустить Console

```powershell
dotnet run --project src/SmartMealService.Console --no-launch-profile
```

Ожидаемый вывод меню:

```text
Каша гречневая – 5979224 (A1004292) – 50
Конфеты Коровка – 9084246 (A1004293) – 300
Введите блюда в формате Код1:Количество1;Код2:Количество2
```

Пример валидного заказа:

```text
5979224:1;9084246:0.408
```

Ожидаемый ответ:

```text
УСПЕХ
```

### 4. Проверить PostgreSQL

```powershell
docker exec -it smart-meal-postgres psql -U postgres -d smart_meal_service
```

Внутри `psql`:

```sql
\dt
select * from menu_items;
```

### 5. Запустить WPF

```powershell
dotnet run --project src/SmartMealService.Wpf
```

Ожидаемо:

- таблица показывает переменные из WPF appsettings;
- значения можно редактировать;
- изменения сохраняются в user environment variables.

## Тесты

Запуск всех тестов:

```powershell
dotnet test
```

Покрытие по тестовым проектам:

- `SmartMealService.Shared.Tests` - модели и JSON-контракты.
- `SmartMealService.Http.Tests` - HTTP-клиент, Basic Auth, тела запросов, ошибки, WireMock.Net.
- `SmartMealService.Grpc.Tests` - gRPC-клиент, mapping, ошибки, передача заказа.
- `SmartMealService.Console.Tests` - парсинг ввода, сценарий консоли, EF repository, DI/config.
- `SmartMealService.Wpf.Tests` - ViewModel, DI, appsettings, запись переменных, NLog.

WPF UI-окно не тестируется unit-тестами напрямую. Это осознанное решение: по TDD покрыта ViewModel и сервисная логика.

## TDD

При разработке использовался цикл:

```text
Red -> Green -> Refactor
```

## Ручная Проверка По ТЗ

Пункты ручной проверки Console, где нужен ответ SMS-сервера, выполняются при предварительно запущенном `SmartMealService.FakeSmsServer`.

| № | Требование из ТЗ | Проверка | Статус |
|---|---|---|---|
| 1 | HTTP DLL компилируется отдельно | `SmartMealService.Http` есть в `src` и solution | Пройдено |
| 2 | HTTP использует общий endpoint | WireMock-тесты проверяют POST `/` | Пройдено |
| 3 | HTTP использует Basic Auth | Тест проверяет точный `Authorization: Basic ...` | Пройдено |
| 4 | HTTP `GetMenu` отправляет `Command = GetMenu` | Тест проверяет тело запроса | Пройдено |
| 5 | HTTP `GetMenu` парсит успешный ответ | Тест возвращает два блюда из ТЗ | Пройдено |
| 6 | HTTP `Success = false` обрабатывается как ошибка | Тест ожидает `SmsApiException` | Пройдено |
| 7 | HTTP `SendOrder` отправляет заказ | Тест проверяет `OrderId` и позиции | Пройдено |
| 8 | HTTP дробное количество отправляется как `"0.408"` | Тест проверяет точную строку quantity | Пройдено |
| 9 | gRPC proto соответствует ТЗ | `Protos/sms.proto` содержит сервис и сообщения из приложения №3 | Пройдено |
| 10 | gRPC без авторизации | `SmsGrpcClient` не использует auth | Пройдено |
| 11 | gRPC `GetMenu` возвращает блюда | Тест проверяет mapping меню | Пройдено |
| 12 | gRPC `SendOrder` передает все позиции | Тест проверяет captured gRPC request | Пройдено |
| 13 | Console .NET 8 | `SmartMealService.Console` target `net8.0` | Пройдено |
| 14 | Console подключает DLL-клиент | DI регистрирует `SmsHttpClient` как `ISmsClient` | Пройдено |
| 15 | Console инициализирует БД и таблицу | EF Core migrations создают `__EFMigrationsHistory` и `menu_items` | Пройдено |
| 16 | Console получает меню от сервера | Ручной запуск с предварительно запущенным fake server выводит два блюда | Пройдено |
| 17 | Console сохраняет меню в PostgreSQL | `select * from menu_items` возвращает два блюда | Пройдено |
| 18 | Console выводит меню в нужном формате | Ручной запуск выводит `Название – Код (артикул) – Цена за единицу` | Пройдено |
| 19 | Console повторяет ввод при ошибке | Проверены `bad input`, неизвестный код, `0`, `-1`, `abc` | Пройдено |
| 20 | Console принимает валидный заказ | Проверен ввод `5979224:1;9084246:0.408` | Пройдено |
| 21 | Console выводит `УСПЕХ` при успехе | Ручная проверка с предварительно запущенным fake server | Пройдено |
| 22 | Console appsettings содержит БД и настройки | `Properties/appsettings.json` содержит connection string и SMS HTTP | Пройдено |
| 23 | Console пишет все содержимое консоли в лог | Лог содержит вывод, ввод, ошибки и `УСПЕХ` | Пройдено |
| 24 | Console log filename соответствует ТЗ | `test-sms-console-app-yyyyMMdd.log` | Пройдено |
| 25 | WPF .NET 8 | `SmartMealService.Wpf` target `net8.0-windows10.0.19041.0` | Пройдено |
| 26 | WPF читает набор переменных из appsettings | WPF tests и ручной запуск | Пройдено |
| 27 | WPF показывает переменные в графическом виде | Таблица открывается и отображает переменные | Пройдено |
| 28 | WPF UI стилизован по макету | Ручная проверка: окно, фон, таблица, кнопки | Пройдено |
| 29 | WPF создает отсутствующие переменные | Ручная проверка и tests | Пройдено |
| 30 | WPF сохраняет значения после перезапуска | User environment variables сохраняются | Пройдено |
| 31 | WPF значения доступны другим приложениям | Проверено через новый PowerShell | Пройдено |
| 32 | WPF логирует изменения переменных | NLog-файл создается и содержит изменения | Пройдено |
| 33 | WPF log filename соответствует ТЗ | `test-sms-wpf-app-yyyyMMdd.log` | Пройдено |
| 34 | Все проекты подключены в solution | `SmartMealService.slnx` содержит src и tests проекты | Пройдено |
| 35 | Сборка проходит | `dotnet build` успешен | Пройдено |
| 36 | Тесты проходят | `dotnet test`: 64 теста пройдено | Пройдено |
