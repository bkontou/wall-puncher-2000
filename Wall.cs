using Godot;
using System;
using System.Collections.Generic;


[Tool]
public partial class Wall : Node3D
{
	[Export]
	public MeshInstance3D wall_background;
	[Export]
	public CollisionShape3D wall_collider;
	
	[Export]
	public float wall_width = 5f;
	[Export]
	public float wall_height = 5f;

	public float stud_spacing_distance = 0.95f;
	public float stud_width = 0.1f;

	[Export]
	public PackedScene wall_fragment_scene;
	[Export]
	public PackedScene wall_stud_scene;

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		initializeWall();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			// change wall background 
			//wall_background.Mesh.Set("size", new Vector2(wall_width, wall_height));
		}
	}

	private void initializeWall()
	{
		Vector3 wall_pos_min = new Vector3(-0.5f * wall_width, -0.5f * wall_height, 0);
		Vector3 wall_pos_max = new Vector3(0.5f * wall_width, 0.5f * wall_height, 0);
		generateTriangles(wall_pos_min, wall_pos_max, 25);
		
		// change wall collsion
		wall_collider.Shape.Set("size", new Vector3(wall_width, wall_height, 1));

		// change wall background 
		wall_background.Mesh.Set("size", new Vector2(wall_width, wall_height));
		//wall_background.Position = new Vector3(0, 0, 0.5f);
		GD.Print(wall_background.Position);

		placeStuds();
	}
	
	private void generateTriangles(Vector3 min, Vector3 max, int n)
	{
		const float rand_x = 0.5f;
		const float rand_y = 0.5f;
		var rng = new RandomNumberGenerator();
		
		List<TriangleNet.Geometry.Vertex> vertex_list = new List<TriangleNet.Geometry.Vertex>();

		// Create vertices in four corners
		var corner_vertex_1 = new TriangleNet.Geometry.Vertex(min.X, min.Y);
		var corner_vertex_2 = new TriangleNet.Geometry.Vertex(max.X, max.Y);
		var corner_vertex_3 = new TriangleNet.Geometry.Vertex(min.X, max.Y);
		var corner_vertex_4 = new TriangleNet.Geometry.Vertex(max.X, min.Y);

		vertex_list.Add(corner_vertex_1);
		vertex_list.Add(corner_vertex_2);
		vertex_list.Add(corner_vertex_3);
		vertex_list.Add(corner_vertex_4);


		float x, y;
		for (int i = 0; i < n; i++) {
			x = min.X + i * (max.X - min.X) / (n - 1);
			for (int j = 0; j < n; j++) {
				y = min.Y + j * (max.Y - min.Y) / (n - 1) + rng.RandfRange(-rand_y, rand_y);
				x += rng.RandfRange(-rand_x, rand_x);
				// Clamp generated points within bounds
				x = Mathf.Clamp(x, min.X + 0.1f, max.X - 0.1f);
				y = Mathf.Clamp(y, min.Y + 0.1f, max.Y - 0.1f);
				var new_vertex = new TriangleNet.Geometry.Vertex(x, y, 0);
				vertex_list.Add(new_vertex);
			}
		}
		
		var triangulator = new TriangleNet.Meshing.Algorithm.Dwyer();
		var mesh = triangulator.Triangulate(vertex_list, new TriangleNet.Configuration());

		List<int> corner_ids = new List<int>() {corner_vertex_1.ID, corner_vertex_2.ID, corner_vertex_3.ID, corner_vertex_4.ID};

		foreach(var i in mesh.Triangles) {
			var verts = new List<Vector3>();

			verts.Add(new Vector3((float) i.vertices[0].X, (float) i.vertices[0].Y, 0f));
			verts.Add(new Vector3((float) i.vertices[1].X, (float) i.vertices[1].Y, 0f));
			verts.Add(new Vector3((float) i.vertices[2].X, (float) i.vertices[2].Y, 0f));

			wall_fragment new_wall_fragment = wall_fragment_scene.Instantiate<wall_fragment>();
			new_wall_fragment.initializeWallFragment(verts);

			// If this fragment is connected to an end, we are not able to destroy it
			if (corner_ids.Contains(i.vertices[0].ID) || corner_ids.Contains(i.vertices[1].ID) || corner_ids.Contains(i.vertices[2].ID)) {
				new_wall_fragment.area.CollisionMask = 0b0;
			}

			AddChild(new_wall_fragment);
		}
	}

	private void placeStuds()
	{
		int num_studs = (int) Mathf.Floor(wall_width / stud_spacing_distance);

		float stud_position = -0.5f * wall_width;
		for (int i = 0; i < num_studs; i++) {
			MeshInstance3D new_stud = wall_stud_scene.Instantiate<MeshInstance3D>();
			new_stud.Position = new Vector3(stud_position, 0f, 0.3f);
			new_stud.Mesh.Set("size", new Vector3(0.45f, wall_height, stud_width));
			AddChild(new_stud);
			stud_position += stud_spacing_distance; 
		}

		// place stud at very end
		stud_position = 0.5f * wall_width;
		MeshInstance3D final_stud = wall_stud_scene.Instantiate<MeshInstance3D>();
		final_stud.Position = new Vector3(stud_position, 0f, 0.3f);
		final_stud.Mesh.Set("size", new Vector3(0.45f, wall_height, stud_width));
		AddChild(final_stud);
	}
}
