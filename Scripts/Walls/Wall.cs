using AwesomeNamespace;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class Wall : Node3D
{
	[Export]
	public MeshInstance3D wall_background;
	[Export]
	public CollisionShape3D wall_collider;
	[Export]
	public MeshInstance3D editor_wall_rep;
	[Export]
	public StandardMaterial3D wall_material;

	
	[Export]
	public float wall_width = 5f;
	[Export]
	public float wall_height = 3.5f;
	[Export]
	public float wall_thickness = 0.5f;
	[Export]
	public bool double_sided = true;
	[Export]
	public bool cap_start_wall = false;
	[Export]
	public bool cap_end_wall = false;

	public float wall_collision_padding = 1f;

	[Export]
	public Color wall_color = Colors.Aqua;

	public float stud_spacing_distance = 0.95f;
	public float stud_width = 0.1f;

	private List<wall_fragment> wall_fragments = new List<wall_fragment>();

	[Export]
	public PackedScene wall_fragment_scene;
	[Export]
	public PackedScene wall_stud_scene;

	[Signal]
	public delegate void onWallFragmentDestroyedEventHandler(float fragment_area);

	private Callable wall_fragment_signal_callable;
	private List<int> corner_ids = new List<int>(); 
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		wall_fragment_signal_callable = new Callable(this, MethodName.emitDestroySignal);
		editor_wall_rep.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// if (Engine.IsEditorHint())
		// {
		// 	// change wall background 
		// 	//wall_background.Mesh.Set("size", new Vector2(wall_width, wall_height));
		// 	editor_wall_rep.Mesh.Set("size", new Vector2(wall_width, wall_height));
		// }
	}

	public void build()
	{
		wall_fragment_signal_callable = new Callable(this, MethodName.emitDestroySignal);
		initializeWall();
		CallDeferred(MethodName.updateTriangles);
	}

	private void initializeWall()
	{
		Vector3 wall_pos_min = new Vector3(-0.5f * wall_width, -0.5f * wall_height, 0);
		Vector3 wall_pos_max = new Vector3(0.5f * wall_width, 0.5f * wall_height, 0);
		generateTriangles(wall_pos_min, wall_pos_max, 25);
		
		// change wall collsion
		wall_collider.Shape = new BoxShape3D();
		wall_collider.Shape.Set("size", new Vector3(wall_width + wall_collision_padding, wall_height, wall_thickness + wall_collision_padding));

		// change wall background 
		PlaneMesh wall_bg_mesh = new PlaneMesh();
		wall_bg_mesh.Orientation = PlaneMesh.OrientationEnum.Z;
		wall_background.Mesh = wall_bg_mesh;
		wall_background.Mesh.Set("size", new Vector2(wall_width, wall_height));
		//wall_background.Position = new Vector3(0, 0, 0.5f);

		placeStuds(false);
		if (double_sided) {
			placeStuds(true);
		}

		if (cap_start_wall)
			capStart();
		
		if (cap_end_wall)
			capEnd();
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

		foreach (Vector2 v in UniformPoissonDiskSampler.SampleRectangle(new Vector2(min.X, min.Y), new Vector2(max.X, max.Y), 0.35f)) {
			var new_vertex = new TriangleNet.Geometry.Vertex(v.X, v.Y, 0);
			vertex_list.Add(new_vertex);	
		}
		
		var triangulator = new TriangleNet.Meshing.Algorithm.Dwyer();
		var mesh = triangulator.Triangulate(vertex_list, new TriangleNet.Configuration());

		//List<int> corner_ids = new List<int>();  //{corner_vertex_1.ID, corner_vertex_2.ID, corner_vertex_3.ID, corner_vertex_4.ID};
		for (int i = 0; i < 4; i++) {
			corner_ids.Add(vertex_list[i].ID);
		}

		foreach(var i in mesh.Triangles) {

			wall_fragment wall_fragment_primary = createWallFragment(i, false);
			
			AddChild(wall_fragment_primary);
			wall_fragments.Add(wall_fragment_primary);

			if (double_sided)
			{
				wall_fragment wall_fragment_secondary = createWallFragment(i, true);
			
				AddChild(wall_fragment_secondary);
				wall_fragments.Add(wall_fragment_secondary);
			}
		}
	}

	// This function will check if any triangles overlap with non-breakable areas (SPECIFICALLY AREAS IN COLLISON LAYER 3!!!)
	// These triangles will not be breakable
	// Say if you want a door on a wall, it needs an area in layer 3 
	private void updateTriangles()
	{

		foreach (wall_fragment fragment in wall_fragments) {
			var point_q_params = new PhysicsPointQueryParameters3D();
			point_q_params.CollideWithAreas = true;
			point_q_params.CollisionMask = 4;
			Vector2 triangle_center = fragment.getFragmentCenter();
			Vector3 triangle_center_global = Transform * (new Vector3(triangle_center.X, triangle_center.Y, 0));
			point_q_params.Position = triangle_center_global;
			var point_collisions = PhysicsServer3D.SpaceGetDirectState(GetWorld3D().Space).IntersectPoint(point_q_params);
			if (point_collisions.Count > 0) {
				fragment.area.CollisionMask = 0b0;
				//fragment.Visible = false;
			}  
		}
	}

	private void emitDestroySignal(float fragment_area)
	{
		EmitSignal(SignalName.onWallFragmentDestroyed, fragment_area);
	}

	private void placeStuds(bool flipped)
	{
		int num_studs = (int) Mathf.Floor(wall_width / stud_spacing_distance);

		// place stud at very start
		float stud_position = -0.5f * wall_width + 0.5f * stud_width + 0.01f;
		MeshInstance3D first_stud = wall_stud_scene.Instantiate<MeshInstance3D>();
		if (flipped) {
			first_stud.Position = new Vector3(stud_position, 0f, 0.25f * wall_thickness);
			first_stud.RotateY(Mathf.DegToRad(180));
		} else {
			first_stud.Position = new Vector3(stud_position, 0f, -0.25f * wall_thickness);
		}
		first_stud.Mesh.Set("size", new Vector3(0.49f * wall_thickness, wall_height, stud_width));
		AddChild(first_stud);

		stud_position = -0.5f * wall_width + 0.5f * stud_width + stud_spacing_distance;
		for (int i = 1; i < num_studs; i++) {
			MeshInstance3D new_stud = wall_stud_scene.Instantiate<MeshInstance3D>();
			if (flipped) {
				new_stud.Position = new Vector3(stud_position, 0f, 0.25f * wall_thickness);
				new_stud.RotateY(Mathf.DegToRad(180));
			} else {
				new_stud.Position = new Vector3(stud_position, 0f, -0.25f * wall_thickness);
			}
			AddChild(new_stud);
			stud_position += stud_spacing_distance; 
		}

		// place stud at very end
		stud_position = 0.5f * wall_width - 0.5f * stud_width - 0.01f;
		MeshInstance3D final_stud = wall_stud_scene.Instantiate<MeshInstance3D>();
		if (flipped) {
			final_stud.Position = new Vector3(stud_position, 0f, 0.25f * wall_thickness);
			final_stud.RotateY(Mathf.DegToRad(180));
		} else {
			final_stud.Position = new Vector3(stud_position, 0f, -0.25f * wall_thickness);
		}
		final_stud.Mesh.Set("size", new Vector3(0.49f * wall_thickness, wall_height, stud_width));
		AddChild(final_stud);
	}

	public float getWallArea()
	{
		return wall_width * wall_height;
	}

	private wall_fragment createWallFragment(TriangleNet.Topology.Triangle triangle, bool flipped)
	{
		var verts = new List<Vector3>();

		verts.Add(new Vector3((float) triangle.vertices[0].X, (float) triangle.vertices[0].Y, 0f));
		verts.Add(new Vector3((float) triangle.vertices[1].X, (float) triangle.vertices[1].Y, 0f));
		verts.Add(new Vector3((float) triangle.vertices[2].X, (float) triangle.vertices[2].Y, 0f));
		
		wall_fragment new_wall_fragment = wall_fragment_scene.Instantiate<wall_fragment>();
		new_wall_fragment.initializeWallFragment(verts);
		new_wall_fragment.setColor(wall_color);
		if (flipped) {
			new_wall_fragment.Position += new Vector3(0, 0, 0.5f * wall_thickness);
			new_wall_fragment.RotateY(Mathf.DegToRad(180));
		} else {
			new_wall_fragment.Position -= new Vector3(0, 0, 0.5f * wall_thickness);
		}

		new_wall_fragment.Connect(wall_fragment.SignalName.onWallDestroy, wall_fragment_signal_callable);

		// If this fragment is connected to an end, we are not able to destroy it
		if (corner_ids.Contains(triangle.vertices[0].ID) || corner_ids.Contains(triangle.vertices[1].ID) || corner_ids.Contains(triangle.vertices[2].ID)) {
			new_wall_fragment.area.CollisionMask = 0b0;
		}

		return new_wall_fragment;
	}

	private void capStart()
	{
		MeshInstance3D cap_mesh = new MeshInstance3D();
		cap_mesh.Mesh = new PlaneMesh();
		cap_mesh.Mesh.SurfaceSetMaterial(0, wall_material);
		cap_mesh.Mesh.Set("size", new Vector2(wall_thickness, wall_height));
		cap_mesh.RotationDegrees = new Vector3(-90, -90, 0);
		cap_mesh.Position = new Vector3(0.5f * wall_width, 0, 0);

		AddChild(cap_mesh);
	}

	private void capEnd()
	{
		MeshInstance3D cap_mesh = new MeshInstance3D();
		cap_mesh.Mesh = new PlaneMesh();
		cap_mesh.Mesh.SurfaceSetMaterial(0, wall_material);
		cap_mesh.Mesh.Set("size", new Vector2(wall_thickness, wall_height));
		cap_mesh.RotationDegrees = new Vector3(90, -90, 0);
		cap_mesh.Position = new Vector3(-0.5f * wall_width, 0, 0);

		AddChild(cap_mesh);
	}
}
