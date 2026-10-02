using System;
using Godot;

public partial class Car : VehicleBody3D
{
	private const float AirDensity = 1.225f;
	[Export] private Camera3D _camera;

	[Export] private Node3D _cameraPivot;

	private float _cameraRotation = 10f;
	[Export] private float _cd = 0.1f; //Coefficient of Drag
	[Export] private float _cl = 0.2f; //Coefficient of Lift
	[Export] private float _crr = 0.02f; //Coefficient of Rolling Resistance (Normal 0.01-0.03)
	private int _currentGear = 1;

	private bool _doubleJump = true;
	[Export] private float _fArea = 2f; //frontal Area
	private float _finalDrive = 3.38f;
	[Export] private float[] _gears = [-3.20f, 3.36f, 2.7f, 2.3f, 1.5f];
	[Export] private float _idleRevs = 800;

	[Export] private VehicleWheel3D _leftRearWheel;
	[Export] private VehicleWheel3D _rightRearWheel;
	
	private Vector3 _lookAt;
	[Export] private float _maxEngineForce = 200;
	[Export] private float _maxEngineRevs = 8000;
	[Export] private float _maxSteer = 0.3f;
	[Export] private float _nosAffect = 2f;
	private float _nosCameraFov;

	private float _nosEngineForce;

	[Export] private Curve _powerCurve;
	private float _revs;

	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_nosEngineForce = _nosAffect * _maxEngineForce;
		_nosCameraFov = _camera.Fov + 10 * _nosAffect;
		Freeze = false;
		foreach (int device in Input.GetConnectedJoypads())
		{
			GD.Print($"Controller detected: {device}");
			GD.Print($"Name: {Input.GetJoyName(device)}");
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		//Main Stuff
		Steering = (float)Mathf.MoveToward(Steering, Input.GetAxis("right", "left") * _maxSteer, delta);

		//Resistances
		var drag = -LinearVelocity.Normalized() * LinearVelocity.LengthSquared() * _cd * _fArea * AirDensity / 2f;
		var downForce = -GlobalTransform.Basis.Y * AirDensity * _cl * _fArea * LinearVelocity.LengthSquared() / 2f;
		ApplyCentralForce(drag + downForce);

		//NOS
		if (Input.IsActionPressed("NOS"))
		{
			_maxEngineForce = (float)Mathf.MoveToward(_maxEngineForce, _nosEngineForce, delta * 20);
			_camera.Fov = (float)Mathf.MoveToward(_camera.Fov, _nosCameraFov, delta * 20);
		}
		else
		{
			_maxEngineForce = (float)Mathf.MoveToward(_maxEngineForce, _nosEngineForce / _nosAffect, delta * 15);
			_camera.Fov = (float)Mathf.MoveToward(_camera.Fov, _nosCameraFov - 10 * _nosAffect, delta * 15);
		}

		//Hand Brake
		if (Input.IsActionPressed("hand_brake"))
		{
			_rightRearWheel.Brake = 5f;
			_leftRearWheel.Brake = 5f;
			_leftRearWheel.SetFrictionSlip(0.2f);
			_leftRearWheel.SetFrictionSlip(0.2f);
			_cameraRotation = 4f;
		}
		else
		{
			_rightRearWheel.Brake = 0f;
			_leftRearWheel.Brake = 0f;
			_leftRearWheel.SetFrictionSlip(1f);
			_leftRearWheel.SetFrictionSlip(1f);
			_cameraRotation = 7f;
		}

		//Jumping
		if (Input.IsActionJustPressed("jump") && WheelsInContact())
		{
			_doubleJump = true;
			ApplyCentralImpulse(new Vector3(0, 500, 0) * GlobalTransform.Inverse() - downForce);
		}
		else if (_doubleJump && Input.IsActionJustPressed("jump"))
		{
			_doubleJump = false;
			ApplyCentralImpulse(new Vector3(0,250,0) * GlobalTransform.Inverse() - downForce);
			// backflip();
		}


		//Camera stuff
		_cameraPivot.GlobalPosition = GlobalPosition;
		_cameraPivot.GlobalTransform =
			_cameraPivot.Transform.InterpolateWith(Transform, (float)(delta * _cameraRotation));
		_lookAt = _lookAt.Lerp(GlobalPosition + LinearVelocity.Normalized(), (float)(delta * 10));
		_camera.LookAt(_lookAt);

		//Actual Engine Stuff
		EngineForce = CalculateEngineForce(delta); //-(_crr * Mass * 9.81f); Rolling Resistance
	}

	private bool WheelsInContact()
	{
		foreach (var child in GetChildren())
		{
			if (child is not VehicleWheel3D wheel) continue;
			if (!wheel.IsInContact()) return false;
		}

		return true;
	}

	private float WheelsRpm()
	{
		float avgRpm = 0;
		float numberOfWheels = 0;
		foreach (var child in GetChildren())
			if (child is VehicleWheel3D wheel)
			{
				avgRpm += wheel.GetRpm();
				numberOfWheels++;
			}

		return avgRpm / numberOfWheels;
	}

	private float? GetWheelsCircumference()
	{
		foreach (var child in GetChildren())
			if (child is VehicleWheel3D wheel)
				return (float?)(wheel.GetRadius() * 2f * Math.PI);

		return null;
	}

	private float CalculateEngineForce(double delta)
	{
		if (Input.IsActionJustPressed("shift up") && _currentGear < _gears.Length - 1)
		{
			_currentGear++;
			_revs *= _gears[_currentGear] / _gears[_currentGear - 1];
		}
		else if (Input.IsActionJustPressed("shift down") && _currentGear > 0)
		{
			_currentGear--;
			if (_gears[_currentGear] / _gears[_currentGear + 1] * _revs < _maxEngineRevs)
				_revs *= _gears[_currentGear] / _gears[_currentGear + 1];
			//TODO remove health?
		}

		var wheelDrivenRevs = WheelsRpm() * _gears[_currentGear] * _finalDrive;
		var targetRevs = Mathf.Max(_idleRevs, wheelDrivenRevs);
		var revsAcceleration = Mathf.Abs(Input.GetAxis("back", "forward")) > 0.1f ? 15f : 5f;
		_revs = (float)Mathf.MoveToward(_revs, targetRevs, delta * _maxEngineRevs * revsAcceleration);

		_revs = Mathf.Clamp(_revs, _idleRevs, _maxEngineRevs);
		// GD.Print(_powerCurve.SampleBaked(Mathf.Clamp(_revs / _maxEngineRevs, 0f, 1.0f)));
		return _powerCurve.SampleBaked(Mathf.Clamp(_revs / _maxEngineRevs, 0f, 1.0f)) *
			   Input.GetAxis("back", "forward")*
			   _finalDrive *
			   _gears[_currentGear] *
			   _maxEngineForce;
	}
}
