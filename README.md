# CrossApp
Наскрізний проєкт з крос-платформного програмування.
Предметна область: Замовлення. Сутності: Customer, Product, Order, OrderLine.
Призначення: оформлення замовлень і підрахунок сум.

## Запуск 
```bash
dotnet build
dotnet run --project src/Cli
```

## Запуск у форматі JSON: 
```bash
dotnet run --project src/Cli -- --json
```
## Середовище

.NET SDK 10.0.303
Windows 11 x64
RID: win-x64
VS Code
C# Dev Kit
Git

## Self-contained публікація

### Windows x64
Публікація:
dotnet publish src/Cli -c Release -r win-x64 --self-contained true


Запуск:
.\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe

Розмір каталогу win-x64 publish: 78 МБ

### Linux x64
Публікація:
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true


Запуск:
./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli

Розмір каталогу linux-x64 publish: 80МБ
