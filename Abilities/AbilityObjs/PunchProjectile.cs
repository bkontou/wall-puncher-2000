using Godot;
using System;

public partial class PunchProjectile : Node3D
{
	public float projectile_speed = 5.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		
		GlobalPosition += (float) delta * projectile_speed * Basis.Z;
	}

	public void _on_kill_timer_timeout()
	{
		QueueFree();
	}

	private void _on_area_3d_area_entered(Area3D area)
	{
		QueueFree();
	}
}


