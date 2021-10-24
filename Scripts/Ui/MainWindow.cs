using CSC473.Lib;
using Godot;

namespace CSC473.Scripts.Ui
{
    public class MainWindow : Panel
    {
        // Menu item ids
        enum FileMenuItem
        {
            Load,
            Save,
            SaveAs,
            Quit
        }

        private StateManager _stateManager;
        private Viewport _viewport3d;

        private PopupMenu _fileMenu;

        // // overrides

        public override void _Ready()
        {
            // get node refs
            _viewport3d = GetNode<Viewport>("OuterMargin/MainContainer/VPSidebar" +
                                            "/ViewportContainer/Viewport");

            _fileMenu = GetNode<MenuButton>("OuterMargin/MainContainer/MenuButtons/File").GetPopup();

            // preload 3d viewport scene (could use ResourceInteractiveLoader in future)
            Node root3d = ResourceLoader.Load<PackedScene>("res://3DView.tscn").Instance();
            _viewport3d.AddChild(root3d);

            _stateManager = GetNode<StateManager>("/root/StateManager");

            // mouse sensitivity settings
            Slider mSensSlider = GetNode<Slider>("OuterMargin/MainContainer/VPSidebar/SideBar/MouseSens");
            mSensSlider.Connect("value_changed", this, nameof(_MouseSensChanged));
            GetNode<Label>("OuterMargin/MainContainer/VPSidebar/SideBar/LMouseSens")
                .Text = "Mouse Sensitivity: " + mSensSlider.Value;

            // populate menubar
            _fileMenu.AddItem("Load Layout", (int) FileMenuItem.Load);
            _fileMenu.AddItem("Save Layout", (int) FileMenuItem.Save);
            _fileMenu.AddItem("Save Layout As", (int) FileMenuItem.SaveAs);
            _fileMenu.AddSeparator();
            _fileMenu.AddItem("Quit", (int) FileMenuItem.Quit);

            // shortcuts
            ShortCut sc = ActionToShortCut("ui_save");
            int idx = _fileMenu.GetItemIndex((int) FileMenuItem.Save);
            _fileMenu.SetItemShortcut(idx, sc);

            sc = ActionToShortCut("ui_save_as");
            idx = _fileMenu.GetItemIndex((int) FileMenuItem.SaveAs);
            _fileMenu.SetItemShortcut(idx, sc);

            sc = ActionToShortCut("ui_load");
            idx = _fileMenu.GetItemIndex((int) FileMenuItem.Load);
            _fileMenu.SetItemShortcut(idx, sc);

            sc = ActionToShortCut("ui_quit");
            idx = _fileMenu.GetItemIndex((int) FileMenuItem.Quit);
            _fileMenu.SetItemShortcut(idx, sc);

            // menu bar callbacks
            _fileMenu.Connect("id_pressed", this, nameof(_FileMenuCallback));
        }

        // // Callbacks

        public void _MouseSensChanged(float value)
        {
            // updated mouse sens to the state manager
            _stateManager.SMouseSensitivity = value;

            // update label
            GetNode<Label>("OuterMargin/MainContainer/VPSidebar/SideBar/LMouseSens")
                .Text = "Mouse Sensitivity: " + value;
        }

        public void _FileMenuCallback(int id)
        {
            // id is the id of the item
            FileMenuItem fmId = (FileMenuItem) id;

            // 0.1sec timer to prevent winapi file dialogs glitching out
            Timer tmr = new Timer();
            tmr.WaitTime = 0.1f;
            tmr.OneShot = true;
            tmr.Autostart = true;
            
            // TODO: block input in main window when win32 file dialog open

            switch (fmId)
            {
                case FileMenuItem.Load:
                {
                    tmr.Connect("timeout", this, nameof(Load));
                    AddChild(tmr);
                    break;
                }
                case FileMenuItem.Save:
                {
                    tmr.Connect("timeout", this, nameof(SaveAs));
                    AddChild(tmr);
                    break;
                }
            }
        }

        // for godot filedialog
        public void _OpenFileSelected(string path)
        {
            GD.Print(path);
        }

        // for godot filedialog
        public void _SaveFileSelected(string path)
        {
            GD.Print(path);
        }

        // // public
        
        // return value of OpenFileDialog and SaveFileDialog is complicated
        // non-empty string means it was a windows dialog and a file was selected
        // null means it was a windows dialog and no file was selected
        // empty string means we're not on windows and a non-blocking file dialog has opened.

        public void Load()
        {
            PlatformFileDialog.FileFilter[] filters = new PlatformFileDialog.FileFilter[1];
            filters[0] = new PlatformFileDialog.FileFilter();
            filters[0].Desc = "Layout";
            filters[0].Ext = "*.tsl";
            GD.Print(PlatformFileDialog.OpenFileDialog(filters, "Load Layout", this, nameof(_OpenFileSelected)));
        }

        public void SaveAs()
        {
            PlatformFileDialog.FileFilter[] filters = new PlatformFileDialog.FileFilter[1];
            filters[0] = new PlatformFileDialog.FileFilter();
            filters[0].Desc = "Layout";
            filters[0].Ext = "*.tsl";
            GD.Print(PlatformFileDialog.SaveFileDialog(filters, "Save Layout", this, nameof(_OpenFileSelected)));
        }

        // // statics

        private static ShortCut ActionToShortCut(string action)
        {
            ShortCut sc = new ShortCut();

            // this will throw some kind of array index exception if the action string is not found
            InputEvent ev = (InputEvent) InputMap.GetActionList(action)[0];
            sc.Shortcut = ev;
            return sc;
        }
    }
}