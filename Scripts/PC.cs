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

	[Export]
	public AbilityBase ability_e;
	[Export]
	public AbilityBase ability_shift;

	public const float Speed = 5.0f;
	public const float JumpVelocity = 200.5f;
	public const float mouse_sensitiviy = 1.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	private bool controllable = true;

	private Godot.Vector3 force;

	private float FLOOR_FRICTION = 0.5f;


	public override void _PhysicsProcess(double delta)
	{
		force += gravity * Godot.Vector3.Down;


		

		if (controllable)
		{
			Godot.Vector3 direction;
			if (IsOnFloor()) {				
				// Get the input direction and handle the movement/deceleration.
				// As good practice, you should replace UI actions with custom gameplay actions.
				Godot.Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
				direction = (Transform.Basis * new Godot.Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
			} else {
				direction = Godot.Vector3.Zero;
			}

			if (IsOnFloor()) {
				if (direction != Godot.Vector3.Zero) {
					Godot.Vector3 max_vel = Speed * direction;
					Velocity = Velocity.MoveToward(max_vel, FLOOR_FRICTION);
				} else {
					Velocity = Velocity.MoveToward(Godot.Vector3.Zero, FLOOR_FRICTION);
				}
			}

			if (Input.IsActionJustPressed("ui_accept"))
			{
				if (!IsOnFloor()) {
					Velocity = Godot.Vector3.Zero;
					force += JumpVelocity * (Godot.Vector3.Up + direction);
				}
				else {
					force += JumpVelocity * Godot.Vector3.Up;
				}
			}

			if (Input.IsActionJustPressed("ui_lmb") && pc_fist_timer.IsStopped()) {
				pc_fist_area.ProcessMode = ProcessModeEnum.Always;
				pc_sfx.Play();
				pc_fist_timer.Start();
				pc_fist_animation.Play("fist_punch");
			}
				

			if (ability_e != null)
				ability_e.pollAction("ui_action_e");
			
			if (ability_shift != null)
				ability_shift.pollAction("ui_action_shift");
		} else 
		{
			Velocity = Velocity.MoveToward(Godot.Vector3.Zero, FLOOR_FRICTION);
		}
		
		Velocity += ((float) delta) * force;
		
		MoveAndSlide();
		force = Godot.Vector3.Zero;
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
