using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

Console.OutputEncoding = System.Text.Encoding.UTF8;

bool jsonMode = false;

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--json")
    {
        jsonMode = true;
        break;
    }
}

var info = new
{
    Student = "Качановський Владислав",
    Group = "ФЕІ-36",
    OSDescription = RuntimeInformation.OSDescription,
    OSVersion = Environment.OSVersion.ToString(),
    Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
    DotNetVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    ApplicationDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Предметна область: Замовлення (клієнти, товари, замовлення, рядки замовлень)"
};

var JsonOption = new JsonSerializerOptions
{
    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
};

if (jsonMode == true)
{
    string json = JsonSerializer.Serialize(info, JsonOption);
    Console.WriteLine(json);
    Console.WriteLine($"Натисніть будь-яку клавішу для завершення");
    Console.ReadKey();
    return;
}

Console.WriteLine("CrossApp - практикум з крос-платформного програмування");
Console.WriteLine("Студент: Качановський Владислав, група ФЕІ-36");
Console.WriteLine(new string('-', 67));
Console.WriteLine($"ОС (OSDescription) : {RuntimeInformation.OSDescription}");
Console.WriteLine($"ОС (Environment) : {Environment.OSVersion}");
Console.WriteLine($"Архітектура процесу : {RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"Версія .NET (CLR) : {Environment.Version}");
Console.WriteLine($"Runtime : {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Каталог застосунку : {AppContext.BaseDirectory}");
Console.WriteLine($"Поточний каталог : {Environment.CurrentDirectory}");
Console.WriteLine(new string('-', 67));
Console.WriteLine("Предметна область: Замовлення (клієнти, товари, замовлення, рядки замовлень)");
Console.WriteLine("Призначення: оформлення замовлень і підрахунок сум.");

Console.WriteLine(new string('-', 67));
Console.WriteLine($"Натисніть будь-яку клавішу для завершення");
Console.ReadKey();