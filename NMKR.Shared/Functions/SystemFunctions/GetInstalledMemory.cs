using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NMKR.Shared.Functions.SystemFunctions
{
    public static class GetInstalledMemory
    {
        public static void GetRamBytes(out ulong availableBytes, out ulong totalBytes)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                GetBytesCountOnLinux(out availableBytes, out totalBytes);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GetBytesCountOnWindows(out availableBytes, out totalBytes);
            }
            else
            {
                throw new NotImplementedException("Not implemented for OS: " + Environment.OSVersion);
            }
        }

        private static readonly object _winMemoryLock = new();
        private static readonly MEMORYSTATUSEX _memStatus = new();
        private static readonly object _linuxMemoryLock = new();
        private static readonly char[] _arrayForMemInfoRead = new char[200];

        public static void GetBytesCountOnLinux(out ulong availableBytes, out ulong totalBytes)
        {
            lock (_linuxMemoryLock) // lock because of reusing static fields due to optimization
            {
                totalBytes = GetBytesCountFromLinuxMemInfo(token: "MemTotal:", refreshFromFile: true);
                availableBytes = GetBytesCountFromLinuxMemInfo(token: "MemAvailable:", refreshFromFile: false);
            }
        }

        private static ulong GetBytesCountFromLinuxMemInfo(string token, bool refreshFromFile)
        {
            // NOTE: Using the linux file /proc/meminfo which is refreshed frequently and starts with:
            //MemTotal:        7837208 kB
            //MemFree:          190612 kB
            //MemAvailable:    5657580 kB

            var readSpan = _arrayForMemInfoRead.AsSpan();

            if (refreshFromFile)
            {
                using var fileStream = new FileStream("/proc/meminfo", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                using var reader = new StreamReader(fileStream, Encoding.UTF8, leaveOpen: true);

                reader.ReadBlock(readSpan);
            }

            var tokenIndex = readSpan.IndexOf(token);

            var fromTokenSpan = readSpan.Slice(tokenIndex + token.Length);

            var kbIndex = fromTokenSpan.IndexOf("kB");

            var notTrimmedSpan = fromTokenSpan.Slice(0, kbIndex);

            var trimmedSpan = notTrimmedSpan.Trim(' ');

            var kBytesCount = ulong.Parse(trimmedSpan);

            var bytesCount = kBytesCount * 1024;

            return bytesCount;
        }
        private static void GetBytesCountOnWindows(out ulong availableBytes, out ulong totalBytes)
        {
            lock (_winMemoryLock) // lock because of reusing the static class _memStatus
            {
                GlobalMemoryStatusEx(_memStatus);

                availableBytes = _memStatus.ullAvailPhys;
                totalBytes = _memStatus.ullTotalPhys;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In][Out] MEMORYSTATUSEX lpBuffer);
    }
}
