using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

public partial class wall_fragment : Node3D
{
	[Export]
	public MeshInstance3D array_mesh;

	[Export]
	public CollisionPolygon3D collision_polygon;
	[Export]
	public Area3D area;
	[Export]
	public CpuParticles3D wall_particles;

	[Signal]
	public delegate void onWallDestroyEventHandler(float fragment_area);

	public Vector3 trianglePosition;
	public Vector3 triangleNormal;
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void initializeWallFragment(List<Vector3> verts) 
	{
		var surfaceArray = new Godot.Collections.Array();
		surfaceArray.Resize((int)Mesh.ArrayType.Max);

		// Convert Lists to arrays and assign to surface array
		surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();

		var arrMesh = new ArrayMesh();
		if (arrMesh != null)
		{
			// Create mesh surface from mesh array
			// No blendshapes, lods, or compression used.
			arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
		}

		array_mesh.Mesh = arrMesh;

		collision_polygon.Polygon = new Vector2[] { new Vector2(verts[0].X, verts[0].Y), new Vector2(verts[1].X, verts[1].Y), new Vector2(verts[2].X, verts[2].Y)};
		
		// Change particle position to be the average of the polygon
		wall_particles.Position = (((Vector3) verts[0]) + ((Vector3) verts[1]) + ((Vector3) verts[2])) / 3f;

		Vector2 fragment_center = getFragmentCenter();
		trianglePosition = new Vector3(fragment_center.X, fragment_center.Y, 0);
	}

	public void _on_area_3d_area_entered(Area3D area)
	{
		Vector3 hit_dir = area.GlobalPosition.DirectionTo(GlobalPosition);
		 
		// Vector3 hit_dir = area.GlobalBasis * Vector3.Forward;
		if (hit_dir.Dot(triangleNormal) > 0) {
			return;
		}

		wall_particles.Emitting = true;
		array_mesh.Visible = false;
		//area.SetDeferred("monitorable", false);
		collision_polygon.SetDeferred("disabled", true);
		EmitSignal(SignalName.onWallDestroy, getFragmentArea());
	}

	public float getFragmentArea()
	{
		Vector2 v1 = collision_polygon.Polygon[0];
		Vector2 v2 = collision_polygon.Polygon[1];
		Vector2 v3 = collision_polygon.Polygon[2];

		return 0.5f * Mathf.Abs(v1.X * (v2.Y - v3.Y) + v2.X * (v3.Y - v1.Y) + v3.X * (v1.Y - v2.Y));
	}

	public Vector2 getFragmentCenter()
	{
		Vector2 v1 = collision_polygon.Polygon[0];
		Vector2 v2 = collision_polygon.Polygon[1];
		Vector2 v3 = collision_polygon.Polygon[2];
		return (v1 + v2 + v3) / 3f;
	}

	public Vector3 getNormal()
	{
		Vector3 v1 = new Vector3(collision_polygon.Polygon[0].X, collision_polygon.Polygon[0].Y, 0);
		Vector3 v2 = new Vector3(collision_polygon.Polygon[1].X, collision_polygon.Polygon[1].Y, 0);
		Vector3 v3 = new Vector3(collision_polygon.Polygon[2].X, collision_polygon.Polygon[2].Y, 0);

		return ((v3 - v1).Normalized()).Cross((v2 - v1).Normalized()).Normalized();
	}

	public void _on_cpu_particles_3d_finished() 
	{
		QueueFree();
	}

	public void setColor(Color c)
	{
		array_mesh.MaterialOverride.Set("albedo_color", c);
		wall_particles.Mesh.SurfaceGetMaterial(0).Set("albedo_color", c);
	}

	public void _on_tree_entered()
	{
		triangleNormal = (GlobalBasis * getNormal()).Normalized();
	}
}
