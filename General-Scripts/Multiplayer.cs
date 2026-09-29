using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace DeadMansDrift.General_Scripts;

public partial class Multiplayer : Node
{ 
    int _playerCount = Input.GetConnectedJoypads().Count;
    List<String> _players;
    
    
    public override void _Ready()
    {
        _playerCount = Input.GetConnectedJoypads().Count;

        Input.JoyConnectionChanged += OnJoyConnectionChanged;

        GD.Print($"Controllers connected: {_playerCount}");
    }

    private void OnJoyConnectionChanged(long device, bool connected)
    {
        _playerCount = Input.GetConnectedJoypads().Count;
        if (connected)
        {
            GD.Print("connected device " + device);
            // New controller connected
            // PackedScene scene = ResourceLoader.Load<PackedScene>("res://Scenes/vehicle.tscn");
            // Car player = scene.Instantiate<Car>();
            inputSetup((int)device, true);
            
        }
        else
        {
            GD.Print("disconnected device " + device);
            // Controller disconnected
            // Swap around players TODO Create UI and functions
        }   
    }

    void inputSetup(int device, bool isCar)
    {
        if (device % 2 == 0)
        {// + device
            InputMap.ActionAddEvent("forward", new InputEventJoypadMotion()
            {
                Device = device,
                Axis = JoyAxis.TriggerRight,
                AxisValue = 1.0f
            });
            InputMap.ActionAddEvent("back", new InputEventJoypadMotion()
            {
                Device = device,
                Axis = JoyAxis.TriggerLeft,
                AxisValue = 1.0f
            });
            InputMap.ActionAddEvent("hand_brake", new InputEventJoypadButton()
            {
                Device = device,
                ButtonIndex = JoyButton.A,
            });
            InputMap.ActionAddEvent("NOS", new InputEventJoypadButton()
            {
                Device = device,
                ButtonIndex = JoyButton.B,
            });
            InputMap.ActionAddEvent("shift up", new InputEventJoypadButton()
            {
                Device = device,
                ButtonIndex = JoyButton.Y,
            });
            InputMap.ActionAddEvent("shift down", new InputEventJoypadButton()
            {
                Device = device,
                ButtonIndex = JoyButton.X,
            });
            InputMap.ActionAddEvent("right", new InputEventJoypadMotion()
            {
                Device = device,
                Axis = JoyAxis.LeftX,
                AxisValue = 1.0f
            });
            InputMap.ActionAddEvent("left", new InputEventJoypadMotion()
            {
                Device = device,
                Axis = JoyAxis.LeftX,
                AxisValue = -1.0f
            });
            
            
            
        }
        else
        {
            
        }
    }
}