using System;
using System.Collections.Generic;
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

        // the first node in a U->V link set with the link tool
        public PathNode LinkNodeU;

        // // signals

        [Signal]
        public delegate void StatusLabelChangeRequest(string newText);

        [Signal]
        public delegate void ControllingCameraChanged(bool enabled);

        [Signal]
        public delegate void ToolTypeChanged(ToolType newTool);

        [Signal]
        public delegate void GroundPlaneClicked(Vector3 where);

        // //

        public StateManager()
        {
            // start with a random seed from system random
            _rng = new RandomNumberGenerator();
            _rng.Seed = (ulong) new Random().Next() + (ulong) new Random().Next();
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
