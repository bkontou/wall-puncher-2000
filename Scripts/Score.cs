using Godot;
using System;
using System.Numerics;

public partial class Score : Node
{
	[Export]
	public Label score_label;
	[Export]
	public Label timer_label;
	[Export]
	public Timer game_timer;

	[Export]
	public Node3D game_score_arm;

	[Export]
	public Node wall_nodes;

	[Export]
	public float score_leeway = 0.1f;
	private float _total_score;
	public float total_score
	{
		get { return _total_score; }
		set 
		{
			_total_score = value;
			score_label.Text = "SCORE: " + _total_score.ToString().PadRight(2) + " / " + _max_score.ToString();
			game_score_arm.RotationDegrees = new Godot.Vector3(0f, Mathf.Lerp(171, -171, total_score / max_score), 0f);
		}
	}

	private float _max_score;
	public float max_score { 
		get { return _max_score; } 
		set { _max_score = value; } 
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		max_score = 0;
		foreach (Wall wall in wall_nodes.GetChildren()) {
			max_score += wall.getWallArea();
		}
		max_score = max_score * (1 - score_leeway);

		GD.Print(max_score);
		total_score = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!game_timer.IsStopped()) {
			timer_label.Text = game_timer.TimeLeft.ToString();
		}
	}

	public void _on_wall_fragment_destroyed(float fragment_area)
	{
		total_score += fragment_area;
	}
}