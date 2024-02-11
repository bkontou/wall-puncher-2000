using Godot;
using System;

public partial class AbilityBase : Node3D
{
	[Export]
	public Texture2D ability_image = null;
	[Export]
	public Timer ability_timer;

	[Export]
	public CharacterBody3D pc;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public virtual void pollAction(String input_name)
	{

	}
}
