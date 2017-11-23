using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ZebraFix
{
    class Win32Spool
    {
        // define access values
        // from https://msdn.microsoft.com/en-us/library/cc244650.aspx
        public const uint PRINTER_ACCESS_ADMINISTER = 0x00000004; // this one is not sufficient for even reading security descriptors
        public const uint PRINTER_ACCESS_USE = 0x00000008; // for basic operations
        public const uint PRINTER_ALL_ACCESS = 0x000F000C; // statement "to perform all administrative tasks and basic printing operations except synchronization" is a false one, you still wouldn't be able to read security info.
        public const uint PRINTER_EXECUTE = 0x00020008; // we'll be using this, as it's sufficient at least for getting security descriptor; TODO: check for setting it
        //some errors
        public const uint ERROR_INSUFFICIENT_BUFFER = 122;
        public const uint ERROR_IO_PENDING = 997;
        public const uint ERROR_FILE_NOT_FOUND = 0x80070002;
        
        [StructLayout(LayoutKind.Sequential)]
        public struct PRINTER_DEFAULTS
        {
            public IntPtr pDatatype;
            public IntPtr pDevMode;
            public uint DesiredAccess;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct PRINTER_OPTIONS
        {
            public uint cbSize;
            public IntPtr dwFlags;
        }
        
        // INFO_1 is for general Info, simple fields
        [StructLayout(LayoutKind.Sequential)]
        public struct PRINTER_INFO_1
        {
            public uint Flags;
            public string pDescription, pName, pComment;
        }
/*
 * TODO: implement DEVMODE
 *      // INFO_2 is a detailed info, care for buffer size here could extend to 3MB or even more
        [StructLayout(LayoutKind.Sequential)]
        public struct PRINTER_INFO_2
        {
            public string pServername,pPrinterName,pShareName,pPortName,pComment,pLocation,pSepFile,pPrintProcessor,pDatatype,pParameters;
            public uint Attributes, Priority, DefaultPriority, StartTime, UntilTime, Status, cJobs, AveragePPM;
        }
*/
        
        // INFO_3 is for security info only
        [StructLayout(LayoutKind.Sequential)]
        public struct PRINTER_INFO_3
        {
            public SECURITY_DESCRIPTOR pSecurityDescriptor;
        }

        // INFO_4 is for minimal general info
        // Attributes is really behave more like a Bool type but it really returns DWORD. Some workaround maybe?
        // There's only two values of Attributes: LOCAL and NETWORK though.
        [StructLayout(LayoutKind.Sequential)]
        public struct PRINTER_INFO_4
        {
            public string pPrinterName;
            public string pServerName;
            public uint Attributes;
        }

        // INFO_5 is for detailed info with more ATTRIBUTE VALUE
        // TODO: implement INFO_5

        // INFO_6 is for printer status codes
        [StructLayout(LayoutKind.Sequential)]
        public struct PRINTER_INFO_6
        {
            public uint dwStatus;
        }

        // INFO_7 is for directory services info
        [StructLayout(LayoutKind.Sequential)]
        public struct PRINTER_INFO_7
        {
            public IntPtr pszObjectGUID;
            public uint dwAction;
        }

        // INFO_8 is for global default settings
        // TODO: Implement INFO_8 | after DEVMODE as this one needs DEVMODE to function

        // INFO_9 is for user specific settings
        // TODO: Implement INFO_9 | same with DEVMODE

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "OpenPrinterW")]
        public static extern bool OpenPrinter(
            string pPrinterName,
            out IntPtr phPrinter,
            ref PRINTER_DEFAULTS pDefault
        );

        // this was a test, still not sure if I need this
        // but it works
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "OpenPrinter2W")]
        public static extern bool OpenPrinter2(
            string pPrintername,
            out IntPtr phPrinter,
            ref PRINTER_DEFAULTS pDefault,
            ref PRINTER_OPTIONS pOptions
            );

/*      TODO: implement SetPrinter(), needs more testing on GetPrinter though;  
 *      [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool SetPrinter(
            IntPtr hPrinter,
            int Level,
            ref PRINTER_INFO_# pPrinter,
            int Command
        );
*/
        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool GetPrinter(
            IntPtr hPrinter,
            int Level,
            IntPtr pPrinter,
            int cbBuf,
            out int pcbNeeded
        );

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool ClosePrinter(
            IntPtr hPrinter
        );

        // TODO: check for actual values in last 4 fields, this seems pretty odd;
        // maybe chose other type
        [StructLayoutAttribute(LayoutKind.Sequential)] 
        public struct SECURITY_DESCRIPTOR
        {
            public byte revision;
            public byte size;
            public short control;
            public IntPtr owner;
            public IntPtr group;
            public IntPtr sacl;
            public IntPtr dacl;
        }

    }
}