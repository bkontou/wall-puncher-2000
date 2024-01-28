using Godot;
using System;

public partial class StartScreen : Node3D
{
	[Export]
	public PackedScene[] levels;

	[Export]
	public Control loading_screen;
	[Export]
	public Control level_select;
	[Export]
	public VBoxContainer levels_list;
	[Export]
	public Control main_menu_buttons;
	[Export]
	public Timer start_timer;


	private int _selected_level = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for (int i = 0; i < levels.Length; i++) 
		{
			Button new_button = new Button();
			new_button.Text = "Level " + i.ToString();
			int level_id = new int();
			level_id = i;
			var load_level_lambda = () => { _on_level_button_pressed(level_id); };
			new_button.Connect(Button.SignalName.Pressed, Callable.From(load_level_lambda));
			levels_list.AddChild(new_button);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_start_button_pressed()
	{
		level_select.Visible = true;
		main_menu_buttons.Visible = false;
	}

	public void _on_quit_button_pressed()
	{
		GetTree().Quit();
	}

	public void _on_start_timer_timeout()
	{
		GD.Print(_selected_level);
		GetTree().ChangeSceneToPacked(levels[_selected_level]);
	}
	
	public void _on_back_button_pressed()
	{
		level_select.Visible = false;
		main_menu_buttons.Visible = true;
	}

	public void _on_level_button_pressed(int level)
	{
		GD.Print("LEVEL NO: ", level);
		loading_screen.Visible = true;
		start_timer.Start();
		_selected_level = level;
	}
}
