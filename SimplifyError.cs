using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplifyError : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        Vector3[] ver = mesh.vertices;
        Vector3[] nor = mesh.normals;
        int[] tri = mesh.triangles;

        Vector3[] v = new Vector3[ver.Length*2];
        Vector3[] n = new Vector3[nor.Length*2];
        int[] t = new int[tri.Length*2];

        for (int i = 0; i < ver.Length; i++)
        {
            v[i] = ver[i];
            v[i+ ver.Length] = ver[i];
            n[i] = nor[i];
            n[2 + ver.Length] = -nor[i];
        }

        for (int i = 0;i<tri.Length;i++)
        {
            t[i] = tri[i];
            t[t.Length - 1 - i] = tri[i]+ ver.Length;
        }
        mesh.vertices = v;
        mesh.normals = n;
        mesh.triangles = t;

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
