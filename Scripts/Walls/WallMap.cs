using Godot;
using System;
using System.Collections.Generic;

public struct WallSegmentData
{
	public Vector3 position_start;
	public Vector3 position_end;
	public bool cap_start;
	public bool cap_end;
	public bool double_sided;
}

public partial class WallMap : Node3D
{
	//[Export]
	public List<WallSegmentData> wall_segments = new List<WallSegmentData>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
