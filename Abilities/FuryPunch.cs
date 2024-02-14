using Godot;
using System;

public partial class FuryPunch : AbilityBase
{
	[Export]
	public float destruction_radius = 3.5f;
	[Export]
	public float explosion_knockback = 26.5f;

	[Export]
	public RayCast3D raycast;

	[Export]
	public PackedScene explosion_scene;

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
			StaticBody3D collider = (StaticBody3D) raycast.GetCollider();

			if (collider == null)
			{
				return;
			}

			Vector3 collision_point = raycast.GetCollisionPoint();
			Vector3 collision_normal = raycast.GetCollisionNormal();

			Explosion explosion = explosion_scene.Instantiate<Explosion>();
			GetTree().CurrentScene.AddChild(explosion);
			explosion.explodeExplosion(destruction_radius);
			explosion.Position = collision_point;

			pc.Velocity = explosion_knockback * collision_normal;

			ability_timer.Start();
		}
	}
}
