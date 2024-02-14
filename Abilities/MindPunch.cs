using Godot;
using System;

public partial class MindPunch : AbilityBase
{
	[Export]
	public PackedScene punch_projectile_scene;

	[Export]
	public float projectile_speed;
	[Export]
	public AudioStreamPlayer sfx_stream;

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
			PunchProjectile punch_projectile = punch_projectile_scene.Instantiate<PunchProjectile>();
			punch_projectile.GlobalPosition = GlobalPosition - 1.0f * GlobalBasis.Z;
			punch_projectile.projectile_speed = projectile_speed;
			
			punch_projectile.Basis = GlobalBasis;
			punch_projectile.Rotate(punch_projectile.Basis.Y, Mathf.DegToRad(180));
			GetTree().CurrentScene.AddChild(punch_projectile);
			punch_projectile.Scale = 0.5f * Vector3.One;
			ability_timer.Start();

			var rng = new RandomNumberGenerator();
			sfx_stream.PitchScale = rng.RandfRange(0.9f, 1.1f);
			sfx_stream.Play();
		}
	}
}
