# CrossApp
Наскрізний проєкт з крос-платформного програмування.
Предметна область: Замовлення. Сутності: Customer, Product, Order, OrderLine.
Призначення: оформлення замовлень і підрахунок сум.

## Середовище

.NET SDK 10.0.303
Windows 11 x64
RID: win-x64
VS Code
C# Dev Kit
Git

## Структура solution

```text
CrossApp/
├── CrossApp.slnx
├── README.md
├── .gitignore
├── data/
│   ├── sample.csv
│   ├── sample.json
│   └── mixed.csv
└── src/
    ├── Cli/
    │   ├── Cli.csproj
    │   └── Program.cs
    └── Core/
        ├── Core.csproj
        ├── EnvironmentInfo.cs
        ├── Domain/
        │   ├── Order.cs
        │   └── OrderLine.cs
        ├── Dto/
        │   ├── ImportEntryDto.cs
        │   ├── ProductDto.cs
        │   ├── CustomerDto.cs
        │   ├── ImportResult.cs
        │   ├── OrderDto.cs
        │   └── OrderLineDto.cs
        └── Import/
            ├── ProductCsvImporter.cs
            ├── ProductJsonImporter.cs
            └── MixedCsvImporter.cs
```

## Лабораторна робота 4

Головна сутність: Order.
Пов'язана сутність: OrderLine.

### Інваріанти

- CustomerId не може бути порожнім.
- ProductId і назва товару не можуть бути порожніми.
- Кількість у рядку має бути більшою за нуль.
- Ціна не може бути від'ємною.
- До підтвердженого замовлення не можна додавати рядки.
- Порожнє замовлення не можна підтвердити.
- Підтверджене замовлення не можна підтверджувати повторно.

Стан замовлення змінюється лише через AddLine і Confirm.
Колекція Lines доступна назовні тільки для читання.
Для збереження і відновлення використовуються ToDto і FromDto.

### Запуск лабораторної 4

```bash
dotnet build
dotnet run --project src/Cli
```

Або явно:

```bash
dotnet run --project src/Cli -- --lab4
```

## Лабораторна робота 3

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

## Імпорт даних

```text
Основний формат CSV:
id;name;price
P-xxx;name;price

Програма завантажує коректні записи, а пошкоджені рядки пропускає та виводить номер рядка і причину помилки.

Підтримуються:
- CSV
- JSON
- змішані записи Product і Customer.
```
