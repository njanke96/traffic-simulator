using System;
using System.Collections.Generic;
using CSC473.Lib;
using Godot;

namespace CSC473.Scripts
{
    public enum ToolType
    {
        Select,
        AddNode,
        AddHintObject,
        LinkNodes,
        DeleteNode
    }
    
    /// <summary>
    /// A singleton instantiated by the engine, manages global state.
    /// </summary>
    public class StateManager : Node
    {
        // // Public state

        public float SMouseSensitivity = 5.0f;

        private RandomNumberGenerator _rng;
        public string RngSeed
        {
            get => _RngSeedAsString();
            set => _RngSeedFromString(value);
        }
        
        private bool _controllingCamera;
        public bool ControllingCamera
        {
            get => _controllingCamera;
            set
            {
                _controllingCamera = value;
                EmitSignal(nameof(ControllingCameraChanged), value);
            }
        }

        private ToolType _currentTool = ToolType.Select;
        public ToolType CurrentTool
        {
            get => _currentTool;
            set
            {
                // clear current selection
                CurrentSelection = null;
                
                // signal
                _currentTool = value;
                EmitSignal(nameof(ToolTypeChanged), value);

                // tool change status message
                var toolNameMap = new Dictionary<ToolType, string>
                {
                    [ToolType.Select] = "Select",
                    [ToolType.AddNode] = "Add Node",
                    [ToolType.DeleteNode] = "Delete Node",
                    [ToolType.LinkNodes] = "Link Nodes",
                    [ToolType.AddHintObject] = "Add Hint Object"
                };

                EmitSignal(nameof(StatusLabelChangeRequest), $"Current tool: {toolNameMap[value]}");
                
                // ensure this is reset
                LinkNodeU = null;
            }
        }

        private ISelectable _currentSelection;
        public ISelectable CurrentSelection
        {
            get => _currentSelection;
            set
            {
                _currentSelection = value;
                EmitSignal(nameof(SelectionChanged));
            }
        }

        // the first node in a U->V link set with the link tool
        public PathNode LinkNodeU;

        private bool _nodesVisible;
        public bool NodesVisible
        {
            get => _nodesVisible;
            set
            {
                _nodesVisible = value;
                EmitSignal(nameof(NodeVisChanged));
            }
        }
        
        // the traffic light channel that is currently green
        public int CurrentGreenChannel;
        public float LightTimerTimeout
        {
            get => _trafficTimer.WaitTime;
            set => _trafficTimer.WaitTime = value;
        }

        public bool ShortestPathNeedsRebuild;

        // // signals

        [Signal]
        public delegate void StatusLabelChangeRequest(string newText);

        [Signal]
        public delegate void ControllingCameraChanged(bool enabled);

        [Signal]
        public delegate void ToolTypeChanged(ToolType newTool);

        [Signal]
        public delegate void GroundPlaneClicked(Vector3 where);

        [Signal]
        public delegate void SelectionChanged();

        [Signal]
        public delegate void NodeVisChanged();

        [Signal]
        public delegate void ResetVehicleSimulation();

        // //

        private Timer _trafficTimer;

        public StateManager()
        {
            // start with a random seed from system random
            _rng = new RandomNumberGenerator();
            _rng.Seed = (ulong) new Random().Next() + (ulong) new Random().Next();
        }

        public override void _Ready()
        {
            // create the traffic light timer
            _trafficTimer = new Timer();
            _trafficTimer.PauseMode = PauseModeEnum.Stop;
            _trafficTimer.Autostart = true;
            _trafficTimer.WaitTime = 10f;
            _trafficTimer.Connect("timeout", this, nameof(TimerTimeout));
            AddChild(_trafficTimer);
        }

        public override void _Process(float delta)
        {
            // selection validity checks
            if (!IsInstanceValid((Godot.Object) CurrentSelection))
            {
                CurrentSelection = null;
                return;
            }

            Node selectedNode = (Node) CurrentSelection;
            if (selectedNode.IsQueuedForDeletion())
            {
                CurrentSelection = null;
            }
        }

        /// <summary>
        /// Generate a random integer getween min and max (inclusive).
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public int RandInt(int min, int max)
        {
            // wrapping rng instead of exposing it as a public member because the object in memory
            // changes as the random seed changes.
            return _rng.RandiRange(min, max);
        }

        public void TimerTimeout()
        {
            // swap green channels
            CurrentGreenChannel = CurrentGreenChannel == 0 ? 1 : 0;
        }

        public void ResetVehicles()
        {
            EmitSignal(nameof(ResetVehicleSimulation));
        }

        private string _RngSeedAsString()
        {
            return BitConverter.ToString(BitConverter.GetBytes(_rng.Seed)).Replace("-", "");
        }

        private void _RngSeedFromString(string str)
        {
            // when the seed is changed the random number generator is re-instantiated
            _rng = new RandomNumberGenerator();
            
            // hex str to bytes
            byte[] bytes = new byte[str.Length / 2];
            for (int i = 0; i < str.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            }

            _rng.Seed = BitConverter.ToUInt64(bytes, 0);
        }
    }
    
    
}
