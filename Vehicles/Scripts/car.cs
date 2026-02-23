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
	[Export] private float _NOSAffect = 2f;

	[Export] Node3D _cameraPivot;
	[Export] Camera3D _camera;

	[Export] private VehicleWheel3D _rightRearWheel;
	
	[Export] private VehicleWheel3D _leftRearWheel;

	private const float AirDensity = 1.225f;
	
	Vector3 _lookat = new Vector3();

	private float _cameraRotation = 10f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		ApplyCentralImpulse(Vector3.Up * 0.01f);
		Freeze = false;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		//Main Stuff
		Steering = (float) Mathf.MoveToward(Steering, Input.GetAxis("right", "left")* _maxSteer, delta );
		EngineForce = (_maxEngineForce * Input.GetAxis("back", "forward")) - (_crr * Mass * 9.81f); //Rolling Resistance
		
		//Resistances
		Vector3 drag = -LinearVelocity.Normalized() * LinearVelocity.LengthSquared() * _cd * _fArea * AirDensity/2f; 
		Vector3 downForce = -GlobalTransform.Basis.Y *  AirDensity * _cl * _fArea * LinearVelocity.LengthSquared()/2f;
		ApplyCentralForce(drag + downForce);
		
		//NOS & Handbrake
		if (Input.IsActionJustPressed("NOS"))
		{
			_maxEngineForce = _maxEngineForce * _NOSAffect; 
			//(float) Mathf.MoveToward(_maxEngineForce, _maxEngineForce * _NOSAffect, delta * 5) ;
			_camera.Fov = _camera.Fov + 10 * _NOSAffect;
			//(float) Mathf.MoveToward(_camera.Fov, _camera.Fov + 10 * _NOSAffect, delta * 5) ;
		}else if(Input.IsActionJustReleased("NOS"))
		{
			_maxEngineForce = _maxEngineForce / _NOSAffect; 
			//(float) Mathf.MoveToward(_maxEngineForce, _maxEngineForce * _NOSAffect, delta * 5) ;
			_camera.Fov = _camera.Fov - 10 * _NOSAffect;
			//(float) Mathf.MoveToward(_camera.Fov, _camera.Fov + 10 * _NOSAffect, delta * 5) ;
		}
		if (Input.IsActionPressed("hand_brake"))
		{
			_rightRearWheel.Brake = 5f;
			_leftRearWheel.Brake = 5f;
			_leftRearWheel.SetFrictionSlip(0.2f);
			_leftRearWheel.SetFrictionSlip(0.2f);
			_cameraRotation = 2f;
		}else {
			_rightRearWheel.Brake = 0f;
			_leftRearWheel.Brake = 0f;
			_leftRearWheel.SetFrictionSlip(1f);
			_leftRearWheel.SetFrictionSlip(1f);
			_cameraRotation = 10f;
		}
		
		if (Input.IsActionJustPressed("jump"))
		{
			ApplyCentralImpulse(new Vector3(0,500,0));
		}

		
		//Camera stuff
		 _cameraPivot.GlobalPosition = GlobalPosition;
		 _cameraPivot.GlobalTransform = _cameraPivot.Transform.InterpolateWith(Transform, (float)(delta * _cameraRotation));
		 _lookat = _lookat.Lerp(GlobalPosition + LinearVelocity.Normalized(), (float)(delta * 20));
		 _camera.LookAt(_lookat);
		 
		
	}
}
