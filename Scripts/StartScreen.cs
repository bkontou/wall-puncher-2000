using Godot;
using System;

public partial class StartScreen : Node3D
{
	[Export]
	public PackedScene[] levels;

	[Export]
	public Control loading_screen;
	[Export]
	public Timer start_timer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_start_button_pressed()
	{
		loading_screen.Visible = true;
		start_timer.Start();
	}

	public void _on_quit_button_pressed()
	{
		GetTree().Quit();
	}

	public void _on_start_timer_timeout()
	{
		GetTree().ChangeSceneToPacked(levels[0]);
	}
}
