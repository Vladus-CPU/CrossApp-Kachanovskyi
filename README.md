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
└── src/
    ├── Cli/
    │   ├── Cli.csproj
    │   └── Program.cs
    └── Core/
        ├── Core.csproj
        └── EnvironmentInfo.cs
```

## Запуск 
```bash
dotnet build
dotnet run --project src/Cli
```

## Запуск у форматі JSON: 
```bash
dotnet run --project src/Cli -- --json
```

## Публікація (Publish)

### Команди збірки публікаційних артефактів

Windows x64 (Framework-dependent):
```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained false
```

Windows x64 (Self-contained):
```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true
```

Windows x64 (Single File):
```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Windows x64 (Trimmed):
```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishTrimmed=true
```

Linux x64 (Self-contained):
```bash
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true
```

### Запуск опублікованих додатків

Windows:
```bash
.\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe
.\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe --json
```

Linux:
```bash
./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli
./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli --json
```

## Self-contained vs Framework-dependent

### Framework-dependent
```text
Публікує виключно скомпільований код застосунку та його прямі сторонні залежності.

Вимагає наявності попередньо встановленого на цільовій машині .NET Runtime відповідної версії (.NET 10).

Перевагою є мінімальний розмір  і швидке розгортання.
Недоліком є залежність від конфігурації системи.
```

### Self-contained (автономний)
```text
Пакує застосунок разом із середовищем виконання, системними бібліотеками та всіма необхідними компонентами під конкретний RID (Runtime Identifier).

Не вимагає наявності встановленого .NET на цільовій машині.

Перевага є повна ізоляція та гарантована працездатність на системі з відповідною платформою. 
Недоліком є суттєво більший розмір дистрибутива та значна кількість файлів у каталозі.
```
## Порівняння режимів публікації

| RID | Режим | Розмір publish | Кількість файлів | Потрібен runtime |
|---|---|---:|---:|---|
| win-x64 | Framework-dependent | ~0.21 МБ | 7 | Так (.NET 10) |
| win-x64 | Self-contained | 77.11 МБ | 194 | Ні |
| win-x64 | Single File (Self-contained) | 70.16 МБ | 3 | Ні |
| win-x64 | PublishTrimmed (Self-contained) | 19.44 МБ | 35 | Ні |
| linux-x64 | Self-contained | 80.00 МБ | 195 | Ні |

