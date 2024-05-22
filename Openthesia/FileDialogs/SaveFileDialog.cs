using System.Text;
using System.Runtime.InteropServices;

namespace Openthesia.FileDialogs;

public class SaveFileDialog
{
    // Constants for the SaveFileDialog
    private const int OFN_OVERWRITEPROMPT = 0x00000002;
    private const int OFN_FILEMUSTEXIST = 0x00001000;

    // P/Invoke declarations
    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetSaveFileName(ref OPENFILENAME lpofn);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct OPENFILENAME
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        [MarshalAs(UnmanagedType.LPTStr)] public string lpstrFilter;
        [MarshalAs(UnmanagedType.LPTStr)] public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        [MarshalAs(UnmanagedType.LPTStr)] public string lpstrFile;
        public int nMaxFile;
        [MarshalAs(UnmanagedType.LPTStr)] public string lpstrFileTitle;
        public int nMaxFileTitle;
        [MarshalAs(UnmanagedType.LPTStr)] public string lpstrInitialDir;
        [MarshalAs(UnmanagedType.LPTStr)] public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        [MarshalAs(UnmanagedType.LPTStr)] public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public IntPtr lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }

    public string FileName { get; private set; }

    public bool ShowDialog(string filter = "All Files\0*.*\0", string title = "Save As", string defaultExt = "", string initialDir = "")
    {
        OPENFILENAME ofn = new OPENFILENAME();
        StringBuilder fileBuffer = new StringBuilder(256);

        ofn.lStructSize = Marshal.SizeOf(typeof(OPENFILENAME));
        ofn.hwndOwner = IntPtr.Zero;
        ofn.lpstrFilter = filter;
        ofn.nFilterIndex = 1;
        ofn.lpstrFile = new string(new char[256]);
        ofn.nMaxFile = ofn.lpstrFile.Length;
        ofn.lpstrTitle = title;
        ofn.Flags = OFN_OVERWRITEPROMPT | OFN_FILEMUSTEXIST;
        ofn.lpstrDefExt = defaultExt;
        ofn.lpstrInitialDir = initialDir;

        if (GetSaveFileName(ref ofn))
        {
            FileName = ofn.lpstrFile;
            return true;
        }
        else
        {
            FileName = null;
            return false;
        }
    }
}
