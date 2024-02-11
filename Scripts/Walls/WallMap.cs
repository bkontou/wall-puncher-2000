using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public struct WallSegmentData
{
	public WallSegmentData(Vector3 ps, Vector3 pe, bool cs, bool ce, bool ds)
	{
		position_start = ps;
		position_end = pe;
		cap_start = cs;
		cap_end = ce;
		double_sided = ds;
	}

	public Vector3 position_start = Vector3.Zero;
	public Vector3 position_end = Vector3.Zero;
	public bool cap_start = false;
	public bool cap_end = false;
	public bool double_sided = false;
}

[Tool]
public partial class WallMap : Node3D
{	
	[Export]
	public PackedScene wall_scene;

	[Export]
	public Json walls_data;

	[Export]
	public StandardMaterial3D wall_material;

	[Export]
	public Color wall_color;

	//[Export]
	public List<WallSegmentData> wall_segments = new List<WallSegmentData>();

	public float wall_thickness = 0.57f;

	private bool _added_mesh_reps = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Engine.IsEditorHint()) {
			loadData();
			foreach (WallSegmentData wall_segment in wall_segments) 
			{
				// Code to execute in editor.
				MeshInstance3D wall_1 = new MeshInstance3D();
				MeshInstance3D wall_2 = new MeshInstance3D();
				
				float wall_width = (wall_segment.position_start - wall_segment.position_end).Length() - 0.5f * wall_thickness;
				Vector3 wall_center = (wall_segment.position_end + wall_segment.position_start) / 2f;
				wall_center.Y = 0.5f * 5f;
				Vector3 wall_dir_final = (wall_segment.position_end - wall_segment.position_start).Normalized();
				Vector3 wall_normal_dir = Vector3.Right.Rotated(Vector3.Up, 0.5f * Vector3.Right.SignedAngleTo(wall_dir_final, Vector3.Up));

				wall_1.Mesh = new PlaneMesh();
				wall_1.Mesh.SurfaceSetMaterial(0, wall_material);
				wall_1.Mesh.Set("size", new Vector2(wall_width, 5f));
				
				// Rotate and translate
				wall_1.RotateX(Mathf.DegToRad(90));
				wall_1.RotateY(Vector3.Right.SignedAngleTo(wall_dir_final, Vector3.Up));
				wall_1.Position = wall_center  +0.5f * wall_thickness * wall_normal_dir;


				wall_2.Mesh = new PlaneMesh();
				wall_2.Mesh.SurfaceSetMaterial(0, wall_material);
				wall_2.Mesh.Set("size", new Vector2(wall_width, 5f));
				
				// Rotate and translate
				wall_2.RotateX(Mathf.DegToRad(90));
				wall_2.RotateY(Vector3.Right.SignedAngleTo(wall_dir_final, Vector3.Up));
				wall_2.Position = wall_center - 0.5f * wall_thickness * wall_normal_dir;

				AddChild(wall_1);
				AddChild(wall_2);
			}
		}
		else {
			loadData();
			buildAllWalls();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// if (Engine.IsEditorHint() && !_added_mesh_reps)
		// {
		// 	// Code to execute in editor.
		// 	MeshInstance3D test = new MeshInstance3D();
		// 	test.Mesh = new SphereMesh();
		// 	AddChild(test);
		// 	_added_mesh_reps = true;
		// 	GD.Print("hey lol");
		// }
	}

	private void loadData()
	{
		foreach(Godot.Collections.Dictionary val in walls_data.Data.AsGodotDictionary()["walls"].AsGodotArray())
		{
			WallSegmentData wall = new WallSegmentData();
			wall.position_start = arrayToVec3(val["position_start"].AsGodotArray<float>());
			wall.position_end = arrayToVec3(val["position_end"].AsGodotArray<float>());
			wall.cap_start = val["cap_start"].AsBool();
			wall.cap_end = val["cap_end"].AsBool();
			wall.double_sided = val["double_sided"].AsBool();
			wall_segments.Add(wall);
		}
	}

	private Vector3 arrayToVec3(Godot.Collections.Array<float> a)
	{
		return new Vector3(a[0], a[1], a[2]);
	}

	private void buildAllWalls()
	{
		foreach (WallSegmentData segment in wall_segments) {
			//GD.Print(segment.position_start, " ", segment.position_end);
			buildWall(segment);
		}

		foreach (var val in walls_data.Data.AsGodotDictionary()["corners"].AsGodotArray())
		{
			GD.Print(val);
			MeshInstance3D corner_mesh = addCornerMesh();
			AddChild(corner_mesh);
			corner_mesh.GlobalPosition = arrayToVec3(val.AsGodotArray<float>()) + 0.5f * 4 * Vector3.Up;
		}
	}

	private void buildWall(WallSegmentData segment)
	{
		float wall_width = (segment.position_start - segment.position_end).Length() - 0.5f * wall_thickness;

		Wall wall = wall_scene.Instantiate<Wall>();
		AddChild(wall);

		wall.wall_width = wall_width;
		wall.wall_height = 4;
		wall.wall_thickness = 0.5f;
		wall.double_sided = segment.double_sided;
		wall.cap_start_wall = segment.cap_start;
		wall.cap_end_wall = segment.cap_end;
		wall.wall_color = wall_color;

		// Rotate and translate
		Vector3 wall_center = (segment.position_end + segment.position_start) / 2f;
		wall_center.Y = 0.5f * wall.wall_height;
		Vector3 wall_dir_final = (segment.position_end - segment.position_start).Normalized();

		wall.RotateY(Vector3.Right.SignedAngleTo(wall_dir_final, Vector3.Up));

		wall.Position = wall_center;
		//wall.LookAtFromPosition(wall_center, wall_dir_final, Vector3.Back);

		wall.build();

		// MeshInstance3D corner_mesh = addCornerMesh();
		// wall.AddChild(corner_mesh);
		// corner_mesh.GlobalPosition = segment.position_end + 0.5f * 4 * Vector3.Up;
	}

	MeshInstance3D addCornerMesh()
	{
		MeshInstance3D corner_mesh = new MeshInstance3D();
		corner_mesh.Mesh = new BoxMesh();
		corner_mesh.Mesh.SurfaceSetMaterial(0, wall_material);
		corner_mesh.Mesh.Set("size", new Vector3(wall_thickness, 4, wall_thickness));
		//corner_mesh.RotationDegrees = new Vector3(-90, 0, 0);

		return corner_mesh;
	}
}
