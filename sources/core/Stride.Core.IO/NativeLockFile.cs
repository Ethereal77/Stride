// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Runtime.InteropServices;

namespace Stride.Core.IO;

public static partial class NativeLockFile
{
    [DllImport("Kernel32.dll", SetLastError = true)]
    private static extern bool LockFileEx(Microsoft.Win32.SafeHandles.SafeFileHandle handle, uint flags, uint reserved, uint countLow, uint countHigh, ref NativeOverlapped overlapped);

    [DllImport("Kernel32.dll", SetLastError = true)]
    private static extern bool UnlockFileEx(Microsoft.Win32.SafeHandles.SafeFileHandle handle, uint reserved, uint countLow, uint countHigh, ref NativeOverlapped overlapped);

    internal const uint LOCKFILE_FAIL_IMMEDIATELY = 0x00000001;
    internal const uint LOCKFILE_EXCLUSIVE_LOCK = 0x00000002;

    private const int ERROR_LOCK_VIOLATION = 33;


    /// <summary>Outcome of <see cref="TryAcquireFile"/> — contention means retry, unsupported means stop trying.</summary>
    public enum FileLockResult
    {
        Acquired,
        Contended,
        Unsupported,
    }


    /// <summary>
    ///   Attempts to lock a byte range of a file.
    /// </summary>
    /// <returns><c>true</c> if the lock was acquired, <c>false</c> if the lock could not be acquired.</returns>
    public static bool TryLockFile(FileStream fileStream, long offset, long count, bool exclusive, bool failImmediately = false)
    {
        if (OperatingSystem.IsWindows())
        {
            var countLow = (uint)count;
            var countHigh = (uint)(count >> 32);

            var overlapped = new NativeOverlapped()
            {
                OffsetLow = (int)(offset & uint.MaxValue),
                OffsetHigh = (int)(offset >> 32),
            };

            return LockFileEx(fileStream.SafeFileHandle,
                (exclusive ? LOCKFILE_EXCLUSIVE_LOCK : 0) + (failImmediately ? LOCKFILE_FAIL_IMMEDIATELY : 0),
                0, countLow, countHigh, ref overlapped);
        }

        throw new PlatformNotSupportedException("Non-Windows platforms are not supported for TryLockFile.");
    }

    /// <summary>Non-blocking lock acquisition that classifies failure for retry decisions.</summary>
    public static FileLockResult TryAcquireFile(FileStream fileStream, long offset, long count, bool exclusive)
    {
        if (OperatingSystem.IsWindows())
        {
            var countLow = (uint)count;
            var countHigh = (uint)(count >> 32);

            var overlapped = new NativeOverlapped()
            {
                OffsetLow = (int)(offset & uint.MaxValue),
                OffsetHigh = (int)(offset >> 32),
            };

            if (LockFileEx(fileStream.SafeFileHandle,
                (exclusive ? LOCKFILE_EXCLUSIVE_LOCK : 0) | LOCKFILE_FAIL_IMMEDIATELY,
                0, countLow, countHigh, ref overlapped))
            {
                return FileLockResult.Acquired;
            }
            return Marshal.GetLastWin32Error() == ERROR_LOCK_VIOLATION
                ? FileLockResult.Contended
                : FileLockResult.Unsupported;
        }

        throw new PlatformNotSupportedException("Non-Windows platforms are not supported for TryAcquireFile.");
    }

    /// <summary>
    ///   Unlocks a byte range of a file previously locked with <see cref="TryLockFile"/>.
    /// </summary>
    public static void TryUnlockFile(FileStream fileStream, long offset, long count)
    {
        if (OperatingSystem.IsWindows())
        {
            var countLow = (uint)count;
            var countHigh = (uint)(count >> 32);

            var overlapped = new NativeOverlapped()
            {
                OffsetLow = (int)(offset & uint.MaxValue),
                OffsetHigh = (int)(offset >> 32),
            };

            if (!UnlockFileEx(fileStream.SafeFileHandle, 0, countLow, countHigh, ref overlapped))
            {
                throw new IOException("Couldn't unlock file.");
            }
        }
    }
}
