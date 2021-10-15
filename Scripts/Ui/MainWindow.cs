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

            if (fmId == FileMenuItem.Load)
            {
                GD.Print("Load callback!");
            }
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