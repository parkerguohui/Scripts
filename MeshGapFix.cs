using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class MeshGapFix : MonoBehaviour {
	Mesh mesh;

    static List<Vector3>[] newVerticesArrays;
    static List<Vector3>[] newNormalsArrays;
    static List<int>[] newTrianglesArrays;
    // Use this for initialization
    void Start () {
		mesh = new Mesh();
		mesh = gameObject.GetComponent<MeshFilter>().mesh;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
			//fun1();
			fun2();
        }
    }
	void fun1()
    {
		int start = System.Environment.TickCount;
		Debug.Log("修复前的顶点数：" + mesh.vertices.Length);
		Debug.Log("修复前的三角面数：" + (mesh.triangles.Length / 3));
		mesh = MergeVertices.MergeCloseVertices(mesh);
		Debug.Log("修复后的顶点数：" + mesh.vertices.Length);
		Debug.Log("修复后的三角面数：" + (mesh.triangles.Length / 3));
		Debug.Log("修复耗时:" + (System.Environment.TickCount - start));
	}
	void fun2()
    {
        Mesh mesh = new Mesh();
        mesh = gameObject.GetComponent<MeshFilter>().mesh;
        IsolateMesh(mesh);
        for(int i = 0; i < newVerticesArrays.Length; ++i)
        {

        }
    }
    public static void IsolateMesh(Mesh mesh)
    {
        int startTime = System.Environment.TickCount;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        int[] tri = mesh.triangles;
        UnionFind union = new UnionFind(vertices.Length);
        for (int i = 0; i < tri.Length; i = i + 3) //连通三角面
        {
            union.union(tri[i], tri[i + 1]);
            union.union(tri[i + 1], tri[i + 2]);
            union.union(tri[i + 2], tri[i]);
        }
        Dictionary<Vector3, int> dictionary = new Dictionary<Vector3, int>();
        for (int i = 0; i < vertices.Length; i++) //连通重复点
        {

            if (dictionary.ContainsKey(vertices[i]))
            {
                union.union(i, dictionary[vertices[i]]);
            }
            else
            {
                dictionary.Add(vertices[i], i);
            }
        }
        int num = 0;

        int count = union.getCount();

        if (count >= 2)//分离
        {
            List<int>[] arrays = union.getResult();
            int[] oldToNewVertexMap = new int[vertices.Length];
            int[] partNumber = new int[vertices.Length];

            //List<Vector3>[] newVerticesArrays = new List<Vector3>[count];
            //List<Vector3>[] newNormalsArrays = new List<Vector3>[count];
            //List<int>[] newTrianglesArrays = new List<int>[count];

            newVerticesArrays = new List<Vector3>[count];
            newNormalsArrays = new List<Vector3>[count];
            newTrianglesArrays = new List<int>[count];

            for (int i = 0; i < count; i++) //初始化新顶点数组
            {
                newVerticesArrays[i] = new List<Vector3>(vertices.Length / 2);
                newNormalsArrays[i] = new List<Vector3>(vertices.Length / 2);
                newTrianglesArrays[i] = new List<int>(tri.Length / 2);
            }

            for (int i = 0; i < count; i++)//映射 旧顶点与新顶点
            {
                List<int> array = arrays[i];
                for (int j = 0; j < array.Count; j++)
                {
                    oldToNewVertexMap[array[j]] = j;
                    partNumber[array[j]] = i;

                    newVerticesArrays[i].Add(vertices[array[j]]);
                    newNormalsArrays[i].Add(normals[array[j]]);
                }
            }

            for (int i = 0; i < tri.Length; i = i + 3)//映射 旧三角面与新三角面
            {
                int index = partNumber[tri[i]];

                newTrianglesArrays[index].Add(oldToNewVertexMap[tri[i]]);
                newTrianglesArrays[index].Add(oldToNewVertexMap[tri[i + 1]]);
                newTrianglesArrays[index].Add(oldToNewVertexMap[tri[i + 2]]);
            }
            int endTime = System.Environment.TickCount;
            //用于记录较小的那部分 用于创建双导板时使用
            int n = int.MaxValue;
            Debug.Log("分离耗时：" + (endTime - startTime));
        }
        else
        {
            int endTime = System.Environment.TickCount;
            Debug.Log("分离耗时：" + (endTime - startTime));
        }
    }
}
