using System.Runtime.InteropServices;

namespace Openthesia.FileDialogs;

public class OpenFileDialog
{
    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private class OpenFileName
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public string filter;
        public string customFilter;
        public int maxCustFilter = 0;
        public int filterIndex = 0;
        public IntPtr file;
        public int maxFile = 0;
        public string fileTitle;
        public int maxFileTitle = 0;
        public string initialDir;
        public string title;
        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;
        public string defExt;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public string templateName;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }

    private enum OpenFileNameFlags
    {
        OFN_HIDEREADONLY = 0x4,
        OFN_FORCESHOWHIDDEN = 0x10000000,
        OFN_ALLOWMULTISELECT = 0x200,
        OFN_EXPLORER = 0x80000,
        OFN_FILEMUSTEXIST = 0x1000,
        OFN_PATHMUSTEXIST = 0x800
    }

    public string Title { get; set; } = "Open a file...";
    public bool Multiselect { get; set; } = false;
    public string InitialDirectory { get; set; } = null;
    public string Filter { get; set; } = "All files(*.*)\0\0";
    public bool ShowHidden { get; set; } = false;
    public bool Success { get; private set; }
    public string[] Files { get; private set; }


    /// <summary>
    /// Open a single file
    /// </summary>
    /// <param name="file">Path to the selected file, or null if the return value is false</param>
    /// <param name="title">Title of the dialog</param>
    /// <param name="filter">File name filter. Example : "txt files (*.txt)|*.txt|All files (*.*)|*.*"</param>
    /// <param name="initialDirectory">Example : "c:\\"</param>
    /// <param name="showHidden">Forces the showing of system and hidden files</param>
    /// <returns>True of a file was selected, false if the dialog was cancelled or closed</returns>
    public static bool OpenFile(out string file, string title = null, string filter = null, string initialDirectory = null, bool showHidden = false)
    {
        OpenFileDialog dialog = new OpenFileDialog();
        dialog.Title = title;
        dialog.InitialDirectory = initialDirectory;
        dialog.Filter = filter;
        dialog.ShowHidden = showHidden;

        dialog.OpenDialog();
        if (dialog.Success)
        {
            file = dialog.Files[0];
            return true;
        }

        file = null;
        return false;
    }

    /// <summary>
    /// Open multiple files
    /// </summary>
    /// <param name="files">Paths to the selected files, or null if the return value is false</param>
    /// <param name="title">Title of the dialog</param>
    /// <param name="filter">File name filter. Example : "txt files (*.txt)|*.txt|All files (*.*)|*.*"</param>
    /// <param name="initialDirectory">Example : "c:\\"</param>
    /// <param name="showHidden">Forces the showing of system and hidden files</param>
    /// <returns>True of one or more files were selected, false if the dialog was cancelled or closed</returns>
    public static bool OpenFiles(out string[] files, string title = null, string filter = null, string initialDirectory = null, bool showHidden = false)
    {
        OpenFileDialog dialog = new OpenFileDialog();
        dialog.Title = title;
        dialog.InitialDirectory = initialDirectory;
        dialog.Filter = filter;
        dialog.ShowHidden = showHidden;
        dialog.Multiselect = true;

        dialog.OpenDialog();
        if (dialog.Success)
        {
            files = dialog.Files;
            return true;
        }

        files = null;
        return false;
    }

    private void OpenDialog()
    {
        Thread thread = new Thread(() => ShowOpenFileDialog());
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    public void ShowOpenFileDialog()
    {
        const int MAX_FILE_LENGTH = 2048;

        Success = false;
        Files = null;

        OpenFileName ofn = new OpenFileName();

        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = Filter?.Replace("|", "\0") + "\0";
        ofn.fileTitle = new string(new char[MAX_FILE_LENGTH]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        ofn.initialDir = InitialDirectory;
        ofn.title = Title;
        ofn.flags = (int)OpenFileNameFlags.OFN_HIDEREADONLY | (int)OpenFileNameFlags.OFN_EXPLORER | (int)OpenFileNameFlags.OFN_FILEMUSTEXIST | (int)OpenFileNameFlags.OFN_PATHMUSTEXIST;

        // Create buffer for file names
        ofn.file = Marshal.AllocHGlobal(MAX_FILE_LENGTH * Marshal.SystemDefaultCharSize);
        ofn.maxFile = MAX_FILE_LENGTH;

        // Initialize buffer with NULL bytes
        for (int i = 0; i < MAX_FILE_LENGTH * Marshal.SystemDefaultCharSize; i++)
        {
            Marshal.WriteByte(ofn.file, i, 0);
        }

        if (ShowHidden)
        {
            ofn.flags |= (int)OpenFileNameFlags.OFN_FORCESHOWHIDDEN;
        }

        if (Multiselect)
        {
            ofn.flags |= (int)OpenFileNameFlags.OFN_ALLOWMULTISELECT;
        }

        Success = GetOpenFileName(ofn);

        if (Success)
        {
            IntPtr filePointer = ofn.file;
            long pointer = (long)filePointer;
            string file = Marshal.PtrToStringAuto(filePointer);
            List<string> strList = new List<string>();

            // Retrieve file names
            while (file.Length > 0)
            {
                strList.Add(file);

                pointer += file.Length * Marshal.SystemDefaultCharSize + Marshal.SystemDefaultCharSize;
                filePointer = (IntPtr)pointer;
                file = Marshal.PtrToStringAuto(filePointer);
            }

            if (strList.Count > 1)
            {
                Files = new string[strList.Count - 1];
                for (int i = 1; i < strList.Count; i++)
                {
                    Files[i - 1] = Path.Combine(strList[0], strList[i]);
                }
            }
            else
            {
                Files = strList.ToArray();
            }
        }

        Marshal.FreeHGlobal(ofn.file);
    }
}
