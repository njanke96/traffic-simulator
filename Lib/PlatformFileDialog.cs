using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;

namespace CSC473.Lib
{
    /// <summary>
    /// This class provides open and save file dialogs through the windows api when on windows,
    /// falling back to the Godot GUI Library on other platforms. The purpose of this is to be able to
    /// use the windows GUI dialogs when available.
    ///
    /// https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/w5tyztk9(v=vs.100)?redirectedfrom=MSDN
    /// </summary>
    public static class PlatformFileDialog
    {
        public struct FileFilter
        {
            public string Desc;
            public string Ext;
        }
#if WINDOWS
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName
        {
            public int       structSize = 0;
            public IntPtr    hwnd = IntPtr.Zero;
            public IntPtr    hinst = IntPtr.Zero;
            public string    filter = null;
            public string    custFilter = null;
            public int       custFilterMax = 0;
            public int       filterIndex = 0;
            public string    file = null;
            public int       maxFile = 0;
            public string    fileTitle = null;
            public int       maxFileTitle = 0;
            public string    initialDir = null;
            public string    title = null;
            public int       flags = 0;
            public short     fileOffset = 0;
            public short     fileExtMax = 0;
            public string    defExt = null;
            public int       custData = 0;
            public IntPtr    pHook = IntPtr.Zero;
            public string    template = null;
        }

        private static class LibWrap
        {
            // Declare a managed prototype for the unmanaged function.
            [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
            public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
            
            [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
            public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
        }

        /// <summary>
        /// Open a file dialog, returning the path to the file or null on fail;
        /// </summary>
        /// <param name="filters">array of FileFilter structs</param>
        /// <param name="title">file dialog title</param>
        /// <param name="initialdir">file dialog initial dir</param>
        /// <returns></returns>
        public static string OpenFileDialog(FileFilter[] filters, string title, Node parent, string callback, string initialdir
 = null)
        {
            OpenFileName ofn = new OpenFileName();
            ofn.structSize = Marshal.SizeOf(ofn);
            ofn.filter = WinFilters(filters);
            ofn.file = new string(new char[256]);
            ofn.maxFile = ofn.file.Length;
            ofn.initialDir = initialdir;
            ofn.title = title;

            if (LibWrap.GetOpenFileName(ofn))
            {
                return ofn.file;
            }
            
            // call failed
            return null;
        }

        public static string SaveFileDialog(FileFilter[] filters, string title, Node parent, string callback, string initialdir
 = null)
        {
            OpenFileName ofn = new OpenFileName();
            ofn.structSize = Marshal.SizeOf(ofn);
            ofn.filter = WinFilters(filters);
            ofn.file = new string(new char[256]);
            ofn.maxFile = ofn.file.Length;
            ofn.initialDir = initialdir;
            ofn.title = title;

            if (LibWrap.GetSaveFileName(ofn))
            {
                return ofn.file;
            }
            
            // call failed
            return null;
        }

        private static string WinFilters(IEnumerable<FileFilter> filters)
        {
            string filterstr = "";
            foreach (FileFilter filter in filters)
            {
                filterstr += filter.Desc + " " + "(" + filter.Ext + ")\0" + filter.Ext + "\0";
            }

            filterstr += "All files (*.*)\0*.*";
            return filterstr;
        }
#else
        public static string OpenFileDialog(FileFilter[] filters, string title, Node parent, string callback,
            string initialdir = null)
        {
            FileDialog fd = new FileDialog();
            fd.Access = FileDialog.AccessEnum.Filesystem;
            fd.Filters = GdFilters(filters);
            fd.Mode = FileDialog.ModeEnum.OpenFile;
            fd.ModeOverridesTitle = false;
            fd.WindowTitle = title;

            if (initialdir != null)
                fd.CurrentDir = initialdir;

            fd.Connect("file_selected", parent, callback);
            parent.AddChild(fd);
            fd.PopupCentered(new Vector2(600f, 400f));

            return "";
        }

        public static string SaveFileDialog(FileFilter[] filters, string title, Node parent, string callback,
            string initialdir = null)
        {
            FileDialog fd = new FileDialog();
            fd.Access = FileDialog.AccessEnum.Filesystem;
            fd.Filters = GdFilters(filters);
            fd.Mode = FileDialog.ModeEnum.SaveFile;
            fd.ModeOverridesTitle = false;
            fd.WindowTitle = title;

            if (initialdir != null)
                fd.CurrentDir = initialdir;

            fd.Connect("file_selected", parent, callback);
            parent.AddChild(fd);
            fd.PopupCentered(new Vector2(600f, 400f));
            
            return "";
        }

        private static string[] GdFilters(IEnumerable<FileFilter> filters)
        {
            List<string> lfilters = new List<string>();

            foreach (FileFilter filter in filters)
            {
                lfilters.Add(filter.Ext + " ; " + filter.Desc);
            }

            return lfilters.ToArray();
        }
#endif
    }
}