using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class FileSink(string path) : INotificationSink
{
    private readonly string _path = Path.GetFullPath(path);

    public void Notify(OrderChangedEventArgs change)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.AppendAllText(_path, change + Environment.NewLine);
    }
}