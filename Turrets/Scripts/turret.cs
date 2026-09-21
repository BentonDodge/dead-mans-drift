using Godot;
using System;

public partial class turret : Node3D
{
	[Export] public float MouseSensitivity = 0.002f;
	[Export] private Camera3D _camera;
	[Export] private CpuParticles3D _particles;
	[Export] private float _cameraFov = 40f;
	private float _cameraAngle = 0f;
	
	
	public override void _Ready() {
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_camera = GetNode<Camera3D>("Camera3D");
		_particles.Emitting = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsMouseButtonPressed(MouseButton.Right))
		{
			_camera.Fov = 40f;

		}else {
			_camera.Fov = 70f;
		}
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motion)
		{
			// Horizontal
			RotateY(-motion.Relative.X * MouseSensitivity);

			// Vertical
			_cameraAngle -= motion.Relative.Y * MouseSensitivity;

			_cameraAngle = Mathf.Clamp(
				_cameraAngle,
				-Mathf.Pi / 2,
				Mathf.Pi / 2
			);

			_camera.Rotation = new Vector3(
				_cameraAngle,
				_camera.Rotation.Y,
				_camera.Rotation.Z
			);
		}else if (@event is InputEventMouseButton mouseButton) {
			if (mouseButton.ButtonIndex == MouseButton.Left)
			{
				var spaceState = GetWorld3D().DirectSpaceState;
				var query = PhysicsRayQueryParameters3D.Create(this.GlobalPosition,
					-_camera.GlobalTransform.Basis.Z * 100f);
				var result = spaceState.IntersectRay(query);
				if (result.Count > 0)
				{
					GD.Print("Hit at point: ", result["position"]);
					_particles.GlobalPosition = result["position"].AsVector3();
					_particles.Emitting = true;
					// if (result["collider"].) {
					// 	var collider = (RigidBody3D)result["collider"];
					// 	collider.ApplyImpulse(-_camera.GlobalTransform.Basis.Z * 100f,(Vector3) result["position"]);
					// 	GD.Print(collider.GlobalPosition);
					// }
				}
			}
		}
	}
}
