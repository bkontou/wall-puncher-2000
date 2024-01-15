using Godot;
using System;

public partial class Score : Node
{
	[Export]
	public Label score_label;

	private float _total_score;
	public float total_score
	{
		get { return _total_score; }
		set 
		{
			_total_score = value;
			score_label.Text = "SCORE: " + _total_score.ToString();
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		total_score = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_wall_fragment_destroyed(float fragment_area)
	{
		total_score += fragment_area;
	}
}
