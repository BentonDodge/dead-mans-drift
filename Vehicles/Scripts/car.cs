using Godot;
using System;

public partial class car : VehicleBody3D
{
	[Export] float _maxSteer = 0.3f;
	[Export] float _maxEngineSpeed = 500;
	[Export] float _maxEngineForce = 200;
	[Export] float _cd = 0.4f;//Coefficient of Drag
	[Export] float _cl = 1.2f;//Coefficient of Lift
	[Export] float _crr= 0.02f;//Normal 0.01-0.03
	[Export] float _fArea = 2f;//frontal Area
	
	[Export] Node3D _cameraPivot;
	[Export] Camera3D _camera;

	private const float AirDensity = 1.225f;
	
	Vector3 _lookat = new Vector3();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		ApplyCentralImpulse(Vector3.Up * 0.01f);
		Freeze = false;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		Steering = (float) Mathf.MoveToward(Steering, Input.GetAxis("ui_right", "ui_left")* _maxSteer, delta * 5);
		Vector3 drag = -LinearVelocity.Normalized() * LinearVelocity.LengthSquared() * _cd * _fArea * AirDensity/2f; 
		Vector3 downForce = -GlobalTransform.Basis.Y *  AirDensity * _cl * _fArea * LinearVelocity.LengthSquared()/2f;
		ApplyCentralForce(drag);
		ApplyCentralForce(downForce);
		if (Input.IsActionJustPressed("NOS"))
		{
			_maxEngineForce = _maxEngineForce * 1.5f;
		}else if (Input.IsActionJustReleased("NOS"))
		{
			_maxEngineForce = _maxEngineForce / 1.5f;
		}

		// float baseBrake = 0f;
		// float baseFrictionSlip = 0f;
		// if (Input.IsActionPressed("hand_brake"))
		// {
		// 	foreach (Node child in GetChildren())
		// 	{
		// 		if (child is VehicleWheel3D wheel)
		// 		{
		// 			baseBrake = wheel.Brake;
		// 			baseFrictionSlip = wheel.WheelFrictionSlip;
		// 			wheel.Brake = 0.5f;
		// 			wheel.WheelFrictionSlip = 1f;
		// 		}
		// 	}
		// }else if (Input.IsActionJustReleased("hand_brake"))
		// {
		// 	foreach (Node child in GetChildren())
		// 	{
		// 		if (child is VehicleWheel3D wheel)
		// 		{
		// 			wheel.Brake = baseBrake;
		// 			wheel.WheelFrictionSlip = baseFrictionSlip;
		// 		}
		// 	}
		// }

		// if (LinearVelocity.Length() >= _maxEngineSpeed) {
		EngineForce = (_maxEngineForce * Input.GetAxis("ui_down", "ui_up")); //- (_crr * Mass * 9.81f); //Rolling Resistance
		GD.Print(LinearVelocity.Length());
		// }
		
		
		//Camera stuff from random tutorial
		// _cameraPivot.GlobalPosition = GlobalPosition;
		// _cameraPivot.GlobalTransform = _cameraPivot.Transform.InterpolateWith(Transform, (float)(delta * 20));
		// // _camera.GlobalPosition = _cameraPivot.GlobalPosition;
		// _lookat = _lookat.Lerp(GlobalPosition + LinearVelocity, (float)(delta * 20));
		// _camera.LookAt(_lookat);
	}
}
