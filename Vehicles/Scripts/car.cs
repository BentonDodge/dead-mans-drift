using Godot;
using System;

public partial class car : VehicleBody3D
{
	const double MAXSTEER = 0.8;
	
	@onready
	Node3D cameraPivot = $CameraPivot
	@onready
	Camera3D camera = $CameraPivot/Camera3D
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		steering = moveTowards(steering, Input.get_axis() * MAXSTEER ,delta);
		
	}
}
