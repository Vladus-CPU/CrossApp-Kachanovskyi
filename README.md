# CrossApp

Наскрізний проєкт з крос-платформного програмування.

Предметна область: Замовлення. Сутності: Customer, Product, Order, OrderLine.

Призначення: оформлення замовлень, підрахунок сум, зберігання даних та обробка подій.

## Середовище

- .NET SDK 10.0.303
- Windows 11 x64
- RID: win-x64
- VS Code
- C# Dev Kit
- Git

## Структура solution

```text
CrossApp/
├── CrossApp.slnx
├── README.md
├── .gitignore
├── data/
│   ├── sample.csv
│   ├── sample.json
│   ├── mixed.csv
│   └── orders.json
└── src/
    ├── Cli/
    │   ├── Cli.csproj
    │   ├── Program.cs
    │   ├── CompositionRoot.cs
    │   ├── StoreFactory.cs
    │   └── Notifications/
    │       ├── ConsoleSink.cs
    │       └── FileSink.cs
    └── Core/
        ├── Core.csproj
        ├── EnvironmentInfo.cs
        ├── SampleData.cs
        ├── Abstractions/
        │   ├── IOrderStore.cs
        │   └── INotificationSink.cs
        ├── Domain/
        │   ├── Customer.cs
        │   ├── Order.cs
        │   ├── OrderLine.cs
        │   ├── OrderMapper.cs
        │   ├── OrderRules.cs
        │   └── OrderStatus.cs
        ├── Dto/
        │   ├── ImportEntryDto.cs
        │   ├── ProductDto.cs
        │   ├── CustomerDto.cs
        │   ├── ImportResult.cs
        │   ├── OrderDto.cs
        │   └── OrderLineDto.cs
        ├── Events/
        │   ├── OrderChangeKind.cs
        │   └── OrderChangedEventArgs.cs
        ├── Import/
        │   ├── ProductCsvImporter.cs
        │   ├── ProductJsonImporter.cs
        │   └── MixedCsvImporter.cs
        ├── Services/
        │   └── OrderService.cs
        └── Storage/
            ├── InMemoryOrderStore.cs
            ├── FileOrderStore.cs
            └── CachingOrderStore.cs
```

## Сутності

Головна сутність: Order.

Пов'язані сутності: OrderLine і Customer.

Order містить ідентифікатор замовлення, ідентифікатор клієнта, список рядків замовлення, загальну суму та стан замовлення.

### Інваріанти

- CustomerId не може бути порожнім.
- ProductId і назва товару не можуть бути порожніми.
- Кількість у рядку має бути більшою за нуль.
- Ціна не може бути від'ємною.
- Рядки можна додавати тільки до замовлення у стані Draft.
- Порожнє замовлення не можна підтвердити.
- Підтверджене або скасоване замовлення не можна змінювати.
- Клієнт не може мати більше 5 активних замовлень.

Для стану замовлення використовуються Draft, Confirmed і Cancelled.

Для збереження і відновлення сутностей використовуються ToDto і FromDto.

OrderMapper перетворює ImportResult<OrderDto> у ImportResult<Order>, зберігаючи записи, що пройшли перевірки, та повідомлення про помилки.

## Сервісний шар і сховища

Для роботи із замовленнями використовується інтерфейс IOrderStore.

Він містить основні операції:

- List - отримання всіх замовлень.
- GetById - пошук замовлення за ID.
- Add - додавання нового замовлення.
- Update - оновлення замовлення.
- Remove - видалення замовлення.

Реалізовано два основні способи зберігання:

- InMemoryOrderStore - зберігає дані тільки в оперативній пам'яті.
- FileOrderStore - зберігає дані у файлі data/orders.json у форматі JSON.

CachingOrderStore приймає інший IOrderStore та додає кешування без зміни основного контракту.

Для виконання операцій із замовленнями використовується OrderService.

OrderService залежить від IOrderStore, тому не прив'язаний до конкретного способу зберігання даних.

### Composition Root і фабрика

Для створення сервісів та передавання залежностей використовується CompositionRoot.

StoreFactory відповідає за вибір реалізації IOrderStore залежно від аргументів запуску.

CompositionRoot створює OrderService та передає залежності через конструктор.

Схема залежностей:

```text
Cli -> CompositionRoot -> OrderService -> IOrderStore
              |                           ├── InMemoryOrderStore
              |                           ├── FileOrderStore
              |                           └── CachingOrderStore
              |
              └── StoreFactory -> IOrderStore
```

Для додаткового завдання також підтримується передавання INotificationSink через конструктор OrderService.

## Події та делегати

У OrderService реалізовано подію Changed типу EventHandler<OrderChangedEventArgs> для повідомлення про зміни стану замовлень.

Визначено такі типи подій:

- OrderCreated - створення замовлення.
- LineAdded - додавання товарної позиції.
- OrderConfirmed - підтвердження замовлення.
- OrderCancelled - скасування замовлення.
- OrderRemoved - видалення замовлення.

OrderChangedEventArgs містить OrderId, Kind, LinesCount, Total та At.

Подія викликається через приватний метод OnChanged після успішної зміни стану. У разі порушення інваріанта подія не виникає.

### Підписники

У Cli реалізовано два підписники:

- OnOrderChanged - виведення повідомлень у консоль та запис до logs/app.log.
- Лямбда-вираз - підрахунок отриманих подій через змінну changes.

Підписка виконується оператором +=, відписка - оператором -=.

Після відписки OnOrderChanged другий підписник продовжує працювати, але нові повідомлення не записуються до app.log.

### Делегати

В OrderService реалізовано пошук із параметром Func<Order, bool>, який дозволяє передавати критерій відбору замовлень.

У MixedCsvImporter.Load додано необов'язковий параметр Action<string> для повідомлень про пропущені рядки під час імпорту.

### Dependency Injection

Для обробки повідомлень реалізовано інтерфейс INotificationSink із двома реалізаціями:

- ConsoleSink - виведення повідомлень у консоль.
- FileSink - запис повідомлень до файлу.

Залежність передається до OrderService через конструктор.

Створення сервісів виконується через CompositionRoot.

### Журналювання

Повідомлення зберігаються у файлах:

- logs/app.log - журнал підписника Changed.
- logs/di.log - журнал FileSink.

Шляхи формуються через Path.Combine та AppContext.BaseDirectory, а каталоги створюються автоматично.

Для стандартного запуску файли розміщуються у:

```text
src/Cli/bin/Debug/net10.0/logs/
```

## Запуск

Збірка solution:

```bash
dotnet build
```

### Запуск зі сховищем у пам'яті

```bash
dotnet run --project src/Cli
```

У цьому режимі використовується InMemoryOrderStore.

Дані, створені під час роботи програми, не зберігаються після завершення.

Також демонструються події, підписники, відписка, журналювання та пошук через Func.

### Запуск із файловим сховищем

```bash
dotnet run --project src/Cli -- --file
```

У цьому режимі використовується FileOrderStore.

Дані зберігаються у:

```text
data/orders.json
```

Після повторного запуску попередні записи залишаються у файлі.

### Запуск із кешем

```bash
dotnet run --project src/Cli -- --cache
```

У цьому режимі використовується CachingOrderStore поверх InMemoryOrderStore.

### Запуск файлового сховища з кешем

```bash
dotnet run --project src/Cli -- --file --cache
```

У цьому режимі CachingOrderStore працює поверх FileOrderStore, тому дані також зберігаються у orders.json.

### Запуск сценарію замовлень

```bash
dotnet run --project src/Cli -- --order
```

У цьому режимі перевіряються доменні сутності, інваріанти, стани замовлень і перетворення DTO.

## Імпорт даних

Основний формат CSV:

```text
id;name;price
P-xxx;name;price
```

Програма завантажує коректні записи, а пошкоджені рядки пропускає та виводить номер рядка і причину помилки.

Підтримуються:

- CSV
- JSON
- змішані записи Product і Customer

### Запуск CSV

```bash
dotnet run --project src/Cli -- data/sample.csv
```

### Запуск JSON

```bash
dotnet run --project src/Cli -- data/sample.json
```

### Запуск змішаного CSV

```bash
dotnet run --project src/Cli -- --mixed data/mixed.csv
```

Під час змішаного імпорту використовується Action<string> для повідомлень про пропущені рядки.

## Публікація (Publish)

### Команди збірки публікаційних артефактів

Windows x64 - Framework-dependent:

```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained false
```

Windows x64 - Self-contained:

```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true
```

Windows x64 - Single File:

```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Windows x64 - Trimmed:

```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishTrimmed=true
```

Linux x64 - Self-contained:

```bash
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true
```

### Запуск опублікованих додатків

Windows:

```bash
.\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe
.\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe data\sample.json
.\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe --mixed data\mixed.csv
.\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe --order
.\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe --file
.\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe --cache
.\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe --file --cache
```

Linux:

```bash
./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli
./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli data/sample.json
./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli --mixed data/mixed.csv
./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli --order
./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli --file
./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli --cache
./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli --file --cache
```

## Self-contained vs Framework-dependent

### Framework-dependent

Публікує скомпільований код застосунку та необхідні залежності.

Вимагає встановленого .NET Runtime відповідної версії (.NET 10).

Перевагою є мінімальний розмір публікації. Недоліком є залежність від встановленого середовища виконання.

### Self-contained (автономний)

Публікує застосунок разом із середовищем виконання .NET під конкретний RID (Runtime Identifier).

Не потребує встановленого .NET Runtime на цільовій машині.

Перевагою є автономність застосунку. Недоліком є більший розмір публікації.

## Порівняння режимів публікації

Результати попередніх вимірювань:

| RID | Режим | Розмір publish | Кількість файлів | Потрібен runtime |
|---|---|---:|---:|---|
| win-x64 | Framework-dependent | ~0.21 МБ | 7 | Так (.NET 10) |
| win-x64 | Self-contained | 77.11 МБ | 194 | Ні |
| win-x64 | Single File (Self-contained) | 70.16 МБ | 3 | Ні |
| win-x64 | PublishTrimmed (Self-contained) | 19.44 МБ | 35 | Ні |
| linux-x64 | Self-contained | 80.00 МБ | 195 | Ні |