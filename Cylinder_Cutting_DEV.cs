using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

/*
 * 该类用于圆柱对皮肤的切割
 * 用于解决Cylinder_cutting类中 出现的切割不当问题
 * Cylinder_cutting存在的问题:
 * 基于切割的四边形 并不属于同一个平面 导致获取的切割面的环不在同一平面
 */
public class Cylinder_Cutting_DEV : MonoBehaviour
{
    int start = 0, end = 0;
    List<Vector3> vers;
    List<Vector3> nors;
    List<int> tris;
    //static bool cutTag = false;
    public GameObject Cylinder;
    Vector3[] CutPlanePoints = new Vector3[3];
    GameObject go;
    Transform Cylinder_TF;
    Vector3[] Cylinder_vers;
    //List<List<Vector3>> loops = new List<List<Vector3>>();
    List<List<Vector3>> loops;
    List<List<Vector3>> loops_tobeGizoms;
    List<Vector3> cutedge = new List<Vector3>(80);
    /*
     * 2021.03.10 by gh
     * 基于拼接解决思路 非切割算法本身 即:不解决根本问题
     * 解决思路:
     * 通过存储切割过程中产生的切点 通过排序获得闭环 然后直接根据闭环生成辅助针道
     */
    List<Vector3> edgeVertexSaver = new List<Vector3>(80);//预计切点在80以内

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //现改为按键操控
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("Cylinder_Cutting_DEV::开始");
            start = System.Environment.TickCount;

            /*
             * 2021.02.16记录
             * 应该为切割面设置有误 在观察
             * 
             */
            //初始化数组数据
            vers = new List<Vector3>(gameObject.GetComponent<MeshFilter>().mesh.vertices);
            nors = new List<Vector3>(gameObject.GetComponent<MeshFilter>().mesh.normals);
            tris = new List<int>(gameObject.GetComponent<MeshFilter>().mesh.triangles);
            go = gameObject;
            Cylinder_TF = Cylinder.transform;
            Cylinder_vers = Cylinder.GetComponent<MeshFilter>().mesh.vertices;
           
            //传入一个数组用于接收环上的顶点
            //List<Vector3> cutedge = new List<Vector3>(80);
            //传入一个数组用于保存索引
            //List<int> cutedgeIndex = new List<int>(80);
            for (int i = 0; i < 19; ++i)
            {
                //获取四个切点  注意 不在同一个平面！！
                //采用三个成一个面  点顺序为顺时针
                CutPlanePoints[2] = Cylinder_TF.TransformPoint(Cylinder_vers[i + 1 + 20]);//右下三角
                CutPlanePoints[1] = Cylinder_TF.TransformPoint(Cylinder_vers[i + 1]);
                CutPlanePoints[0] = Cylinder_TF.TransformPoint(Cylinder_vers[i]);
                MeshCutting_Summary.LocalCutting_Triangularchip(ref vers, ref tris, ref nors, transform, CutPlanePoints, ref cutedge);

                CutPlanePoints[0] = Cylinder_TF.TransformPoint(Cylinder_vers[i]);//左上三角
                CutPlanePoints[1] = Cylinder_TF.TransformPoint(Cylinder_vers[i + 20]);
                CutPlanePoints[2] = Cylinder_TF.TransformPoint(Cylinder_vers[i + 1 + 20]);
                MeshCutting_Summary.LocalCutting_Triangularchip(ref vers, ref tris, ref nors, transform, CutPlanePoints, ref cutedge);
                
                //continue;//测试
            }
            Debug.Log("==切割循环结束==");
            //cutTag = true;
            Debug.Log("~~~~~~~~~最后一步切割!!~~~~~~~~~~~~~~");
            //最后一个切割面单补(代码不易实现 故单补)
            {
                CutPlanePoints[0] = Cylinder_TF.TransformPoint(Cylinder_vers[20]);//右下三角
                CutPlanePoints[1] = Cylinder_TF.TransformPoint(Cylinder_vers[0]);
                CutPlanePoints[2] = Cylinder_TF.TransformPoint(Cylinder_vers[19]);
                MeshCutting_Summary.LocalCutting_Triangularchip(ref vers, ref tris, ref nors, transform, CutPlanePoints, ref cutedge);

                CutPlanePoints[0] = Cylinder_TF.TransformPoint(Cylinder_vers[39]);//左上三角
                CutPlanePoints[1] = Cylinder_TF.TransformPoint(Cylinder_vers[20]);
                CutPlanePoints[2] = Cylinder_TF.TransformPoint(Cylinder_vers[19]);
                MeshCutting_Summary.LocalCutting_Triangularchip(ref vers, ref tris, ref nors, transform, CutPlanePoints, ref cutedge);
                //Debug.Log("环的个数为: " + cutedge.Count);
                //更新网格 
                Mesh MS = gameObject.GetComponent<MeshFilter>().mesh;
                MS.vertices = vers.ToArray();
                MS.triangles = tris.ToArray();
                MS.normals = nors.ToArray();

                //最后网格分离
                //MeshCutting_Summary.IsolateMesh(ref MS, transform);
                IsolateMesh(gameObject);//分离操作
                //IsolateMesh(gameObject);//分离操作
                //cutTag = true;
                Debug.Log("======环形切割完成!!=====");
                end = System.Environment.TickCount;
                Debug.Log("切割总耗时：" + (end - start));
            }
            

            
        }

    }
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
            for (int i = 0; i < cutplanemark.Length; i++)
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
            //记录最小的网格标号 用于赋值给cutposition的TempMesh 仅限于腿骨切割
            int IndexOfMinMesh = 0;
            GameObject temp = null;
            for (int i = 0; i < count; i++)//
            {
                if (newTrianglesArrays[i].Count < newTrianglesArrays[IndexOfMinMesh].Count)
                {
                    IndexOfMinMesh = i;
                }
                /*if (newTrianglesArrays[i].Count < tri.Length / 500) continue;*/ //小于原网格1%的小碎片忽视掉（但可能有些小碎片是有用的）
                //GameObject part = new GameObject(beIsolatedThing.name + i.ToString());
                ///<summary>需要删除原物体碰撞器 再重新生成</summary>
                GameObject part = Instantiate(beIsolatedThing, null);
                //Destroy(part.GetComponent<MeshCollider>());
                //part.AddComponent<MeshCollider>();
                num++;
                part.name = beIsolatedThing.name + "[" + i.ToString() + "]";
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
            

            beIsolatedThing.SetActive(false);
        }
        else
        {
            int endTime = System.Environment.TickCount;
            Debug.Log("分离耗时：" + (endTime - startTime));
        }
    }
    private void OnDrawGizmos()
    {
        return;
        ////Debug.Log("开始OnDrawGizmos");
        ////Debug.Log("loops.Count: " + loops.Count);
        ////Debug.Log("loops!=null: " + (loops != null));
        //Debug.Log("cutedge!=null: " + (cutedge != null));
        //Debug.Log("cutedge大小: " + (cutedge.Count));
        //for (int i = 0; i < loops.Count; ++i)
        //{
        //    Gizmos.color = Color.red;
        //    //for(int j = 0; j < loops[i].Count; ++j)
        //    //{
        //    //    Gizmos.DrawSphere(transform.TransformPoint(loops[i][j]), 0.00008f);
        //    //}
        //    for (int j = 0; j < cutedge.Count / 2; ++j)
        //    {
        //        Gizmos.DrawSphere(transform.TransformPoint(cutedge[2 * i + 1]), 0.00008f);
        //    }
        //}
        //Debug.Log("结束");
        Debug.Log("开始");
        //显示三角形
        Cylinder_TF = Cylinder.transform;
        Cylinder_vers = Cylinder.GetComponent<MeshFilter>().mesh.vertices;
        for (int i = 1; i < 0; ++i)
        {
            //获取四个切点  注意 不在同一个平面！！
            //采用三个成一个面  点顺序为顺时针
            Gizmos.color = Color.red;
            
            Gizmos.DrawSphere(Cylinder_TF.TransformPoint(Cylinder_vers[i + 1 + 20]), 0.0008f);//右下三角
            Gizmos.DrawSphere(Cylinder_TF.TransformPoint(Cylinder_vers[i + 1]), 0.0008f);
            Gizmos.DrawSphere(Cylinder_TF.TransformPoint(Cylinder_vers[i]), 0.0008f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(Cylinder_TF.TransformPoint(Cylinder_vers[i]), 0.0008f);//右下三角
            Gizmos.DrawSphere(Cylinder_TF.TransformPoint(Cylinder_vers[i + 20]), 0.0008f);
            Gizmos.DrawSphere(Cylinder_TF.TransformPoint(Cylinder_vers[i + 1 + 20]), 0.0008f);
            //MeshCutting_Summary.LocalCutting_Triangularchip(ref vers, ref tris, ref nors, transform, CutPlanePoints, ref cutedge);

            //CutPlanePoints[0] = Cylinder_TF.TransformPoint(Cylinder_vers[i]);//左上三角
            //CutPlanePoints[1] = Cylinder_TF.TransformPoint(Cylinder_vers[i + 20]);
            //CutPlanePoints[2] = Cylinder_TF.TransformPoint(Cylinder_vers[i + 1 + 20]);
            //MeshCutting_Summary.LocalCutting_Triangularchip(ref vers, ref tris, ref nors, transform, CutPlanePoints, ref cutedge);

            //continue;//测试
        }


        //Debug.Log("四个点" + (MeshCutting_Summary.juge(Cylinder_TF.TransformPoint(Cylinder_vers[20]),
        //    Cylinder_TF.TransformPoint(Cylinder_vers[0]), Cylinder_TF.TransformPoint(Cylinder_vers[19]),
        //    Cylinder_TF.TransformPoint(Cylinder_vers[39])) ? "在" : "不在") + "同一平面");
        Debug.Log("四个点" + (MeshCutting_Summary.juge(Cylinder_vers[20], Cylinder_vers[0], Cylinder_vers[19], Cylinder_vers[39]) ? "在" : "不在") + "同一平面");

    }
}
