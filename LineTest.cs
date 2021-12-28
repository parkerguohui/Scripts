using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineTest : MonoBehaviour {
    Mesh mesh;
    List<Vector3> ver;
    List<Vector3> nor;
    List<int> tri;
    public Material material;
    // Use this for initialization
    void Start () {
        mesh = GetComponent<MeshFilter>().mesh;
        ver = new List<Vector3>(mesh.vertices);
        nor = new List<Vector3>(mesh.normals);
        tri = new List<int>(mesh.triangles);
        AssistionMeshTools assistionMeshTools = new AssistionMeshTools(gameObject, material);

        List<Vector2Int> edgeLines = assistionMeshTools.TrianglesEdgeAnalysis(GetComponent<MeshFilter>().mesh.triangles);
        List<int> edgeIndex;
        List<List<Vector3>> result = assistionMeshTools.SpliteLines_single(edgeLines, GetComponent<MeshFilter>().mesh.vertices, out edgeIndex);

        List<Vector3> vers=new List<Vector3>(edgeLines.Count);
        List<Vector3> nors = new List<Vector3>(edgeLines.Count);
        List<int> tris = new List<int>(edgeLines.Count);
        for (int i = 0; i < result[0].Count; ++i)
        {
            vers.Add(result[0][i]);
        }
        for (int i = 0; i < result[0].Count; ++i)
        {
            vers.Add(result[0][i]);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
