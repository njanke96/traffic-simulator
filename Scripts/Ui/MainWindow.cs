using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using CSC473.Lib;
using Godot;

namespace CSC473.Scripts.Ui
{
    public class MainWindow : Panel
    {
        // Menu item ids
        private enum FileMenuItem
        {
            Load,
            Save,
            SaveAs,
            Quit
        }

        private StateManager _stateManager;
        private Viewport _viewport3d;
        private Label _statusLabel;
        
        // sidebar
        private OptionButton _nodeType;
        private LineEdit _speedLimit;
        private LineEdit _minSpawnTimer;
        private LineEdit _maxSpawnTimer;
        private Label _nodeEditHint;

        private Label _vehicleClass;
        private Label _vehicleSpeed;

        private OptionButton _objType;
        private OptionButton _lightChannel;
        private Slider _hintObjRot;
        private Label _hintObjRotLabel;
        private LineEdit _randSeed;

        private PopupMenu _fileMenu;
        private CheckBox _nodesVisible;

        private Label _lightTimerLabel;
        private Slider _lightTimer;
        
        // toolbar
        private Button _playButton;
        private Button _pauseButton;
        private Button _resetButton;
        private Button _selectButton;
        private Button _addNodeButton;
        private Button _addHintObjButton;
        private Button _linkNodesButton;
        private Button _deleteNodeButton;

        // // overrides

        public override void _Ready()
        {
            // sidebar node path
            string sidebarPath = "OuterMargin/MainContainer/VPSidebar/ScrollContainer/SideBar";
            
            // state manager
            _stateManager = GetNode<StateManager>("/root/StateManager");
            _stateManager.Connect(nameof(StateManager.ToolTypeChanged), this, nameof(_CurrentToolChanged));
            _stateManager.Connect(nameof(StateManager.StatusLabelChangeRequest), this,
                nameof(_StatusLabelChangeRequested));

            _stateManager.Connect(nameof(StateManager.SelectionChanged), this, nameof(_SelectionChanged));

            // // get node refs
            _viewport3d = GetNode<Viewport>("OuterMargin/MainContainer/VPSidebar" +
                                            "/ViewportContainer/Viewport");

            _fileMenu = GetNode<MenuButton>("OuterMargin/MainContainer/MenuButtons/File").GetPopup();

            _statusLabel = GetNode<Label>("OuterMargin/MainContainer/StatusContainer/Label");
            
            // toolbar

            _playButton = GetNode<Button>("OuterMargin/MainContainer/ToolBar/Play");
            _playButton.Connect("button_down", this, nameof(_PlayClicked));
            
            _pauseButton = GetNode<Button>("OuterMargin/MainContainer/ToolBar/Pause");
            _pauseButton.Connect("button_down", this, nameof(_PauseClicked));

            _resetButton = GetNode<Button>("OuterMargin/MainContainer/ToolBar/Restart");
            _resetButton.Connect("button_down", this, nameof(_ResetClicked));
            
            _selectButton = GetNode<Button>("OuterMargin/MainContainer/ToolBar/Select");
            _addNodeButton = GetNode<Button>("OuterMargin/MainContainer/ToolBar/AddNode");
            _addHintObjButton = GetNode<Button>("OuterMargin/MainContainer/ToolBar/AddHintObject");
            _linkNodesButton = GetNode<Button>("OuterMargin/MainContainer/ToolBar/LinkNodes");
            _deleteNodeButton = GetNode<Button>("OuterMargin/MainContainer/ToolBar/DeleteNode");
            
            _selectButton.Connect("button_down", this, nameof(_ToolButtonClicked), 
                new Godot.Collections.Array(ToolType.Select));
            _addNodeButton.Connect("button_down", this, nameof(_ToolButtonClicked), 
                new Godot.Collections.Array(ToolType.AddNode));
            _addHintObjButton.Connect("button_down", this, nameof(_ToolButtonClicked), 
                new Godot.Collections.Array(ToolType.AddHintObject));
            _linkNodesButton.Connect("button_down", this, nameof(_ToolButtonClicked), 
                new Godot.Collections.Array(ToolType.LinkNodes));
            _deleteNodeButton.Connect("button_down", this, nameof(_ToolButtonClicked), 
                new Godot.Collections.Array(ToolType.DeleteNode));
            
            
            // sidebar
            _nodeType = GetNode<OptionButton>(sidebarPath + "/TypeContainer/Type");
            _nodeType.AddItem("Enroute", (int)PathNodeType.Enroute);
            _nodeType.AddItem("Start", (int)PathNodeType.Start);
            _nodeType.AddItem("End", (int)PathNodeType.End);

            _speedLimit = GetNode<LineEdit>(sidebarPath + "/SpeedLimitContainer/SpeedLimit");
            _minSpawnTimer = GetNode<LineEdit>(sidebarPath + "/MinSpawnTimerContainer/MinSpawnTimer");
            _maxSpawnTimer = GetNode<LineEdit>(sidebarPath + "/MaxSpawnTimerContainer/MaxSpawnTimer");
            _nodeEditHint = GetNode<Label>(sidebarPath + "/NodeUpdateHint");
            
            _vehicleClass = GetNode<Label>(sidebarPath + "/LVehClass");
            _vehicleSpeed = GetNode<Label>(sidebarPath + "/LVehSpeed");

            _objType = GetNode<OptionButton>(sidebarPath + "/ObjTypeContainer/ObjType");
            _objType.AddItem("Traffic Light", (int) HintObjectType.TrafficLight);
            
            _lightChannel = GetNode<OptionButton>(sidebarPath + "/LightChannelContainer/LightChannel");
            _lightChannel.AddItem("Channel 0", 0);
            _lightChannel.AddItem("Channel 1", 1);

            _hintObjRot = GetNode<Slider>(sidebarPath + "/ObjRotation");
            _hintObjRotLabel = GetNode<Label>(sidebarPath + "/LObjRotation");
            _hintObjRot.Connect("value_changed", this, nameof(_HintObjRotChanged));
            
            _randSeed = GetNode<LineEdit>(sidebarPath + "/RandomSeedContainer/RandomSeed");
            _randSeed.Text = _stateManager.RngSeed;
            _randSeed.Connect("text_changed", this, nameof(_RandSeedChanged));

            _nodesVisible = GetNode<CheckBox>(sidebarPath + "/NodesVisible");
            _nodesVisible.Connect("pressed", this, nameof(_NodeVisibilityChange));
            
            _lightTimerLabel = GetNode<Label>(sidebarPath + "/LLightTime");
            _lightTimer = GetNode<Slider>(sidebarPath + "/LightTime");
            _lightTimer.Connect("value_changed", this, nameof(_LightTimerChanged));
            
            // path node attribute changes
            _nodeType.Connect("item_selected", this, nameof(_PathNodeAttrChanged));
            _speedLimit.Connect("text_entered", this, nameof(_PathNodeAttrChanged));
            _minSpawnTimer.Connect("text_entered", this, nameof(_PathNodeAttrChanged));
            _maxSpawnTimer.Connect("text_entered", this, nameof(_PathNodeAttrChanged));
            
            // hint object attribute changes
            _lightChannel.Connect("item_selected", this, nameof(_HintObjectAttrChanged));
            _hintObjRot.Connect("value_changed", this, nameof(_HintObjectAttrChanged));

            // // preload 3d viewport scene (could use ResourceInteractiveLoader in future)
            Node root3d = ResourceLoader.Load<PackedScene>("res://3DView.tscn").Instance();
            _viewport3d.AddChild(root3d);

            // mouse sensitivity settings
            Slider mSensSlider = GetNode<Slider>(sidebarPath + "/MouseSens");
            mSensSlider.Connect("value_changed", this, nameof(_MouseSensChanged));
            GetNode<Label>(sidebarPath + "/LMouseSens")
                .Text = "Mouse Sensitivity: " + mSensSlider.Value;

            // controlling camera status label
            _stateManager.Connect(nameof(StateManager.ControllingCameraChanged), this, 
                nameof(_ControllingCameraChanged));
            _statusLabel.Text = "Press F2 to control the camera.";

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

            // force control states and default values
            SetNodeControlsEnabled(false);
            SetHintObjControlsEnabled(false);
        }

        public override void _Process(float delta)
        {
            if (_stateManager.CurrentSelection is Vehicle vehicle)
            {
                // a vehicle is selected and needs its speed updated
                UpdateVehicleClassAndSpeed($"{vehicle.ClassName}", 
                    $"{(int) vehicle.Speed} km/h");
            }
            else
            {
                UpdateVehicleClassAndSpeed(null, null);
            }
        }

        // // Callbacks

        public void _PlayClicked()
        {
            GetTree().Paused = false;
            _playButton.Disabled = true;
            _pauseButton.Disabled = false;
        }

        public void _PauseClicked()
        {
            GetTree().Paused = true;
            _playButton.Disabled = false;
            _pauseButton.Disabled = true;
        }

        public void _ResetClicked()
        {
            _stateManager.ResetVehicles();
        }

        public void _ToolButtonClicked(ToolType tool)
        {
            EnableToolButtons();
            switch (tool)
            {
                case ToolType.Select:
                {
                    _stateManager.CurrentTool = ToolType.Select;
                    _selectButton.Disabled = true;
                    break;
                }
                case ToolType.AddNode:
                {
                    _stateManager.CurrentTool = ToolType.AddNode;
                    _addNodeButton.Disabled = true;
                    break;
                }
                case ToolType.AddHintObject:
                {
                    _stateManager.CurrentTool = ToolType.AddHintObject;
                    _addHintObjButton.Disabled = true;
                    break;
                }
                case ToolType.LinkNodes:
                {
                    _stateManager.CurrentTool = ToolType.LinkNodes;
                    _linkNodesButton.Disabled = true;
                    break;
                }
                case ToolType.DeleteNode:
                {
                    _stateManager.CurrentTool = ToolType.DeleteNode;
                    _deleteNodeButton.Disabled = true;
                    break;
                }
            }
        }

        public void _NodeVisibilityChange()
        {
            _stateManager.NodesVisible = _nodesVisible.Pressed;
        }

        public void _RandSeedChanged(string newText)
        {
            // dont update unless the seed entered is exactly 16 characters
            if (newText.Length != 16)
            {
                return;
            }
            
            string seed = String.Copy(newText);
            string hexa = "ABCDEFabcdef0123456789";
            foreach (char c in newText)
            {
                if (!hexa.Contains("" + c))
                {
                    // invalid characters are replaced with 0
                    seed = seed.Replace(c, '0');
                }
            }

            // set and update
            _stateManager.RngSeed = seed;
            _randSeed.Text = _stateManager.RngSeed;
        }

        public void _HintObjRotChanged(float value)
        {
            // state manager
            
            // update Label
            _hintObjRotLabel.Text = "Object Rotation: " + (int)value;
        }

        public void _MouseSensChanged(float value)
        {
            // updated mouse sens to the state manager
            _stateManager.SMouseSensitivity = value;

            // update label
            GetNode<Label>("OuterMargin/MainContainer/VPSidebar/ScrollContainer/SideBar/LMouseSens")
                .Text = "Mouse Sensitivity: " + value;
        }

        public void _ControllingCameraChanged(bool controlling)
        {
            _statusLabel.Text = 
                controlling ? "Press F2 or Esc to stop controlling the camera. WASD to move the camera, holding shift moves the camera faster." 
                    : "Press F2 to control the camera.";
        }

        public void _StatusLabelChangeRequested(string newText)
        {
            _statusLabel.Text = newText;
        }

        public void _CurrentToolChanged(ToolType newTool)
        {
            SetNodeControlsEnabled(false);
            SetHintObjControlsEnabled(false);
            
            if (newTool == ToolType.AddNode)
            {
                SetNodeControlsEnabled(true);
            }
            else if (newTool == ToolType.AddHintObject)
            {
                SetHintObjControlsEnabled(true);
            }
        }

        public void _SelectionChanged()
        {
            ISelectable newSelection = _stateManager.CurrentSelection;
            if (newSelection == null)
            {
                if (_stateManager.CurrentTool == ToolType.Select)
                {
                    // with the select tool in use, nothing is selected
                    SetNodeControlsEnabled(false);
                    SetHintObjControlsEnabled(false);
                    UpdateVehicleClassAndSpeed(null, null);
                }

                // nothing more to do when nothing is selected
                return;
            }
            
            SetNodeControlsEnabled(false);
            SetHintObjControlsEnabled(false);
            UpdateVehicleClassAndSpeed(null, null);

            if (newSelection is PathNode pathNode)
            {
                // a path node was selected
                SetNodeControlsEnabled(true);
                
                // set control values
                _nodeType.Selected = (int) pathNode.NodeType;
                _speedLimit.Text = pathNode.SpeedLimit.ToString();
                _minSpawnTimer.Text = pathNode.SpawnMin.ToString(CultureInfo.InvariantCulture);
                _maxSpawnTimer.Text = pathNode.SpawnMax.ToString(CultureInfo.InvariantCulture);
            }
            else if (newSelection is HintObject hintObject)
            {
                // a hint object was selected
                SetHintObjControlsEnabled(true);
                
                // set control values
                _lightChannel.Selected = hintObject.Channel;
                _hintObjRot.Value = hintObject.HintRotation;
            }
            else if (newSelection is Vehicle vehicle)
            {
                // a vehicle was selected
                UpdateVehicleClassAndSpeed($"{vehicle.ClassName}", 
                    $"{(int) vehicle.Speed} km/h");
            }
        }

        public void _PathNodeAttrChanged(params object[] paramz)
        {
            if (_stateManager.CurrentTool != ToolType.Select || _stateManager.CurrentSelection == null)
                return;
            
            // get a new path node to copy the new settings from
            PathNode newNode = PathNodeFromSettings();

            if (!(_stateManager.CurrentSelection is PathNode selectedNode))
                return;

            selectedNode.NodeType = newNode.NodeType;
            selectedNode.SpeedLimit = newNode.SpeedLimit;
            selectedNode.SpawnMin = newNode.SpawnMin;
            selectedNode.SpawnMax = newNode.SpawnMax;
            
            selectedNode.RemoveVehicleSpawner();
            if (selectedNode.NodeType == PathNodeType.Start)
                selectedNode.CreateVehicleSpawner();

            _stateManager.ShortestPathNeedsRebuild = true;

            // thanks fella
            newNode.QueueFree();
        }

        public void _HintObjectAttrChanged(params object[] paramz)
        {
            if (_stateManager.CurrentTool != ToolType.Select || _stateManager.CurrentSelection == null)
                return;
            
            if (!(_stateManager.CurrentSelection is HintObject hintObject))
                return;

            hintObject.Channel = _lightChannel.Selected;
            hintObject.HintRotation = (int)_hintObjRot.Value;
        }

        public void _LightTimerChanged(float value)
        {
            _lightTimerLabel.Text = $"Traffic light timer: {(int)value}";
            _stateManager.LightTimerTimeout = value;
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
                    tmr.Connect("timeout", this, nameof(Save));
                    AddChild(tmr);
                    break;
                }
                case FileMenuItem.SaveAs:
                {
                    tmr.Connect("timeout", this, nameof(SaveAs));
                    AddChild(tmr);
                    break;
                }
                case FileMenuItem.Quit:
                {
                    GetTree().Quit(0);
                    break;
                }
            }
        }

        public void _OpenFileSelected(string path)
        {
            // find pathlayout
            PathLayout pathLayout = (PathLayout) FindNode("PathLayout", true, false);
            if (pathLayout == null)
                throw new NullReferenceException($"{nameof(pathLayout)} is null.");

            PathLayout newLayout;
            Stream stream = null;

            // handle deserialization and file i/o errors
            try
            {
                IFormatter formatter = new BinaryFormatter();
                stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                newLayout = (PathLayout) formatter.Deserialize(stream);
            }
            catch (Exception e)
            {
                if (e is SerializationException || e is IOException || e is TargetInvocationException)
                {
                    GD.PrintErr(e);
                    _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest), 
                        $"Could not open layout: {e.GetType().Name}. See log.");
                    return;
                }

                throw;
            }
            finally
            {
                stream?.Dispose();
            }
            
            
            // replace existing pathlayout godot node
            Node parent = pathLayout.GetParent();
            parent.RemoveChild(pathLayout);
            pathLayout.QueueFree();
            
            // add deserialized pathnodes and hintobjects to the scene tree
            newLayout.AddChildrenFromMemory();
            
            parent.AddChild(newLayout);
            newLayout.Name = nameof(PathLayout);

            // reset any vehicles and rng
            _stateManager.ResetVehicles();
            
            // rebuild the shortest paths dictionary
            _stateManager.ShortestPathNeedsRebuild = true;
            
            _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest), 
                $"Loaded {path}");
        }

        public void _SaveFileSelected(string path)
        {
            if (!path.EndsWith(".tsl"))
                path += ".tsl";

            // find pathlayout
            PathLayout pathLayout = (PathLayout) FindNode("PathLayout", true, false);
            if (pathLayout == null)
                throw new NullReferenceException($"{nameof(pathLayout)} is null.");

            Stream stream = null;
            
            // handle serialization and file i/o errors
            try
            {
                IFormatter formatter = new BinaryFormatter();
                stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                formatter.Serialize(stream, pathLayout);
            }
            catch (Exception e)
            {
                if (e is SerializationException || e is IOException || e is TargetInvocationException)
                {
                    GD.PrintErr(e);
                    _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest), 
                        $"Could not save layout: {e.GetType().Name}. See log.");
                    return;
                }

                throw;
            }
            finally
            {
                stream?.Dispose();
            }

            _stateManager.LastSavePath = path;
            _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest), 
                $"Saved {path}");
        }

        // // public

        /// <summary>
        /// Returns a pathnode from the path node settings inputs after sanitizing inputs
        /// </summary>
        /// <returns></returns>
        public PathNode PathNodeFromSettings()
        {
            // handle bad input
            int speedLimit;
            try
            {
                speedLimit = int.Parse(_speedLimit.Text);
            }
            catch (FormatException)
            {
                _speedLimit.Text = PathNode.DefaultSpeedLimit.ToString();
                speedLimit = PathNode.DefaultSpeedLimit;
            }
            
            float spawnMin;
            try
            {
                spawnMin = float.Parse(_minSpawnTimer.Text);
            }
            catch (FormatException)
            {
                _minSpawnTimer.Text = PathNode.DefaultSpawnMin.ToString(CultureInfo.InvariantCulture);
                spawnMin = PathNode.DefaultSpawnMin;
            }

            float spawnMax;
            try
            {
                spawnMax = float.Parse(_maxSpawnTimer.Text);
            }
            catch (FormatException)
            {
                _maxSpawnTimer.Text = PathNode.DefaultSpawnMax.ToString(CultureInfo.InvariantCulture);
                spawnMax = PathNode.DefaultSpawnMax;
            }

            return new PathNode((PathNodeType) _nodeType.Selected, speedLimit, spawnMin, spawnMax);
        }

        /// <summary>
        /// Returns a hint object from the hint object settings controls.
        /// </summary>
        /// <returns></returns>
        public HintObject HintObjectFromSettings()
        {
            return new HintObject((HintObjectType) _objType.Selected, _lightChannel.Selected, 
                (int) _hintObjRot.Value);
        }
        
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
            string path = 
                PlatformFileDialog.OpenFileDialog(filters, "Load Layout", this, nameof(_OpenFileSelected));
            if (path == null)
                return;
            
            if (path.Length > 0)
            {
                // path available immediately
                _OpenFileSelected(path);
            }
        }

        public void Save()
        {
            if (_stateManager.LastSavePath == null)
            {
                SaveAs();
                return;
            }
            
            _SaveFileSelected(_stateManager.LastSavePath);
        }

        public void SaveAs()
        {
            PlatformFileDialog.FileFilter[] filters = new PlatformFileDialog.FileFilter[1];
            filters[0] = new PlatformFileDialog.FileFilter();
            filters[0].Desc = "Layout";
            filters[0].Ext = "*.tsl";
            string path = 
                PlatformFileDialog.SaveFileDialog(filters, "Save Layout", this, nameof(_SaveFileSelected));
            if (path == null)
                return;

            if (path.Length > 0)
            {
                // path available immediately
                _SaveFileSelected(path);
            }
        }
        
        // // private

        /// <summary>
        /// Enables all tool buttons.
        /// </summary>
        private void EnableToolButtons()
        {
            Button[] buttons =
            {
                _selectButton,
                _addNodeButton,
                _addHintObjButton,
                _linkNodesButton,
                _deleteNodeButton
            };

            foreach (Button btn in buttons)
            {
                btn.Disabled = false;
            }
        }

        /// <summary>
        /// Sets enabled mode of UI elements related to the selected node.
        /// </summary>
        private void SetNodeControlsEnabled(bool enabled)
        {
            if (!enabled)
            {
                // restore default values
                _nodeType.Selected = (int) PathNodeType.Enroute;
                _speedLimit.Text = PathNode.DefaultSpeedLimit.ToString();
                _minSpawnTimer.Text = PathNode.DefaultSpawnMin.ToString(CultureInfo.InvariantCulture);
                _maxSpawnTimer.Text = PathNode.DefaultSpawnMax.ToString(CultureInfo.InvariantCulture);
                _nodeEditHint.Text = "";
            }
            else
            {
                if (_stateManager.CurrentTool == ToolType.Select)
                {
                    _nodeEditHint.Text = "Enter to apply.";
                }
                else
                {
                    _nodeEditHint.Text = "";
                }
            }

            _nodeType.Disabled = !enabled;
            _speedLimit.Editable = enabled;
            _minSpawnTimer.Editable = enabled;
            _maxSpawnTimer.Editable = enabled;
        }
        
        /// <summary>
        /// Sets enabled mode of UI elements related to the selected hint object.
        /// </summary>
        private void SetHintObjControlsEnabled(bool enabled)
        {
            if (!enabled)
            {
                // restore default values
                _objType.Selected = (int) HintObjectType.TrafficLight;
                _lightChannel.Selected = 0;
                _hintObjRot.Value = 0.0;
                _HintObjRotChanged(0.0f);
            }
            
            _objType.Disabled = !enabled;
            _lightChannel.Disabled = !enabled;
            _hintObjRot.Editable = enabled;
        }

        /// <summary>
        /// Update vehicle detail labels. Pass null to either argument to show N/A
        /// </summary>
        /// <param name="vehClass"></param>
        /// <param name="speed"></param>
        private void UpdateVehicleClassAndSpeed(string vehClass, string speed)
        {
            if (vehClass == null || speed == null)
            {
                _vehicleClass.Text = "Class: N/A";
                _vehicleSpeed.Text = "Speed: N/A";
            }
            
            _vehicleClass.Text = $"Class: {vehClass}";
            _vehicleSpeed.Text = $"Speed: {speed}";
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