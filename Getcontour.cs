using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

/// <summary>
/// 求出网格模型的轮廓
/// </summary>
public class Getcontour : MonoBehaviour {
    Mesh mesh;
    List<Vector3> ver;
    List<Vector3> nor;
    List<int> tri;
    public Material material;
    // Use this for initialization
    void Start () {
        //初始化
        int start = System.Environment.TickCount;
        mesh = GetComponent<MeshFilter>().mesh;
        ver = new List<Vector3>(mesh.vertices);
        nor = new List<Vector3>(mesh.normals);
        tri = new List<int>(mesh.triangles);
        //tri.Reverse();
        //GetComponent<MeshFilter>().mesh.triangles = tri.ToArray();//反转
        Debug.Log("三角数： " + tri.Count);
        Debug.Log("顶点数： " + ver.Count);
        Debug.Log("法向量数： " + nor.Count);
        Debug.Log("========");
        //遍历三角关系 提取仅出现一次的边
        AssistionMeshTools assistionMeshTools = new AssistionMeshTools(gameObject,material);



        List<Vector2Int> edgeLines = assistionMeshTools.TrianglesEdgeAnalysis(GetComponent<MeshFilter>().mesh.triangles);
        List<int> edgeIndex;
        List<List<Vector3>> result = assistionMeshTools.SpliteLines_single(edgeLines, GetComponent<MeshFilter>().mesh.vertices, out edgeIndex);

        List<Vector3> vers;
        List<Vector3> nors;
        List<int> tris;
        //assistionMeshTools.CopyMesh(mesh, transform, out vers, out tris, out nors);//复制网格

        vers = new List<Vector3>(mesh.vertices.Length);
        Vector3 nor_avg = (mesh.normals[0] + mesh.normals[mesh.normals.Length / 2] + mesh.normals[mesh.normals.Length - 1]) / 3;//法向量平均值  
        nor_avg.Normalize();
        float thickness = 0.003f;
        for (int i = 0; i < mesh.vertices.Length; ++i)
        {
            vers.Add(ver[i] + thickness * nor_avg);//局部坐标
        }
        tris = new List<int>(mesh.triangles);
        tris.Reverse();//翻转三角面
        nors = new List<Vector3>(nor.Count);
        for (int i = 0; i < mesh.normals.Length; ++i)
            nors.Add(-nor[i]);

        Debug.Log("轮廓上的节点数: " + edgeIndex.Count);
        /*
        for (int i = 0; i < edgeIndex.Count - 1; ++i)//缝合
        {
            tri.Add(edgeIndex[i] + ver.Count);
            tri.Add(edgeIndex[i + 1]);
            tri.Add(edgeIndex[i]);

            tri.Add(edgeIndex[i] + ver.Count);
            tri.Add(edgeIndex[i + 1] + ver.Count);
            tri.Add(edgeIndex[i + 1]);
        }
        {//最后两个三角形
            tri.Add(edgeIndex[edgeIndex.Count - 1] + ver.Count);
            tri.Add(edgeIndex[0]);
            tri.Add(edgeIndex[edgeIndex.Count - 1]);

            tri.Add(edgeIndex[0]);
            tri.Add(edgeIndex[0] + ver.Count);
            tri.Add(edgeIndex[edgeIndex.Count - 1] + ver.Count);
        }

        for (int i = 0; i < tris.Count; i ++)//合并
        {
            tri.Add(ver.Count + tris[i]);
            //tri.Add(ver.Count + tris[i + 1]);
            //tri.Add(ver.Count + tris[i + 2]);
        }
        ver.AddRange(vers);
        nor.AddRange(nors);
        //for (int i = 0; i < nor.Count; ++i)
        //    nors.Add(-nor[i]);
        //Debug.Log("三角数： " + tri.Count);

        //通过赋值查看生成情况
        GetComponent<MeshFilter>().mesh.vertices = ver.ToArray();
        GetComponent<MeshFilter>().mesh.normals = nor.ToArray();
        GetComponent<MeshFilter>().mesh.triangles = tri.ToArray();

        */
        assistionMeshTools.Suture_TwoFaces(ref mesh, ver, nor, tri, vers, nors, tris,edgeIndex);

        gameObject.AddComponent<MeshCollider>();
        assistionMeshTools.StlExporter(gameObject);
        Debug.Log("三角数： " + tri.Count);
        Debug.Log("顶点数： " + ver.Count);
        Debug.Log("法向量数： " + nor.Count);
        //assistionMeshTools.showMesh(ver, nor, tri,"NewMesh",transform);//已证明点坐标正确
        //ver.AddRange(vers); nor.AddRange(nors);
        //assistionMeshTools.WriteFile(ver, nor);
        Debug.Log("缝合耗时: " + (System.Environment.TickCount - start)/1000 + " s");
    }

    
    private void Update()
    {
        
    }







}
