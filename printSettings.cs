using System;
using System.Runtime.InteropServices;
using System.Drawing.Printing;
using System.ComponentModel;
using NowDocs.Util;

namespace CustomprinterSettings
{
    #region "Data structure"

    #region PRINTER_DEFAULTS
    [StructLayout(LayoutKind.Sequential)]
    public struct PRINTER_DEFAULTS
    {
        public int pDatatype;
        public int pDevMode;
        public int DesiredAccess;
    }
    #endregion PRINTER_DEFAULTS

    #region PRINTER_INFO_2
    [StructLayout(LayoutKind.Sequential)]
    struct PRINTER_INFO_2
    {
        [MarshalAs(UnmanagedType.LPStr)] public string pServerName;
        [MarshalAs(UnmanagedType.LPStr)] public string pPrinterName;
        [MarshalAs(UnmanagedType.LPStr)] public string pShareName;
        [MarshalAs(UnmanagedType.LPStr)] public string pPortName;
        [MarshalAs(UnmanagedType.LPStr)] public string pDriverName;
        [MarshalAs(UnmanagedType.LPStr)] public string pComment;
        [MarshalAs(UnmanagedType.LPStr)] public string pLocation;
        public IntPtr pDevMode;
        [MarshalAs(UnmanagedType.LPStr)] public string pSepFile;
        [MarshalAs(UnmanagedType.LPStr)] public string pPrintProcessor;
        [MarshalAs(UnmanagedType.LPStr)] public string pDatatype;
        [MarshalAs(UnmanagedType.LPStr)] public string pParameters;
        public IntPtr pSecurityDescriptor;
        public Int32 Attributes;
        public Int32 Priority;
        public Int32 DefaultPriority;
        public Int32 StartTime;
        public Int32 UntilTime;
        public Int32 Status;
        public Int32 cJobs;
        public Int32 AveragePPM;
    }
    #endregion PRINTER_INFO_2

    #region DEVMODE
    [StructLayout(LayoutKind.Sequential)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public short dmOrientation;
        public short dmPaperSize;
        public short dmPaperLength;
        public short dmPaperWidth;
        public short dmScale;
        public short dmCopies;
        public short dmDefaultSource;
        public short dmPrintQuality;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmUnusedPadding;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
    }
    #endregion DEVMODE

    #endregion "Data structure"

    #region PrinterConfiguration
    public class PrinterConfiguration
    {

        #region "Private Variables"
        private IntPtr _hPrinter = new System.IntPtr();
        private PRINTER_DEFAULTS _PrinterValues = new PRINTER_DEFAULTS();
        private PRINTER_INFO_2 _pinfo = new PRINTER_INFO_2();
        private DEVMODE _Devmode;
        private IntPtr _PtrDM;
        private IntPtr _PtrPrinterInfo;
        private int _SizeOfDevMode = 0;
        private int _LastError;
        private int _NBytesNeeded;
        private long _NRet;
        private int _IntError;
        private System.Int32 _NJunk;
        private IntPtr _YDevModeData;
        #endregion

        #region "Win API Def"

        [DllImport("kernel32.dll", EntryPoint = "GetLastError", SetLastError = false,
        ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern Int32 GetLastError();

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true,
        ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "DocumentPropertiesA", SetLastError = true,
        ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int DocumentProperties(IntPtr hwnd, IntPtr hPrinter,
        [MarshalAs(UnmanagedType.LPStr)] string pDeviceNameg,
        IntPtr pDevModeOutput, ref IntPtr pDevModeInput, int fMode);

        [DllImport("winspool.Drv", EntryPoint = "GetPrinterA", SetLastError = true, CharSet = CharSet.Ansi,
        ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool GetPrinter(IntPtr hPrinter, Int32 dwLevel,
        IntPtr pPrinter, Int32 dwBuf, out Int32 dwNeeded);

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi,
        ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter,
        out IntPtr hPrinter, ref PRINTER_DEFAULTS pd);

        [DllImport("winspool.drv", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern bool SetPrinter(IntPtr hPrinter, int Level, IntPtr
        pPrinter, int Command);


        #endregion

        #region "Constants"
        private const int DM_DUPLEX = 0x1000;
        private const int DM_IN_BUFFER = 8;
        private const int DM_OUT_BUFFER = 2;
        private const int PRINTER_ACCESS_ADMINISTER = 0x4;
        private const int PRINTER_ACCESS_USE = 0x8;
        private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        private const int PRINTER_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | PRINTER_ACCESS_ADMINISTER | PRINTER_ACCESS_USE);
        #endregion

        #region "Function to change printer settings"

        #region ChangePrinterPaperSize(string PrinterName,PaperSize paperSize)
        public bool ChangePrinterPaperSize(string printerName, PaperSize paperSize)
        {
            try
            {
                _Devmode = this.GetPrinterSettings(printerName);
                _Devmode.dmPaperSize = (short)paperSize.Kind;
                if (paperSize.Kind == PaperKind.Custom)
                {
                    _Devmode.dmPaperWidth =
                    Convert.ToInt16((double)Math.Round(((Convert.ToDouble(pageSettings.PaperSize.Width)) / 100D) * 25.4D * 10D));
                    _Devmode.dmPaperLength =
                    Convert.ToInt16((double)Math.Round((Convert.ToDouble(pageSettings.PaperSize.Height) / 100D) * 25.4D * 10D));
                }
                Marshal.StructureToPtr(_Devmode, _YDevModeData, true);
                _pinfo.pDevMode = _YDevModeData;
                _pinfo.pSecurityDescriptor = IntPtr.Zero;
                Marshal.StructureToPtr(_pinfo, _PtrPrinterInfo, true);
                _LastError = Marshal.GetLastWin32Error();
                _NRet = Convert.ToInt16(SetPrinter(_hPrinter, 2, _PtrPrinterInfo, 0));

                if (_NRet == 0)
                {
                    //Unable to set shared printer settings.

                    _LastError = Marshal.GetLastWin32Error();
                    //string myErrMsg = GetErrorMessage(_LastError);
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (_hPrinter != IntPtr.Zero)
                {

                    ClosePrinter(_hPrinter);
                }
            }

            return Convert.ToBoolean(_NRet);
        }
        #endregion ChangePrinterPaperSize(string PrinterName,PrinterData PS)

        #region ChangePrinterOrientation(string PrinterName,bool landscape)
        public bool ChangePrinterOrientation(string printerName, bool landscape)
        {

            try
            {
                _Devmode = this.GetPrinterSettings(printerName);
                _Devmode.dmOrientation = (short)((landscape) ? 2 : 1);
                Marshal.StructureToPtr(_Devmode, _YDevModeData, true);
                _pinfo.pDevMode = _YDevModeData;
                _pinfo.pSecurityDescriptor = IntPtr.Zero;
                Marshal.StructureToPtr(_pinfo, _PtrPrinterInfo, true);
                _LastError = Marshal.GetLastWin32Error();
                _NRet = Convert.ToInt16(SetPrinter(_hPrinter, 2, _PtrPrinterInfo, 0));

                if (_NRet == 0)
                {
                    //Unable to set shared printer settings.

                    _LastError = Marshal.GetLastWin32Error();
                    //string myErrMsg = GetErrorMessage(_LastError);
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (_hPrinter != IntPtr.Zero)
                {

                    ClosePrinter(_hPrinter);
                }
            }

            return Convert.ToBoolean(_NRet);
        }
        #endregion ChangePrinterOrientation(string PrinterName,bool landscape)

        #region ChangePrinterSetting(string PrinterName,PageSettings pageSettings)
        public bool ChangePrinterSetting(string PrinterName, PageSettings pageSettings)
        {
            try
            {
                _Devmode = this.GetPrinterSettings(PrinterName);
                _Devmode.dmDefaultSource = (short)pageSettings.PaperSource.Kind;
                _Devmode.dmOrientation = (short)((pageSettings.Landscape) ? 2 : 1);
                _Devmode.dmPaperSize = (short)pageSettings.PaperSize.Kind;
                if (pageSettings.PaperSize.Kind == PaperKind.Custom)
                {
                    _Devmode.dmPaperWidth =
                    Convert.ToInt16((double)Math.Round(((Convert.ToDouble(pageSettings.PaperSize.Width)) / 100D) * 25.4D * 10D));
                    _Devmode.dmPaperLength =
                    Convert.ToInt16((double)Math.Round((Convert.ToDouble(pageSettings.PaperSize.Height) / 100D) * 25.4D * 10D));
                }
                Marshal.StructureToPtr(_Devmode, _YDevModeData, true);
                _pinfo.pDevMode = _YDevModeData;
                _pinfo.pSecurityDescriptor = IntPtr.Zero;
                /*update driver dependent part of the DEVMODE
                1 = DocumentProperties(IntPtr.Zero, _hPrinter, sPrinterName, _YDevModeData
                , ref _pinfo.pDevMode, (DM_IN_BUFFER | DM_OUT_BUFFER));*/
                Marshal.StructureToPtr(_pinfo, _PtrPrinterInfo, true);
                _LastError = Marshal.GetLastWin32Error();
                _NRet = Convert.ToInt16(SetPrinter(_hPrinter, 2, _PtrPrinterInfo, 0));

                if (_NRet == 0)
                {
                    //Unable to set shared printer settings.

                    _LastError = Marshal.GetLastWin32Error();
                    //string myErrMsg = GetErrorMessage(_LastError);
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (_hPrinter != IntPtr.Zero)
                {

                    ClosePrinter(_hPrinter);
                }
            }
            return Convert.ToBoolean(_NRet);
        }

        #endregion ChangePrinterSetting(string PrinterName,PageSettings pageSettings)

        #region ChangePrinterSetting(string PrinterName,PageSettings pageSettings, Duplex duplex)
        public bool ChangePrinterSetting(string PrinterName, PageSettings pageSettings, Duplex duplex)
        {
            try
            {
                _Devmode = this.GetPrinterSettings(PrinterName);
                _Devmode.dmDefaultSource = (short)pageSettings.PaperSource.Kind;
                _Devmode.dmOrientation = (short)((pageSettings.Landscape) ? 2 : 1);
                _Devmode.dmPaperSize = (short)pageSettings.PaperSize.Kind;
                if (pageSettings.PaperSize.Kind == PaperKind.Custom)
                {
                    _Devmode.dmPaperWidth =
                    Convert.ToInt16((double)Math.Round(((Convert.ToDouble(pageSettings.PaperSize.Width)) / 100D) * 25.4D * 10D));
                    _Devmode.dmPaperLength =
                    Convert.ToInt16((double)Math.Round((Convert.ToDouble(pageSettings.PaperSize.Height) / 100D) * 25.4D * 10D));
                }
                _Devmode.dmDuplex = (short)duplex;
                Marshal.StructureToPtr(_Devmode, _YDevModeData, true);
                _pinfo.pDevMode = _YDevModeData;
                _pinfo.pSecurityDescriptor = IntPtr.Zero;
                /*update driver dependent part of the DEVMODE
                1 = DocumentProperties(IntPtr.Zero, _hPrinter, sPrinterName, _YDevModeData
                , ref _pinfo.pDevMode, (DM_IN_BUFFER | DM_OUT_BUFFER));*/
                Marshal.StructureToPtr(_pinfo, _PtrPrinterInfo, true);
                _LastError = Marshal.GetLastWin32Error();
                _NRet = Convert.ToInt16(SetPrinter(_hPrinter, 2, _PtrPrinterInfo, 0));

                if (_NRet == 0)
                {
                    //Unable to set shared printer settings.

                    _LastError = Marshal.GetLastWin32Error();
                    //string myErrMsg = GetErrorMessage(_LastError);
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (_hPrinter != IntPtr.Zero)
                {

                    ClosePrinter(_hPrinter);
                }
            }
            return Convert.ToBoolean(_NRet);
        }

        #endregion ChangePrinterSetting(string PrinterName,PageSettings pageSettings, bool landscape)

        #region ChangePrinterSetting(string PrinterName,Duplex duplex)
        public bool ChangePrinterSetting(string PrinterName, Duplex duplex)
        {
            try
            {
                _Devmode = this.GetPrinterSettings(PrinterName);
                _Devmode.dmDuplex = (short)duplex;
                Marshal.StructureToPtr(_Devmode, _YDevModeData, true);
                _pinfo.pDevMode = _YDevModeData;
                _pinfo.pSecurityDescriptor = IntPtr.Zero;
                /*update driver dependent part of the DEVMODE
                1 = DocumentProperties(IntPtr.Zero, _hPrinter, sPrinterName, _YDevModeData
                , ref _pinfo.pDevMode, (DM_IN_BUFFER | DM_OUT_BUFFER));*/
                Marshal.StructureToPtr(_pinfo, _PtrPrinterInfo, true);
                _LastError = Marshal.GetLastWin32Error();
                _NRet = Convert.ToInt16(SetPrinter(_hPrinter, 2, _PtrPrinterInfo, 0));

                if (_NRet == 0)
                {
                    //Unable to set shared printer settings.

                    _LastError = Marshal.GetLastWin32Error();
                    //string myErrMsg = GetErrorMessage(_LastError);
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (_hPrinter != IntPtr.Zero)
                {

                    ClosePrinter(_hPrinter);
                }
            }
            return Convert.ToBoolean(_NRet);
        }

        #endregion ChangePrinterSetting(string PrinterName,Duplex duplex)

        #region GetPrinterSettings(string PrinterName)
        private DEVMODE GetPrinterSettings(string PrinterName)
        {
            DEVMODE devmode;
            const int PRINTER_ACCESS_ADMINISTER = 0x4;
            const int PRINTER_ACCESS_USE = 0x8;
            const int PRINTER_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | PRINTER_ACCESS_ADMINISTER | PRINTER_ACCESS_USE);

            _PrinterValues.pDatatype = 0;
            _PrinterValues.pDevMode = 0;
            _PrinterValues.DesiredAccess = PRINTER_ALL_ACCESS;
            _NRet = Convert.ToInt32(OpenPrinter(PrinterName, out _hPrinter, ref _PrinterValues));
            if (_NRet == 0)
            {
                _LastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            GetPrinter(_hPrinter, 2, IntPtr.Zero, 0, out _NBytesNeeded);
            if (_NBytesNeeded <= 0)
            {
                throw new System.Exception("Unable to allocate memory");

            }
            else
            {
                // Allocate enough space for PRINTER_INFO_2... {ptrPrinterIn fo = Marshal.AllocCoTaskMem(_NBytesNeeded)};
                _PtrPrinterInfo = Marshal.AllocHGlobal(_NBytesNeeded);

                // The second GetPrinter fills in all the current settings, so all you // need to do is modify what you're interested in...
                _NRet = Convert.ToInt32(GetPrinter(_hPrinter, 2, _PtrPrinterInfo, _NBytesNeeded, out _NJunk));

                if (_NRet == 0)
                {
                    _LastError = Marshal.GetLastWin32Error();
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                _pinfo = (PRINTER_INFO_2)Marshal.PtrToStructure(_PtrPrinterInfo, typeof(PRINTER_INFO_2));
                IntPtr Temp = new IntPtr();
                if (_pinfo.pDevMode == IntPtr.Zero)
                {
                    // If GetPrinter didn't fill in the DEVMODE, try to get it by calling
                    // DocumentProperties...
                    IntPtr ptrZero = IntPtr.Zero;
                    //get the size of the devmode structure
                    _SizeOfDevMode = DocumentProperties(IntPtr.Zero, _hPrinter, PrinterName, ptrZero, ref ptrZero, 0);
                    _PtrDM = Marshal.AllocCoTaskMem(_SizeOfDevMode);
                    int i;
                    i = DocumentProperties(IntPtr.Zero, _hPrinter, PrinterName, _PtrDM, ref ptrZero, DM_OUT_BUFFER);
                    if ((i < 0) || (_PtrDM == IntPtr.Zero))
                    {
                        //Cannot get the DEVMODE structure.
                        throw new System.Exception("Cannot get DEVMODE data");
                    }
                    _pinfo.pDevMode = _PtrDM;
                }
                _IntError = DocumentProperties(IntPtr.Zero, _hPrinter, PrinterName, IntPtr.Zero, ref Temp, 0);
                //IntPtr _YDevModeData = Marshal.AllocCoTaskMem(i1);
                _YDevModeData = Marshal.AllocHGlobal(_IntError);
                _IntError = DocumentProperties(IntPtr.Zero, _hPrinter, PrinterName, _YDevModeData, ref Temp, 2);
                devmode = (DEVMODE)Marshal.PtrToStructure(_YDevModeData, typeof(DEVMODE));
                //_NRet = DocumentProperties(IntPtr.Zero, _hPrinter, sPrinterName, _YDevModeData
                // , ref _YDevModeData, (DM_IN_BUFFER | DM_OUT_BUFFER));
                if ((_NRet == 0) || (_hPrinter == IntPtr.Zero))

                {

                    _LastError = Marshal.GetLastWin32Error();
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                return devmode;
            }

        }
        #endregion GetPrinterSettings(string PrinterName)

        #endregion "Function to change printer settings"
    }
    #endregion PrinterConfiguration
}