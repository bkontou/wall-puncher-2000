using Godot;
using System;

public partial class Explosion : Node3D
{
	[Export]
	public Area3D explosion_area;
	[Export]
	public MeshInstance3D explosion_mesh;
	[Export]
	AudioStreamPlayer3D explosion_audio;

	private Tween explosion_tween;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		explosion_tween = GetTree().CreateTween();
		explodeExplosion(2);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void explodeExplosion(float radius)
	{
		SphereShape3D explosion_shape = (SphereShape3D) explosion_area.GetNode<CollisionShape3D>("CollisionShape3D").Shape;
		explosion_shape.Radius = radius;
		explosion_tween.SetEase(Tween.EaseType.Out);
		explosion_tween.SetTrans(Tween.TransitionType.Quint);
		explosion_tween.TweenProperty(explosion_mesh, "scale", 2 * radius * Vector3.One, 0.5f);
		explosion_tween.TweenProperty(explosion_mesh, "transparency", 1f, 1.2f);
		explosion_tween.Connect(Tween.SignalName.Finished, new Callable(this, MethodName.QueueFree));
		explosion_audio.Play();
	}
}

