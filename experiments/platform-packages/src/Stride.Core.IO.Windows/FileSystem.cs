namespace Stride.Core.IO;

/// <summary>Concrete Windows file-system services.</summary>
public sealed class FileSystem
{
    public StorageInfo Info { get; } = new(@"C:\ProgramData\Stride", '\\', IsCaseSensitive: false);

    public string NativeLibrary => "stride_io_windows.dll";

    public string Combine(string left, string right) => $"{left}{Info.Separator}{right}";
}
