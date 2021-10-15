using Godot;
using System;

namespace CSC473.Scripts.Ui
{
	public class MainWindow : Panel
	{
		private StateManager _stateManager;
		private Label _statusLabel;
		private Viewport _viewport3d;
		
		public override void _Ready()
		{
			// get node refs
			_viewport3d = GetNode<Viewport>("OuterMargin/MainContainer/VPSidebar" + 
													 "/ViewportContainer/Viewport");
			_statusLabel = GetNode<Label>("OuterMargin/MainContainer/StatusContainer/Label");
			
			// preload 3d viewport scene (could use ResourceInteractiveLoader in future)
			Node root3d = ResourceLoader.Load<PackedScene>("res://3DView.tscn").Instance();
			_viewport3d.AddChild(root3d);
			
			_stateManager = GetNode<StateManager>("/root/StateManager");
			
			// mouse sensitivity settings
			Slider mSensSlider = GetNode<Slider>("OuterMargin/MainContainer/VPSidebar/SideBar/MouseSens");
			mSensSlider.Connect("value_changed", this, nameof(_MouseSensChanged));
			GetNode<Label>("OuterMargin/MainContainer/VPSidebar/SideBar/LMouseSens")
				.Text = "Mouse Sensitivity: " + mSensSlider.Value;
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
