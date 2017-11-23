using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ZebraFix
{
    class Program
    {
        static void Main() {
            IntPtr hPrinter = IntPtr.Zero;
            Win32Spool.PRINTER_DEFAULTS printerDefaults = new Win32Spool.PRINTER_DEFAULTS();
            Win32Spool.PRINTER_INFO_3 printerInfo = new Win32Spool.PRINTER_INFO_3();
            int cbNeeded = 0;
            try
            {
                string printerName = "Fax";
                IntPtr pPrinterInfo = IntPtr.Zero;
                printerDefaults.pDatatype = IntPtr.Zero;
                printerDefaults.pDevMode = IntPtr.Zero;
                printerDefaults.DesiredAccess = Win32Spool.PRINTER_EXECUTE;
                if (!Win32Spool.OpenPrinter(printerName, out hPrinter, ref printerDefaults))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                if (!Win32Spool.GetPrinter(hPrinter, 3, IntPtr.Zero, 0, out cbNeeded))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != Win32Spool.ERROR_INSUFFICIENT_BUFFER)
                    {
                        throw new Win32Exception(error);
                    }
                    pPrinterInfo = Marshal.AllocHGlobal(cbNeeded);
                    if (!Win32Spool.GetPrinter(hPrinter, 3, pPrinterInfo, cbNeeded, out cbNeeded))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    printerInfo = (Win32Spool.PRINTER_INFO_3)Marshal.PtrToStructure(pPrinterInfo, typeof(Win32Spool.PRINTER_INFO_3));
                    Console.WriteLine(printerInfo.pSecurityDescriptor.dacl.ToString());

                }
            }
            catch (Exception ex)

            {
                // Show errors
                Console.WriteLine(ex.Message);
            }

        }

    }
}
