using Godot;
using System;

namespace CSC473.Scripts
{
    /// <summary>
    /// A camera controller using Quats and Transforms to handle mouse and keyboard input.
    /// </summary>
    public class CamController : Node
    {
        private StateManager _stateManager;
        
        // camera acceleration rate
        private const float CameraAccel = 20.0f;
        
        // max camera speed before modifier
        private const float MaxCameraSpeed = 10.0f;

        // origin to translate the camera to
        private Vector3 _origin;
        
        // refers to the Camera
        private Camera _camera;
        
        // max camera speed
        private float _cameraMaxSpeed = MaxCameraSpeed;

        // in degrees, 90 is looking straight up, -90 straight down
        private double _pitch = -30.0;
        
        // in degrees, 0 to 359, clockwise positive.
        private double _yaw;

        // camera controls
        private Vector3 _velocity;

        public override void _Ready()
        {
            _stateManager = GetNode<StateManager>("/root/StateManager");
            
            // reference to the camera
            _camera = GetParent<Camera>();
            
            _origin = new Vector3(0, 20, 40);
            _velocity = Vector3.Zero;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseMotion eventMouseMotion)
            {
                if (!_stateManager.ControllingCamera)
                    return;
                
                float mSens = _stateManager.SMouseSensitivity;
                
                // pitch and yaw
                double newPitch = _pitch - (mSens / 10.0) * eventMouseMotion.Relative.y;
                double newYaw = _yaw + (mSens / 10.0) * eventMouseMotion.Relative.x;

                // clamping
                newPitch = newPitch > 90.0 ? 90.0 : newPitch;
                newPitch = newPitch < -90.0 ? -90.0 : newPitch;

                if (newYaw > 359.0)
                    newYaw -= 360.0;
                else if (newYaw < 0.0)
                    newYaw += 360.0;

                _pitch = newPitch;
                _yaw = newYaw;
            }
            else if (@event is InputEventKey)
            {
                if (Input.IsActionPressed("cam_takecontrol"))
                {
                    if (!_stateManager.ControllingCamera)
                    {
                        Input.SetMouseMode(Input.MouseMode.Captured);
                        _stateManager.ControllingCamera = true;
                    }
                    else
                    {
                        Input.SetMouseMode(Input.MouseMode.Visible);
                        _stateManager.ControllingCamera = false;
                    }
                }
                else if (Input.IsActionPressed("ui_cancel"))
                {
                    Input.SetMouseMode(Input.MouseMode.Visible);
                    _stateManager.ControllingCamera = false;
                }
            }
        }

        public override void _Process(float delta)
        {
            //// Camera Rotation
            
            // y-axis rotation (yaw)
            Quat yrot = new Quat(new Vector3(0.0f, 1.0f, 0.0f), (float) ((Math.PI / 180) * -1 * _yaw));
            
            // x-axis rotation (pitch)
            Quat xrot = new Quat(new Vector3(1.0f, 0.0f, 0.0f), (float) ((Math.PI / 180) * _pitch));

            // compose
            Quat rot = yrot * xrot;

            // transform
            Transform t = new Transform(Basis.Identity, _origin);
            t *= new Transform(rot, Vector3.Zero);

            //// Camera velocity

            bool w = Input.IsActionPressed("cam_forward") && _stateManager.ControllingCamera,
                a = Input.IsActionPressed("cam_left") && _stateManager.ControllingCamera,
                s = Input.IsActionPressed("cam_backward") && _stateManager.ControllingCamera,
                d = Input.IsActionPressed("cam_right") && _stateManager.ControllingCamera;

            if (s && !w)
            {
                // +z
                if (_velocity.z < 0.0f)
                    _velocity.z = 0.0f;

                _velocity.z += CameraAccel * delta;
            }
            else if (w && !s)
            {
                // -z
                if (_velocity.z > 0.0f)
                    _velocity.z = 0.0f;

                _velocity.z -= CameraAccel * delta;
            }
            else
            {
                // decelerate z
                if (_velocity.z > 0.01)
                {
                    _velocity.z -= CameraAccel * delta;
                }
                else if (_velocity.z < -0.01)
                {
                    _velocity.z += CameraAccel * delta;
                }
                else
                {
                    _velocity.z = 0.0f;
                }
            }
            
            if (d && !a)
            {
                // +x
                if (_velocity.x < 0.0f)
                    _velocity.x = 0.0f;

                _velocity.x += CameraAccel * delta;
            }
            else if (a && !d)
            {
                // -x
                if (_velocity.x > 0.0f)
                    _velocity.x = 0.0f;

                _velocity.x -= CameraAccel * delta;
            }
            else
            {
                // decelerate x
                if (_velocity.x > 0.01)
                {
                    _velocity.x -= CameraAccel * delta;
                }
                else if (_velocity.x < -0.01)
                {
                    _velocity.x += CameraAccel * delta;
                }
                else
                {
                    _velocity.x = 0.0f;
                }
            }
            
            // clamp velocity
            if (_velocity.Length() > _cameraMaxSpeed)
                _velocity = _velocity.Normalized() * _cameraMaxSpeed;
            
            // modifier
            if (Input.IsActionPressed("cam_speed_modifier") && _stateManager.ControllingCamera)
                _cameraMaxSpeed = MaxCameraSpeed * 2;
            else
                _cameraMaxSpeed = MaxCameraSpeed;

            t = t.Translated(_velocity * delta);
            _origin = new Vector3(t.origin);

            // apply to transform
            _camera.Transform = t;
        }
    }
}


