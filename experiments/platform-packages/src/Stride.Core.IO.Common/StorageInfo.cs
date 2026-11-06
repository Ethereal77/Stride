namespace Stride.Core.IO;

/// <summary>Describes how a platform stores and compares paths.</summary>
public readonly record struct StorageInfo(string Root, char Separator, bool IsCaseSensitive);
