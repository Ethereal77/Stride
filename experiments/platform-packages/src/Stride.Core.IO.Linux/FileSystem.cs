namespace Stride.Core.IO;

/// <summary>Concrete Linux file-system services.</summary>
public sealed class FileSystem
{
    public StorageInfo Info { get; } = new("/var/lib/stride", '/', IsCaseSensitive: true);

    public string NativeLibrary => "libstride_io_linux.so";

    public string Combine(string left, string right) => $"{left}{Info.Separator}{right}";
}
