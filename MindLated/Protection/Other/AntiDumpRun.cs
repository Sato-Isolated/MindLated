using System;
using System.Runtime.InteropServices;

namespace MindLated.Protection.Other
{
    internal class AntiDumpRun
    {
        internal enum MemoryProtection
        {
            ExecuteReadWrite = 0x40,
        }

        public static unsafe void CopyBlock(void* destination, void* source, uint byteCount)
        {
        }

        public static unsafe void InitBlock(void* startAddress, byte value, uint byteCount)
        {
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool VirtualProtect(
            IntPtr lpAddress,
            uint dwSize,
            [MarshalAs(UnmanagedType.U4)] MemoryProtection flNewProtect,
            [MarshalAs(UnmanagedType.U4)] out MemoryProtection lpflOldProtect);

        private static unsafe void Initialize()
        {
            var module = typeof(AntiDumpRun).Module;
            var bas = (byte*)Marshal.GetHINSTANCE(module);
            var ptr = bas + 0x3c;
            ptr = bas + *(uint*)ptr;
            ptr += 0x6;
            var sectNum = *(ushort*)ptr;
            ptr += 14;
            var optSize = *(ushort*)ptr;
            ptr = ptr + 0x4 + optSize;
            var @new = stackalloc byte[11];
            if (module.FullyQualifiedName[0] != '<')
            {
                var mdDir = bas + *(uint*)(ptr - 16);
                if (*(uint*)(ptr - 0x78) != 0)
                {
                    var importDir = bas + *(uint*)(ptr - 0x78);
                    var oftMod = bas + *(uint*)importDir;
                    var modName = bas + *(uint*)(importDir + 12);
                    var funcName = bas + *(uint*)oftMod + 2;
                    VirtualProtect(new IntPtr(modName), 11, MemoryProtection.ExecuteReadWrite, out _);
                    *(uint*)@new = 0x6c64746e;
                    *((uint*)@new + 1) = 0x6c642e6c;
                    *((ushort*)@new + 4) = 0x006c;
                    *(@new + 10) = 0;
                    CopyBlock(modName, @new, 11);
                    VirtualProtect(new IntPtr(funcName), 11, MemoryProtection.ExecuteReadWrite, out _);
                    *(uint*)@new = 0x6f43744e;
                    *((uint*)@new + 1) = 0x6e69746e;
                    *((ushort*)@new + 4) = 0x6575;
                    *(@new + 10) = 0;
                    CopyBlock(funcName, @new, 11);
                }

                for (var i = 0; i < sectNum; i++)
                {
                    VirtualProtect(new IntPtr(ptr), 8, MemoryProtection.ExecuteReadWrite, out _);
                    InitBlock(ptr, 0, 8);
                    ptr += 0x28;
                }

                VirtualProtect(new IntPtr(mdDir), 0x48, MemoryProtection.ExecuteReadWrite, out _);
                var mdHdr = bas + *(uint*)(mdDir + 8);
                InitBlock(mdDir, 0, 16);
                VirtualProtect(new IntPtr(mdHdr), 4, MemoryProtection.ExecuteReadWrite, out _);
                *(uint*)mdHdr = 0;
                mdHdr += 12;
                mdHdr += *(uint*)mdHdr;
                mdHdr = (byte*)(((ulong)mdHdr + 7) & ~3UL);
                mdHdr += 2;
                ushort numOfStream = *mdHdr;
                mdHdr += 2;
                for (var i = 0; i < numOfStream; i++)
                {
                    VirtualProtect(new IntPtr(mdHdr), 8, MemoryProtection.ExecuteReadWrite, out _);
                    mdHdr += 4;
                    mdHdr += 4;
                    for (var ii = 0; ii < 8; ii++)
                    {
                        VirtualProtect(new IntPtr(mdHdr), 4, MemoryProtection.ExecuteReadWrite, out _);
                        *mdHdr = 0;
                        mdHdr++;
                        if (*mdHdr == 0)
                        {
                            mdHdr += 3;
                            break;
                        }
                        *mdHdr = 0;
                        mdHdr++;
                        if (*mdHdr == 0)
                        {
                            mdHdr += 2;
                            break;
                        }
                        *mdHdr = 0;
                        mdHdr++;
                        if (*mdHdr == 0)
                        {
                            mdHdr += 1;
                            break;
                        }
                        *mdHdr = 0;
                        mdHdr++;
                    }
                }
            }
            else
            {
                var mdDir = *(uint*)(ptr - 16);
                var importDir = *(uint*)(ptr - 0x78);

                var vAdrs = new uint[sectNum];
                var vSizes = new uint[sectNum];
                var rAdrs = new uint[sectNum];
                for (var i = 0; i < sectNum; i++)
                {
                    VirtualProtect(new IntPtr(ptr), 8, MemoryProtection.ExecuteReadWrite, out _);
                    Marshal.Copy(new byte[8], 0, (IntPtr)ptr, 8);
                    vAdrs[i] = *(uint*)(ptr + 12);
                    vSizes[i] = *(uint*)(ptr + 8);
                    rAdrs[i] = *(uint*)(ptr + 20);
                    ptr += 0x28;
                }

                if (importDir != 0)
                {
                    for (var i = 0; i < sectNum; i++)
                        if (vAdrs[i] <= importDir && importDir < vAdrs[i] + vSizes[i])
                        {
                            importDir = importDir - vAdrs[i] + rAdrs[i];
                            break;
                        }

                    var importDirPtr = bas + importDir;
                    var oftMod = *(uint*)importDirPtr;
                    for (var i = 0; i < sectNum; i++)
                        if (vAdrs[i] <= oftMod && oftMod < vAdrs[i] + vSizes[i])
                        {
                            oftMod = oftMod - vAdrs[i] + rAdrs[i];
                            break;
                        }

                    var oftModPtr = bas + oftMod;
                    var modName = *(uint*)(importDirPtr + 12);
                    for (var i = 0; i < sectNum; i++)
                        if (vAdrs[i] <= modName && modName < vAdrs[i] + vSizes[i])
                        {
                            modName = modName - vAdrs[i] + rAdrs[i];
                            break;
                        }

                    var funcName = *(uint*)oftModPtr + 2;
                    for (var i = 0; i < sectNum; i++)
                        if (vAdrs[i] <= funcName && funcName < vAdrs[i] + vSizes[i])
                        {
                            funcName = funcName - vAdrs[i] + rAdrs[i];
                            break;
                        }

                    VirtualProtect(new IntPtr(bas + modName), 11, MemoryProtection.ExecuteReadWrite, out _);

                    *(uint*)@new = 0x6c64746e;
                    *((uint*)@new + 1) = 0x6c642e6c;
                    *((ushort*)@new + 4) = 0x006c;
                    *(@new + 10) = 0;

                    CopyBlock(bas + modName, @new, 11);

                    VirtualProtect(new IntPtr(bas + funcName), 11, MemoryProtection.ExecuteReadWrite, out _);

                    *(uint*)@new = 0x6f43744e;
                    *((uint*)@new + 1) = 0x6e69746e;
                    *((ushort*)@new + 4) = 0x6575;
                    *(@new + 10) = 0;

                    CopyBlock(bas + funcName, @new, 11);
                }

                for (var i = 0; i < sectNum; i++)
                    if (vAdrs[i] <= mdDir && mdDir < vAdrs[i] + vSizes[i])
                    {
                        mdDir = mdDir - vAdrs[i] + rAdrs[i];
                        break;
                    }

                var mdDirPtr = bas + mdDir;
                VirtualProtect(new IntPtr(mdDirPtr), 0x48, MemoryProtection.ExecuteReadWrite, out _);
                var mdHdr = *(uint*)(mdDirPtr + 8);
                for (var i = 0; i < sectNum; i++)
                    if (vAdrs[i] <= mdHdr && mdHdr < vAdrs[i] + vSizes[i])
                    {
                        mdHdr = mdHdr - vAdrs[i] + rAdrs[i];
                        break;
                    }

                InitBlock(mdDirPtr, 0, 16);

                var mdHdrPtr = bas + mdHdr;
                VirtualProtect(new IntPtr(mdHdrPtr), 4, MemoryProtection.ExecuteReadWrite, out _);
                *(uint*)mdHdrPtr = 0;
                mdHdrPtr += 12;
                mdHdrPtr += *(uint*)mdHdrPtr;
                mdHdrPtr = (byte*)(((ulong)mdHdrPtr + 7) & ~3UL);
                mdHdrPtr += 2;
                ushort numOfStream = *mdHdrPtr;
                mdHdrPtr += 2;
                for (var i = 0; i < numOfStream; i++)
                {
                    VirtualProtect(new IntPtr(mdHdrPtr), 8, MemoryProtection.ExecuteReadWrite, out _);
                    mdHdrPtr += 4;
                    mdHdrPtr += 4;
                    for (var ii = 0; ii < 8; ii++)
                    {
                        VirtualProtect(new IntPtr(mdHdrPtr), 4, MemoryProtection.ExecuteReadWrite, out _);
                        *mdHdrPtr = 0;
                        mdHdrPtr++;
                        if (*mdHdrPtr == 0)
                        {
                            mdHdrPtr += 3;
                            break;
                        }

                        *mdHdrPtr = 0;
                        mdHdrPtr++;
                        if (*mdHdrPtr == 0)
                        {
                            mdHdrPtr += 2;
                            break;
                        }

                        *mdHdrPtr = 0;
                        mdHdrPtr++;
                        if (*mdHdrPtr == 0)
                        {
                            mdHdrPtr += 1;
                            break;
                        }

                        *mdHdrPtr = 0;
                        mdHdrPtr++;
                    }
                }
            }
        }
    }
}