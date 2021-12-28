using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class yuanzhu : MonoBehaviour {
    Mesh mesh;
    List<Vector3> ver;
    List<Vector3> nor;
    List<int> tri;
    public Material material;

    public float thickness = 0.003f;
    // Use this for initialization
    void Start () {
        //初始化
        int start = System.Environment.TickCount;
        mesh = GetComponent<MeshFilter>().mesh;
        ver = new List<Vector3>(mesh.vertices);
        nor = new List<Vector3>(mesh.normals);
        tri = new List<int>(mesh.triangles);
        //遍历三角关系 提取仅出现一次的边
        AssistionMeshTools assistionMeshTools = new AssistionMeshTools(gameObject, material);

        List<Vector3> vers = new List<Vector3>();
        for(int i=0;i<ver.Count;++i)
        {
            //vers.Add(ver[i] + thickness * nor_avg);//局部坐标
        }
        List<int> tris = new List<int>(tri);
        List<Vector3> nors = new List<Vector3>(nor);
        tris.Reverse();

        List<int> edge1 = new List<int>();
        List<int> edge2 = new List<int>();

        for(int i=0;i<ver.Count/2;++i)
        {
            edge1.Add(i);
            edge2.Add(i + ver.Count / 2);
        }

        assistionMeshTools.Suture_TwoFaces(ref mesh, ver, nor, tri, vers, nors, tris, edge1);
        assistionMeshTools.Suture_TwoFaces(ref mesh, ver, nor, tri, vers, nors, tris, edge2);

        gameObject.AddComponent<MeshCollider>();
        assistionMeshTools.StlExporter(gameObject);
        Debug.Log("三角数： " + tri.Count);
        Debug.Log("顶点数： " + ver.Count);
        Debug.Log("法向量数： " + nor.Count);
        //assistionMeshTools.showMesh(ver, nor, tri,"NewMesh",transform);//已证明点坐标正确
        //ver.AddRange(vers); nor.AddRange(nors);
        //assistionMeshTools.WriteFile(ver, nor);
        Debug.Log("缝合耗时: " + (System.Environment.TickCount - start) / 1000 + " s");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
