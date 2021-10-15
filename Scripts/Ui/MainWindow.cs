using Godot;
using System;

namespace CSC473.Scripts.Ui
{
	public class MainWindow : Panel
	{
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
		}
	}
}
