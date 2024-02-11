using Godot;
using System;

public partial class Leap : AbilityBase
{
	public float leap_forward_velocity = 15.0f;
	public float leap_upward_velocity = 2.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void pollAction(String input_name)
	{
		if (Input.IsActionJustPressed(input_name) && ability_timer.IsStopped())
		{
			pc.Velocity = leap_upward_velocity * Vector3.Up + -leap_forward_velocity * pc.Basis.Z;
			ability_timer.Start();
		}
	}
}
