using Godot;
using System;

public partial class WallPhase : AbilityBase
{
	[Export]
	public RayCast3D raycast;

	public float wall_exit_velocity = 5.0f;

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

			if (collider.GetParent<Wall>().double_sided) {
				Vector3 collision_point = raycast.GetCollisionPoint();
				Vector3 pc_dir = -pc.Basis.Z;
				Vector3 wall_dir = collider.GetParent<Node3D>().Basis.Z;
				
				if (pc_dir.Dot(wall_dir) < 0) {
					float distance_to_wall = (collision_point - raycast.GlobalPosition).Length();
					pc.GlobalPosition = collision_point -  2 * collider.GetParent<Wall>().wall_thickness * wall_dir; 	
				}
				else {
					float distance_to_wall = (collision_point - raycast.GlobalPosition).Length();
					pc.GlobalPosition = collision_point +  2 * collider.GetParent<Wall>().wall_thickness * wall_dir; 
				}

				pc.Velocity = -wall_exit_velocity * pc.Basis.Z;
				ability_timer.Start();
			}
			else {

			}
		}
	}
}
