using UnityEngine;
using System.Collections;

// this quick&dirty script creates a disc shaped mesh based on a polar grid and assigns it to a mesh renderer and mesh collider
//	!!! ONLY VALID FOR XZ-GRIDS !!!

[RequireComponent(typeof(GFPolarGrid))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class DiscMeshGenerator : MonoBehaviour {
	
	void Awake () {
		// store the components for later reference
		GFPolarGrid grid = GetComponent<GFPolarGrid> ();
		MeshFilter filter = GetComponent<MeshFilter> ();
		MeshCollider collider = GetComponent<MeshCollider> ();

		// this is the mesh we will build
		Mesh disc = new Mesh ();
		disc.Clear();

		// we need to fill every sector and each sector is made os smaller secors given by the smoothness
		int segments = grid.sectors * grid.smoothness;
		Vector3[] vertices = new Vector3[segments + 2]; // +1 for the origin and +1 too loop around
		vertices [0] = Vector3.zero; // the origin
		for (int i = 1; i <= segments; i++) {
			//the world points around the grid in local space
			vertices [i] = transform.InverseTransformPoint (grid.GridToWorld( new Vector3(grid.size.x / grid.radius, 0, (float)i / (float)grid.smoothness)));
		}
		vertices[segments + 1] = vertices[1]; // loop around after one full circle
		disc.vertices = vertices; // assign the vertices

		int[] triangles = new int[segments * 3]; // the amount of triangles times three
		int counter = 0;
		for (int i = 1; i <= segments; i++) { // assign the triangels in a clockwise rotation
			triangles [counter] = 0; //origin
			triangles [counter + 1] = i + 1; // upper
			triangles [counter + 2] = i; // lower
			counter += 3; // increment the counter for the next three vertices
		}
		disc.triangles = triangles; // assign the triangles

		// add some dummy UVs to keep the shader happy or else it complains, but they are not used in this example
		Vector2[] uvs = new Vector2[vertices.Length];
		for (int k = 0; k < uvs.Length; k++) {
			uvs[k] = new Vector2(vertices[k].x, vertices[k].y);
		}
		disc.uv = uvs;

		// the usual cleanup
		disc.RecalculateNormals();
		disc.RecalculateBounds();
		disc.Optimize();

		// assign the mesh
		filter.sharedMesh = disc;
		collider.sharedMesh = disc;
	}
}