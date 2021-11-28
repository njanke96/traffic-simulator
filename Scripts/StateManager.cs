using System;
using System.Collections.Generic;
using System.IO;
using CSC473.Lib;
using Godot;
using Directory = System.IO.Directory;

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
        
        // the traffic light channel that is currently green, do not set it without also setting _lastGreenChannel
        public int CurrentGreenChannel;
        public float LightTimerTimeout = 10f;

        private int _lastGreenChannel;

        public bool ShortestPathNeedsRebuild;

        public string LastSavePath = null;

        // stats
        public bool IsLogging;
        private float _logElapsedTime; // seconds
        private int _vehiclesSinceLast;
        public float FramesPerSecond;
        public int VehicleCount;
        public float TotalTravelled;
        private Timer _logUpdateTimer;

        private FileStream _logFileStream;

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

        [Signal]
        public delegate void StatsUpdated();

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
            _trafficTimer.OneShot = false;
            _trafficTimer.Autostart = true;
            _trafficTimer.WaitTime = LightTimerTimeout;
            _trafficTimer.Connect("timeout", this, nameof(TimerTimeout));
            AddChild(_trafficTimer); 
            
            // logging timer (once per second forever regardless of pause state)
            _logUpdateTimer = new Timer();
            _logUpdateTimer.PauseMode = PauseModeEnum.Process;
            _logUpdateTimer.Autostart = true;
            _logUpdateTimer.Connect("timeout", this, nameof(LogTimerTimeout));
            AddChild(_logUpdateTimer);
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
            if (CurrentGreenChannel != -1)
            {
                // short delay of all red lights
                CurrentGreenChannel = -1;
                _trafficTimer.WaitTime = 3f;
                _trafficTimer.Start();
                return;
            }

            // swap green channels
            CurrentGreenChannel = _lastGreenChannel == 0 ? 1 : 0;
            _lastGreenChannel = _lastGreenChannel == 0 ? 1 : 0;
            _trafficTimer.WaitTime = LightTimerTimeout;
            _trafficTimer.Start();
        }

        public void LogTimerTimeout()
        {
            // update stats
            FramesPerSecond = Engine.GetFramesPerSecond();
            TotalTravelled += _vehiclesSinceLast;
            _vehiclesSinceLast = 0;
            EmitSignal(nameof(StatsUpdated));
            
            // logging to csv file
            if (IsLogging)
            {
                _logElapsedTime += 1f;
                
                // check for open logfile stream, make a new one if needed
                if (_logFileStream != null)
                {
                    // one is open
                    if (!_logFileStream.CanWrite)
                    {
                        // something happened to the file
                        _logFileStream = null;
                        return;
                    }

                    try
                    {
                        StreamWriter sw = new StreamWriter(_logFileStream);
                        sw.WriteLine($"{_logElapsedTime},{FramesPerSecond},{VehicleCount},{TotalTravelled}");
                        sw.Flush();
                    }
                    catch (IOException e)
                    {
                        GD.PushWarning($"Could not write to log file: {e}");
                    }
                }
                else
                {
                    try
                    {
                        string fileName = "TSIM_" + DateTime.Now.ToString("yyyy-MM-ddTHH_mm_ss") + ".csv";
                        Directory.CreateDirectory(OS.GetUserDataDir());
                        string fullPath = string.Join("/", OS.GetUserDataDir(), fileName);
                        _logFileStream = new FileStream(fullPath, FileMode.Create);
                    
                        // write csv header
                        StreamWriter sw = new StreamWriter(_logFileStream);
                        sw.WriteLine("Elapsed,FPS,TotalVehicles,TotalReached");
                        sw.Flush();
                    }
                    catch (Exception e)
                    {
                        GD.PushWarning($"Could not open or write to log file: {e}");
                    }
                }
            }
            else
            {
                _logElapsedTime = 0;
                
                // if a logfile stream is open, dispose it
                if (_logFileStream == null) return;
                
                _logFileStream.Dispose();
                _logFileStream = null;
            }
        }

        public void VehicleReached()
        {
            _vehiclesSinceLast += 1;
        }

        public void ResetVehicles()
        {
            EmitSignal(nameof(ResetVehicleSimulation));
            
            // reset rng state
            _rng.State = 0;
            
            // reset reach count
            TotalTravelled = 0;
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
