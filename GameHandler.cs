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

	[Export]
	public ProgressBar abiltiy_e_cooldown;
	[Export]
	public ProgressBar ability_shift_cooldown;
	[Export]
	public TextureRect ability_e_texture;
	[Export]
	public TextureRect ability_shift_texture;

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

		initializePCAbilities();
		initializeAbilityImages();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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

		updateAbilityTimers();
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

	private void updateAbilityTimers()
	{
		if (pc.ability_e != null) {
			abiltiy_e_cooldown.Value = 100 * (1f - ((AbilityBase) pc.ability_e).ability_timer.TimeLeft / ((AbilityBase) pc.ability_e).ability_timer.WaitTime);
		}

		if (pc.ability_shift != null) {
			ability_shift_cooldown.Value = 100 * (1f - ((AbilityBase) pc.ability_shift).ability_timer.TimeLeft / ((AbilityBase) pc.ability_shift).ability_timer.WaitTime);
		}

	}

	private void initializeAbilityImages()
	{
		if (pc.ability_e != null) {
			ability_e_texture.Texture = pc.ability_e.ability_image;
		}

		if (pc.ability_shift != null) {
			ability_shift_texture.Texture = pc.ability_shift.ability_image;
		}
	}

	private void initializePCAbilities()
	{
		if (GetNode<GlobalInfo>("/root/GlobalInfo").selected_shift_item != null) {
			AbilityBase ability = GD.Load<PackedScene>(GetNode<GlobalInfo>("/root/GlobalInfo").selected_shift_item.item_scene).Instantiate<AbilityBase>();
			pc.GetNode("Camera3D").AddChild(ability);
			ability.pc = pc;
			pc.ability_shift = ability;
		}

		if (GetNode<GlobalInfo>("/root/GlobalInfo").selected_e_item != null) {
			AbilityBase ability = GD.Load<PackedScene>(GetNode<GlobalInfo>("/root/GlobalInfo").selected_e_item.item_scene).Instantiate<AbilityBase>();
			pc.GetNode("Camera3D").AddChild(ability);
			ability.pc = pc;
			pc.ability_e = ability;
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
		pc.Velocity = Vector3.Zero;
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
