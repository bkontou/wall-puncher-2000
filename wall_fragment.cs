using Godot;
using System;
using System.Collections.Generic;
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
	}

	public void _on_area_3d_area_entered(Area3D area)
	{
		wall_particles.Emitting = true;
		array_mesh.Visible = false;
	}

	public void _on_cpu_particles_3d_finished() 
	{
		QueueFree();
	}
}
