using Godot;
using System;

public partial class GameHandler : Node
{
	[Export(PropertyHint.File, "*.tscn")]
	public String next_level;

	[Export]
	public PC pc;

	[Export]
	public Timer start_timer;
	[Export]
	public Timer game_timer;
	[Export]
	public Timer cooldown_timer;
	[Export]
	public Node3D timer_arm;
	[Export]
	public Control end_screen;
	[Export]
	public Control countdown_screen;
	[Export]
	public Label countdown_label;
	[Export]
	public RichTextLabel final_stats;
	[Export]
	public Score game_score;

	[Export(PropertyHint.File)]
	public String main_menu;

	private bool paused = false;
	private bool game_finished = false;
	private bool game_starting = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		pc.setControllable(false);
		game_starting = true;
		end_screen.Visible = false;
		countdown_screen.Visible = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GD.Print(Engine.GetFramesPerSecond());
		if (Input.IsActionJustPressed("ui_cancel") && !game_finished && !game_starting) {
			if (paused) {
				pc.setControllable(true);
				paused = false;
				Input.MouseMode = Input.MouseModeEnum.Captured;
				GetTree().Paused = false;
			} 
			else {
				pc.setControllable(false);
				paused = true;
				Input.MouseMode = Input.MouseModeEnum.Visible;
				GetTree().Paused = true;
			}
		}

		if (!start_timer.IsStopped()) {
			countdown_label.Text = Mathf.Ceil(start_timer.TimeLeft).ToString();
		}
	}

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

		if (!game_timer.IsStopped()) {
			float timer_completion = (float) (game_timer.TimeLeft / game_timer.WaitTime);
			//GD.Print(timer_completion);
			timer_arm.RotationDegrees = new Vector3(0, Mathf.Lerp(-180, 180, (timer_completion - 1)), 0);
			//timer_arm.RotateY();
		}
    }

    public void _on_level_timer_timeout() {
		pc.setControllable(false);
		countdown_screen.Visible = true;
		countdown_label.Text = "Finished";
		cooldown_timer.Start();
	}

	public void _on_cooldown_timer_timeout()
	{
		countdown_screen.Visible = false;
		game_finished = true;
		end_screen.Visible = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;

		final_stats.Text = String.Format(final_stats.Text, 100f * (game_score.total_score / game_score.max_score));
	}

	public void _on_start_timer_timeout()
	{
		pc.setControllable(true);
		game_timer.Start();
		game_starting = false;
		countdown_screen.Visible = false;
	}

	public void _on_quit_button_pressed()
	{
		GetTree().ChangeSceneToFile(main_menu);
	}

	public void _on_next_level_button_pressed()
	{
		if (next_level == null) {
			GetTree().ChangeSceneToFile(main_menu);
		} else
		{
			GetTree().ChangeSceneToFile(next_level);
		}
	}

	public void _on_restart_button_pressed()
	{
		GetTree().ReloadCurrentScene();
	}
}
