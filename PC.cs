using Godot;
using System;
using System.Numerics;

public partial class PC : CharacterBody3D
{
	[Export]
	public Node3D pc_fist;
	[Export]
	public Area3D pc_fist_area;
	[Export]
	public Timer pc_fist_timer;
	[Export]
	public MeshInstance3D pc_fist_model;
	[Export]
	public AnimationPlayer pc_fist_animation;

	[Export]
	public AudioStreamPlayer3D pc_sfx;

	[Export]
	public Camera3D pc_camera;

	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public const float mouse_sensitiviy = 1.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	private bool controllable = true;

	public override void _PhysicsProcess(double delta)
	{
		Godot.Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		Godot.Vector3 direction = Godot.Vector3.Zero;

		if (controllable) {
			// Handle Jump.
			if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
				velocity.Y = JumpVelocity;

			if (Input.IsActionJustPressed("ui_lmb") && pc_fist_timer.IsStopped()) {
				pc_fist_area.ProcessMode = ProcessModeEnum.Always;
				pc_sfx.Play();
				pc_fist_timer.Start();
				pc_fist_animation.Play("fist_punch");
			}

			// Get the input direction and handle the movement/deceleration.
			// As good practice, you should replace UI actions with custom gameplay actions.
			Godot.Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			direction = (Transform.Basis * new Godot.Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
			
		}
	
		if (direction != Godot.Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}
			
		Velocity = velocity;
		MoveAndSlide();
	}

	public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)// && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            InputEventMouseMotion mouseEvent = @event as InputEventMouseMotion;
            pc_camera.RotateX(Mathf.DegToRad(-mouseEvent.Relative.Y * mouse_sensitiviy));
            RotateY(Mathf.DegToRad(-mouseEvent.Relative.X * mouse_sensitiviy));

			Godot.Vector3 rot = pc_camera.RotationDegrees;
			rot.X = Mathf.Clamp(rot.X, -70, 70);
			pc_camera.RotationDegrees = rot;
        }
    }

	public void _on_fist_timer_timeout()
	{
		pc_fist_area.ProcessMode = ProcessModeEnum.Disabled;
	}

	public void setControllable(bool b)
	{
		controllable = b;
	}
}
