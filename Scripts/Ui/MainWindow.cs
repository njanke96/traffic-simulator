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
			Quit
		}
		
		private StateManager _stateManager;
		private Viewport _viewport3d;

		private PopupMenu _fileMenu;
		
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
			_fileMenu.AddItem("Load Layout", (int)FileMenuItem.Load);
			_fileMenu.AddItem("Save Layout", (int)FileMenuItem.Save);
			_fileMenu.AddSeparator();
			_fileMenu.AddItem("Quit", (int)FileMenuItem.Quit);
			
			// shortcuts
			ShortCut ctrls = new ShortCut();
			InputEvent ctrlsAction = (InputEvent)InputMap.GetActionList("ui_save")[0];
			ctrls.Shortcut = ctrlsAction;
			int ctrlsIndex = _fileMenu.GetItemIndex((int) FileMenuItem.Save);
			_fileMenu.SetItemShortcut(ctrlsIndex, ctrls);
		}
		
		public void _MouseSensChanged(float value)
		{
			// updated mouse sens to the state manager
			_stateManager.SMouseSensitivity = value;
			
			// update label
			GetNode<Label>("OuterMargin/MainContainer/VPSidebar/SideBar/LMouseSens")
				.Text = "Mouse Sensitivity: " + value;
		}
	}
}
