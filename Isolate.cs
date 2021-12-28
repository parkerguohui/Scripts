using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Isolate : MonoBehaviour {
    //private GameObject beIsolatedThing;
    // Use this for initialization

    //public static Material Transparent;
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetKeyDown("i"))
        //{
        //    RaycastHit raycastHit = new RaycastHit();
        //    Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    if (Physics.Raycast(mouseRay, out raycastHit))
        //    {
        //        Debug.Log("hit！");
        //        beIsolatedThing = raycastHit.collider.gameObject;
        //        MeshFilter face = raycastHit.collider.GetComponent<MeshFilter>();
        //        if (face)
        //        {
        //            IsolateMesh(beIsolatedThing);

        //        }
        //    }
        //}
    }
    ///<summary>
    ///</summary>
    ///<param name="beIsolatedThing">被切割物体</param>
    
    public static void IsolateMesh(GameObject beIsolatedThing)
    {
        Debug.Log("isolate begin~~~");
        int startTime = System.Environment.TickCount;
        Mesh mesh = beIsolatedThing.GetComponent<MeshFilter>().mesh;
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
        //Debug.Log(union.getCount());
        int num = 0;
        
        int count = union.getCount();

        if (count >= 2)              //分离
        {
            ///<summary>
            ///先清除切片标记
            ///</summary>
            GameObject[] cutplanemark = GameObject.FindGameObjectsWithTag("temp_cutplane_mark");
            for(int i=0;i<cutplanemark.Length;i++)
            {
                DestroyImmediate(cutplanemark[i]);//删除标记
            }

            List<int>[] arrays = union.getResult();
            int[] oldToNewVertexMap = new int[vertices.Length];
            int[] partNumber = new int[vertices.Length];

            List<Vector3>[] newVerticesArrays = new List<Vector3>[count];
            List<Vector3>[] newNormalsArrays = new List<Vector3>[count];
            List<int>[] newTrianglesArrays = new List<int>[count];

            for (int i = 0; i < count; i++) //初始化新顶点数组
            {
                newVerticesArrays[i] = new List<Vector3>(vertices.Length/2);
                newNormalsArrays[i] = new List<Vector3>(vertices.Length/2);
                newTrianglesArrays[i] = new List<int>(tri.Length/2);
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
            //记录最小的网格标号 用于赋值给cutposition的TempMesh 仅限于腿骨切割
            int IndexOfMinMesh = 0;
            GameObject temp = null;
            for (int i = 0; i < count; i++)//
            {
                if (newTrianglesArrays[i].Count < newTrianglesArrays[IndexOfMinMesh].Count)
                {
                    IndexOfMinMesh = i;
                }
                if (newTrianglesArrays[i].Count < tri.Length/500) continue; //小于原网格1%的小碎片忽视掉（但可能有些小碎片是有用的）
                //GameObject part = new GameObject(beIsolatedThing.name + i.ToString());
                ///<summary>需要删除原物体碰撞器 再重新生成</summary>
                GameObject part = Instantiate(beIsolatedThing, null);
                //Destroy(part.GetComponent<MeshCollider>());
                //part.AddComponent<MeshCollider>();
                num++;
                part.name = beIsolatedThing.name +"["+ i.ToString() + "]";
                //part.AddComponent<MeshFilter>();
                //part.AddComponent<MeshRenderer>();
                part.GetComponent<MeshFilter>().mesh.Clear();
                part.GetComponent<MeshFilter>().mesh.vertices = newVerticesArrays[i].ToArray();
                part.GetComponent<MeshFilter>().mesh.normals = newNormalsArrays[i].ToArray();
                part.GetComponent<MeshFilter>().mesh.triangles = newTrianglesArrays[i].ToArray();
                part.GetComponent<MeshRenderer>().material = beIsolatedThing.GetComponent<MeshRenderer>().material;
                if (newVerticesArrays[i].Count < n)
                {
                    n = newVerticesArrays[i].Count;
                    temp = part;
                    //temp.name = "temp[" + i + "]";
                    //Debug.Log("更小的是：" + part);
                }
                if (num == 1)
                {
                    //GameObject gg = GameObject.FindGameObjectWithTag("ChosenObject");
                    //if (gg != null)
                    //{
                    //    part.tag = "Objects";
                    //}
                    part.tag = "ChosenObject";
                }
                else
                {
                    part.tag = "Objects";
                }
                
                //part.AddComponent<MeshCollider>();
                //part.GetComponent<MeshCollider>().convex = true;
                //part.GetComponent<MeshCollider>().cookingOptions = MeshColliderCookin`gOptions.InflateConvexMesh;
            }

            CutPosition.TempMeshSaver = new Mesh();
            CutPosition.TempMeshSaver.vertices = newVerticesArrays[IndexOfMinMesh].ToArray();
            CutPosition.TempMeshSaver.triangles = newTrianglesArrays[IndexOfMinMesh].ToArray();
            CutPosition.TempMeshSaver.normals = newNormalsArrays[IndexOfMinMesh].ToArray();
            //清理所有被选中物体 因为可能存在复制问题 导致标签重复 2020.08.08
            GameObject[] ChosenObjects = GameObject.FindGameObjectsWithTag("ChosenObject");
            for (int i = 0; i < ChosenObjects.Length; i++)
                ChosenObjects[i].tag = "Objects";
            //记录数据
            if (num > 1)
            {
                CutPosition.DoubleHolderToolHelper = GameObject.Instantiate(temp);
                CutPosition.DoubleHolderToolHelper.name = "DoubleHolderToolHelper";
                CutPosition.DoubleHolderToolHelper.SetActive(false);
                //CutPosition.DoubleHolderToolHelper.GetComponent<MeshRenderer>().material = Transparent;
                DestroyImmediate(CutPosition.DoubleHolderToolHelper.GetComponent<MeshCollider>());
                DestroyImmediate(CutPosition.DoubleHolderToolHelper.GetComponent<Choosable>());
                DestroyImmediate(CutPosition.DoubleHolderToolHelper.GetComponent<Interactable>());
            }

            beIsolatedThing.SetActive(false);
        }
        else
        {
            int endTime = System.Environment.TickCount;
            Debug.Log("分离耗时：" + (endTime - startTime));
        }
    }
}
