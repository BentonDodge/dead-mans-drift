using System;
using Godot;
public partial class car : VehicleBody3D
{
	[Export] float _maxSteer = 0.3f;
	[Export] float _maxEngineRevs = 8000;
	[Export] float _maxEngineForce = 200;
	[Export] float _cd = 0.1f;//Coefficient of Drag
	[Export] float _cl = 0.2f;//Coefficient of Lift
	[Export] float _crr= 0.02f;//Coefficient of Rolling Resistance (Normal 0.01-0.03)
	[Export] float _fArea = 2f;//frontal Area
	[Export] private float _nosAffect = 2f;

	[Export] Curve _powerCurve;
	[Export] private float[] _gears = [] ;
	private int _currentGear = 1;
	private float _revs;
	private float _finalDrive = 3.38f;
	
	private float _nosEngineForce;
	private float _nosCameraFov;

	[Export] Node3D _cameraPivot;
	[Export] Camera3D _camera;

	[Export] private VehicleWheel3D _rightRearWheel;
	
	[Export] private VehicleWheel3D _leftRearWheel;

	private const float AirDensity = 1.225f;

	private bool _doubleJump = true;
	Vector3 _lookAt;

	private float _cameraRotation = 10f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// _powerCurve.PointCount = (int)(_maxEngineRevs / 10f);
		// _powerCurve.MaxDomain = _maxEngineRevs;
		// _powerCurve.MaxValue = _maxEngineRevs;
		_nosEngineForce = _nosAffect * _maxEngineForce;
		_nosCameraFov = _camera.Fov + 10 * _nosAffect;
		// ApplyCentralImpulse(Vector3.Up * 0.01f);
		Freeze = false;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		//Main Stuff
		Steering = (float) Mathf.MoveToward(Steering, Input.GetAxis("right", "left")* _maxSteer, delta );
		
		//Resistances
		Vector3 drag = -LinearVelocity.Normalized() * LinearVelocity.LengthSquared() * _cd * _fArea * AirDensity/2f; 
		Vector3 downForce = -GlobalTransform.Basis.Y *  AirDensity * _cl * _fArea * LinearVelocity.LengthSquared()/2f;
		ApplyCentralForce(drag +downForce);
		
		//NOS
		if (Input.IsActionPressed("NOS"))
		{
			_maxEngineForce = (float) Mathf.MoveToward(_maxEngineForce, _nosEngineForce, delta * 20) ;
			_camera.Fov = (float) Mathf.MoveToward(_camera.Fov, _nosCameraFov, delta * 20) ;
		}else { 
		_maxEngineForce = (float) Mathf.MoveToward(_maxEngineForce, _nosEngineForce / _nosAffect, delta * 15) ; 
		_camera.Fov = (float) Mathf.MoveToward(_camera.Fov, _nosCameraFov - 10 * _nosAffect, delta * 15) ;
		}
		//Hand Brake
		if (Input.IsActionPressed("hand_brake"))
		{
			_rightRearWheel.Brake = 5f;
			_leftRearWheel.Brake = 5f;
			_leftRearWheel.SetFrictionSlip(0.2f);
			_leftRearWheel.SetFrictionSlip(0.2f);
			_cameraRotation = 4f;
		}else {
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
			ApplyCentralImpulse(new Vector3(0,500,0) * GlobalTransform.Inverse() - downForce);
		}else if (_doubleJump && Input.IsActionJustPressed("jump"))
		{
			_doubleJump = false;
			// ApplyCentralImpulse(new Vector3(0,250,0) * GlobalTransform.Inverse() - downForce);
			// backflip();
		}


		//Camera stuff
		 _cameraPivot.GlobalPosition = GlobalPosition;
		 _cameraPivot.GlobalTransform = _cameraPivot.Transform.InterpolateWith(Transform, (float)(delta * _cameraRotation));
		 _lookAt = _lookAt.Lerp(GlobalPosition + LinearVelocity.Normalized(), (float)(delta * 10));
		 _camera.LookAt(_lookAt);
		 
		 //Actual Engine Stuff
		 EngineForce = CalculateEngineForce(delta);//-(_crr * Mass * 9.81f); Rolling Resistance
	}

	private bool WheelsInContact() {
		foreach (Node child in GetChildren())
		{
			if (child is not VehicleWheel3D wheel) continue;
			if (!wheel.IsInContact())
			{
				return false;
			}
		}
		return true;
	}

	float WheelsRPM() {
		float avgRPM = 0;
		float numberOfWheels = 0;
		foreach (Node child in GetChildren()) {
			if (child is VehicleWheel3D wheel)
			{
				avgRPM += wheel.GetRpm();
				numberOfWheels++;
			}
		}
		return avgRPM/numberOfWheels;
	}
	
	float? GetWheelsCircumference()
	{
		foreach (Node child in GetChildren()) {
			if (child is VehicleWheel3D wheel)
			{
				return (float?)(wheel.GetRadius() * 2f * Math.PI);	
			}
		}
		return null;
	}

	float CalculateEngineForce(double delta)
	{
		if(Input.IsActionJustPressed("shift up") && _currentGear < _gears.Length - 1)
		{
		 _currentGear++;
		 _revs *= (_gears[_currentGear]/_gears[_currentGear - 1]);
		}else if (Input.IsActionJustPressed("shift down") && _currentGear > 0)
		   {
		    _currentGear--;
		    if (((_gears[_currentGear] / _gears[_currentGear + 1]) * _revs) < _maxEngineRevs) {
		     _revs *= (_gears[_currentGear]/_gears[_currentGear + 1]);   
		    }
		    else {
		     //TODO remove health?
		    }
		   }
		
		_revs = WheelsRPM() * _gears[_currentGear] * _finalDrive;
        GD.Print(_currentGear + " revs " + _revs + "power curve" + _powerCurve.SampleBaked(_revs/_maxEngineRevs) + " Velocity " + LinearVelocity.Length());
        
        return (
	        _powerCurve.SampleBaked(Mathf.Clamp(_revs/_maxEngineRevs,0f,1.0f)) *
	        Input.GetAxis("back", "forward") * 
	        _finalDrive * 
	        _gears[_currentGear] * 
	        _maxEngineForce
			);
			
	}
}