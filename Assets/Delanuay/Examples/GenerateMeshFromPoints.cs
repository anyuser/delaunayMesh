using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class GenerateMeshFromPoints : MonoBehaviour {

    public Color edgeColor = Color.red;
    public Color color = Color.white;
    public Transform triangulationPlane;

	Mesh _mesh;
	Mesh mesh{
		get{
			if( !_mesh )
			{
				_mesh = new Mesh();
				_mesh.name = "Generated Mesh";
			}
			return _mesh;
		}
	}

    private void OnDrawGizmos()
    {
        if (!triangulationPlane)
            return;
        
		Gizmos.matrix = triangulationPlane.localToWorldMatrix * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * 10);
		Gizmos.DrawLine(new Vector3(1, 1, 0), new Vector3(-1, 1, 0));
		Gizmos.DrawLine(new Vector3(-1, -1, 0), new Vector3(-1, 1, 0));
		Gizmos.DrawLine(new Vector3(1, -1, 0), new Vector3(-1, -1, 0));
		Gizmos.DrawLine(new Vector3(1, 1, 0), new Vector3(1, -1, 0));
    }

    void OnEnable () {
	
		GetComponent<MeshFilter>().mesh = mesh;
		UpdateMesh();
	}
	
	void OnDisable()
	{
		if( mesh )
        {
			if (Application.isPlaying)
                Destroy(mesh);
			else
				DestroyImmediate(mesh);
        }
	}
	
	void Update () {
		
		if( !Application.isPlaying )
			UpdateMesh();
	}
	
	void UpdateMesh()
	{
	    // get points
		Vector3[] vertices = new Vector3[transform.childCount];
        for( int i = 0; i < transform.childCount; i++)
		{
			vertices[i] = transform.GetChild(i).localPosition;
		}

		// triangulate
		Vector3[] transformedVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
		{
            transformedVertices[i] = triangulationPlane ? triangulationPlane.InverseTransformPoint(vertices[i]): vertices[i];
		}
		int[] triangles = DelaunayTriangulator.Triangulator.ConvertToVertexIndexList(DelaunayTriangulator.Triangulator.Triangulate(transformedVertices));

		// generate colors
		Color[] colors = new Color[transform.childCount];
        for (int i = 0; i < colors.Length; i++)
		{
            colors[i] = color;
        }

        MeshExtrusion.Edge[] edges = MeshExtrusion.BuildManifoldEdges(vertices.Length, mesh.triangles);  // get edges to color them differently
        for (int i = 0; i < edges.Length; i++)
        {
            int vertId0 = edges[i].vertexIndex[0]; // one of the two points of the edge
            colors[vertId0] = edgeColor;
        }
		
        // update mesh
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
        mesh.colors = colors;
		mesh.RecalculateBounds();

	}
}
