using Godot;
using System;

public partial class KickAbility : AbilityBase
{
	[Export]
	public Area3D kick_area;
	[Export]
	public Timer area_timer;



	public static String ability_description = "A low kick";

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
			GD.Print("Action E pressed!");
			kick_area.ProcessMode = ProcessModeEnum.Always;
			area_timer.Start();
			ability_timer.Start();
		}
	}

	public void _on_area_timer_timeout()
	{
		kick_area.ProcessMode = ProcessModeEnum.Disabled;
	}
}
