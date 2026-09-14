using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Core;

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

EnvironmentReport report = EnvironmentInfo.Collect();

var info = new
{
    Student = "Качановський Владислав",
    Group = "ФЕІ-36",
    OSDescription = report.OsDescription,
    OSVersion = report.OsVersion,
    Architecture = report.ProcessArchitecture,
    DotNetVersion = report.DotNetVersion,
    Runtime = report.FrameworkDescription,
    ApplicationDirectory = report.BaseDirectory,
    CurrentDirectory = report.CurrentDirectory,
    DetectedRid = report.DetectedRid,
    ReportedRid = report.ReportedRid,
    BuildNote = report.BuildNote,
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

Console.WriteLine($"ОС (OSDescription) : {report.OsDescription}");
Console.WriteLine($"ОС (Environment) : {report.OsVersion}");
Console.WriteLine($"Архітектура процесу : {report.ProcessArchitecture}");
Console.WriteLine($"Версія .NET (CLR) : {report.DotNetVersion}");
Console.WriteLine($"Runtime : {report.FrameworkDescription}");
Console.WriteLine($"RID (визначено) : {report.DetectedRid}");
Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
Console.WriteLine($"Каталог застосунку : {report.BaseDirectory}");
Console.WriteLine($"Поточний каталог : {report.CurrentDirectory}");
Console.WriteLine($"Версія збірки : {report.BuildNote}");
Console.WriteLine(new string('-', 67));
Console.WriteLine("Предметна область: Замовлення (клієнти, товари, замовлення, рядки замовлень)");
Console.WriteLine("Призначення: оформлення замовлень і підрахунок сум.");

Console.WriteLine(new string('-', 67));
Console.WriteLine($"Натисніть будь-яку клавішу для завершення");
Console.ReadKey();