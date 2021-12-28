using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;
//未做切面缝合
using System.IO;
using System.Text;
//Tempd
public class Cross_vertices
{
    public int VerticesIndex;
    public int index;
}

public class CutPlaneTool : MonoBehaviour {
    private GameObject beCutThing;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetMouseButtonDown(1) || SteamVR_Actions.default_Teleport.GetStateDown(SteamVR_Input_Sources.Any))
        if(Input.GetKeyDown(KeyCode.C))//读取键盘 键入C时 执行
        {
            if (gameObject.tag == "ChosenTool")
            {
                beCutThing = GameObject.FindGameObjectWithTag("ChosenObject");
                Debug.Log("beCutThing: " + beCutThing.name);
                //注意  自己将标签改为chosentool 后面需自行改回
                if (beCutThing != null)
                {
                    
                    Debug.Log("cut begin~~~");
                    Cut();
                }
                else
                {
                    Debug.Log("there is nothing to cut!");
                }

            }
            


        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            MeshCutting_Summary.StlExporter(gameObject);//导出
        }
    }
    /// <summary>
    /// 平面切
    /// </summary>
    private void Cut()
    {
        int start = System.Environment.TickCount;
        //根据本切割工具网格顶点形成切割面
        Vector3[] v = transform.GetComponent<MeshFilter>().mesh.vertices;//transform 代指切割平面

        Plane cutPlane = new Plane(transform.TransformPoint(v[0]), transform.TransformPoint(v[1]), transform.TransformPoint(v[v.Length-1]));
        //Debug.Log(cutPlane.normal);

        //把切割面转化为被切割物体的当地坐标
        Vector3 localPoint = beCutThing.transform.InverseTransformPoint(cutPlane.normal * -cutPlane.distance); 
        //Debug.Log(localPoint);
        Vector3 localSplitPlaneNormal = beCutThing.transform.InverseTransformDirection(cutPlane.normal);
        localSplitPlaneNormal.Normalize();

        //根据切割面，把顶点分成两部分
        
        Vector3[] vertices = beCutThing.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] normals = beCutThing.GetComponent<MeshFilter>().mesh.normals;
        bool[] vertexAbovePlane;
        vertexAbovePlane = new bool[vertices.Length];//与平面位置关系的标记数组
        int[] oldToNewVertexMap = new int[vertices.Length];

        List<Vector3> newVertices1 = new List<Vector3>(vertices.Length);//为了避免数组多次动态增加容量，给最大值
        List<Vector3> newVertices2 = new List<Vector3>(vertices.Length);
        List<Vector3> newNormals1 = new List<Vector3>(normals.Length);
        List<Vector3> newNormals2 = new List<Vector3>(normals.Length);

        //测试（删）
        if (vertices.Length > 0)
            Debug.Log("不为空");
        else
            Debug.Log("为空");


        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            bool abovePlane = Vector3.Dot(vertex - localPoint, localSplitPlaneNormal) >= 0.0f;//原理？
            vertexAbovePlane[i] = abovePlane;
            if (abovePlane)
            {
                // Assign vertex to 1
                oldToNewVertexMap[i] = newVertices1.Count;
                newVertices1.Add(vertex);
                newNormals1.Add(normals[i]);
            }
            else
            {
                // Assign vertex to 2
                oldToNewVertexMap[i] = newVertices2.Count;
                newVertices2.Add(vertex);
                newNormals2.Add(normals[i]);
            }
        }
        if (newVertices1.Count==0|| newVertices2.Count == 0)
        {
            Debug.Log("nothing be cut,over");
            return;
        }
        
        
        
        //分三角面（关键）
        int[] indices = beCutThing.GetComponent<MeshFilter>().mesh.triangles;
        int triangleCount = indices.Length / 3;
        List<int> newTriangles1 = new List<int>(indices.Length);
        List<int> newTriangles2 = new List<int>(indices.Length);
        List<int>  cutPointsIndex = new List<int>();

        ///<value>横截面关系</value>
        //List<Cross_vertices> VerticesIntersection1 = new List<Cross_vertices>(vertices.Length);//给一个较大值
        //List<Cross_vertices> VerticesIntersection2 = new List<Cross_vertices>(vertices.Length);
        //int n1 = 0,n2=0;

        List<int> VerticesIntersection1 = new List<int>(vertices.Length);
        List<int> VerticesIntersection2 = new List<int>(vertices.Length);

        for (int i = 0; i < triangleCount; i++) // 遍历每一个三角面
        {
            int index0 = indices[i * 3 + 0];//（被切物体）三角形索引数组 其中的值为顶点数组中的下标 
            int index1 = indices[i * 3 + 1];
            int index2 = indices[i * 3 + 2];

            bool above0 = vertexAbovePlane[index0];
            bool above1 = vertexAbovePlane[index1];
            bool above2 = vertexAbovePlane[index2];

            if (above0 && above1 && above2)//上方
            {
                newTriangles1.Add(oldToNewVertexMap[index0]);
                newTriangles1.Add(oldToNewVertexMap[index1]);
                newTriangles1.Add(oldToNewVertexMap[index2]);
            }
            else if (!above0 && !above1 && !above2)//下方
            {
                newTriangles2.Add(oldToNewVertexMap[index0]);
                newTriangles2.Add(oldToNewVertexMap[index1]);
                newTriangles2.Add(oldToNewVertexMap[index2]);
            }
            else
            {
                //重新归序
                // Split triangle
                int top, cw, ccw;

                if (above1 == above2 && above0 != above1)
                {
                    top = index0;
                    cw = index1;
                    ccw = index2;
                }
                else if (above2 == above0 && above1 != above2)
                {
                    top = index1;
                    cw = index2;
                    ccw = index0;
                }
                else
                {
                    top = index2;
                    cw = index0;
                    ccw = index1;
                }
                 
                Vector3 cwIntersection, ccwIntersection, cwNormal, ccwNormal;

                

                if (vertexAbovePlane[top])
                {
                    SplitTriangle(localPoint, localSplitPlaneNormal, vertices, normals, top, cw, ccw, out cwIntersection, out ccwIntersection, out cwNormal, out ccwNormal);
                    
                    //表面缝合
                    //part1加1个三角形
                    newTriangles1.Add(oldToNewVertexMap[top]);
                    newTriangles1.Add(newVertices1.Count);
                    newTriangles1.Add(newVertices1.Count + 1);

                    //part2加2个三角形
                    newTriangles2.Add(newVertices2.Count + 1);//cwIntersection
                    newTriangles2.Add(oldToNewVertexMap[cw]);
                    newTriangles2.Add(oldToNewVertexMap[ccw]);

                    
                    newTriangles2.Add(oldToNewVertexMap[ccw]);
                    newTriangles2.Add(newVertices2.Count);//ccwIntersection
                    newTriangles2.Add(newVertices2.Count + 1);// cwIntersection


                    cutPointsIndex.Add(newVertices1.Count);
                    cutPointsIndex.Add(newVertices1.Count + 1);
                    
                    newVertices1.Add(cwIntersection);
                    newVertices1.Add(ccwIntersection);
                    newNormals1.Add(cwNormal);
                    newNormals1.Add(ccwNormal);

                    newVertices2.Add(ccwIntersection);
                    newVertices2.Add(cwIntersection);
                    newNormals2.Add(ccwNormal);
                    newNormals2.Add(cwNormal);

                    //记录切点（T）
                    //VerticesIntersection1.Add(newVertices1.Count);
                    //VerticesIntersection1.Add(newVertices1.Count+1);
                    //VerticesIntersection2.Add(newVertices2.Count);
                    //VerticesIntersection2.Add(newVertices2.Count + 1);
                    //Debug.Log("总1："+newVertices1.Count);
                    //Debug.Log("总2："+newVertices2.Count);
                    
                    VerticesIntersection1.Add(newVertices1.Count - 2);//在newvertices中的索引
                    VerticesIntersection1.Add(newVertices1.Count - 1);
                    VerticesIntersection2.Add(newVertices2.Count - 2);
                    VerticesIntersection2.Add(newVertices2.Count - 1);
                    Debug.Log(newVertices1[VerticesIntersection1.Count - 2]);
                    Debug.Log(newVertices1[VerticesIntersection1.Count - 1]);
                    Debug.Log("ok1");
                }
                else
                {
                    SplitTriangle(localPoint, localSplitPlaneNormal, vertices, normals, top, cw, ccw, out cwIntersection, out ccwIntersection, out cwNormal, out ccwNormal);
                    //记录切割点
                    

                    newTriangles1.Add(newVertices1.Count + 1);//cwIntersection
                    newTriangles1.Add(oldToNewVertexMap[cw]);
                    newTriangles1.Add(oldToNewVertexMap[ccw]);

                    newTriangles1.Add(oldToNewVertexMap[ccw]);
                    newTriangles1.Add(newVertices1.Count);//ccwIntersection
                    newTriangles1.Add(newVertices1.Count + 1);// cwIntersection

                    newTriangles2.Add(oldToNewVertexMap[top]);
                    newTriangles2.Add(newVertices2.Count);
                    newTriangles2.Add(newVertices2.Count + 1);

                    cutPointsIndex.Add(newVertices1.Count);
                    cutPointsIndex.Add(newVertices1.Count + 1);

                    newVertices1.Add(ccwIntersection);
                    newVertices1.Add(cwIntersection);
                    newNormals1.Add(ccwNormal);
                    newNormals1.Add(cwNormal);

                    newVertices2.Add(cwIntersection);
                    newVertices2.Add(ccwIntersection);
                    newNormals2.Add(cwNormal);
                    newNormals2.Add(ccwNormal);

                    //记录切点
                    //记录切点（T）
                    //VerticesIntersection1.Add(newVertices1.Count);
                    //VerticesIntersection1.Add(newTriangles1.Count + 1);
                    //VerticesIntersection2.Add(newVertices2.Count);
                    //VerticesIntersection2.Add(newTriangles2.Count + 1);
                    //Debug.Log("总1：" + newVertices1.Count);
                    //Debug.Log("总2：" + newVertices2.Count);
                    
                    VerticesIntersection1.Add(newVertices1.Count - 2);//在newvertices中的索引
                    VerticesIntersection1.Add(newVertices1.Count - 1);
                    VerticesIntersection2.Add(newVertices2.Count - 2);
                    VerticesIntersection2.Add(newVertices2.Count - 1);
                    Debug.Log(newVertices1[VerticesIntersection1.Count - 2]);
                    Debug.Log(newVertices1[VerticesIntersection1.Count - 1]);
                    Debug.Log("ok2");
                }

                //(temp)cross_vertices赋值 
                //Cross_vertices.Add(cwIntersection);
                //Cross_vertices.Add(ccwIntersection);
            }
        }
        //WriteIntoTxt(newVertices1, newTriangles1, newVertices2, newTriangles2, Cross_vertices);
        ///<summary> 用于平面切割</summary>
        ///
        

        beCutThing.SetActive(false);
        GameObject part1 = new GameObject(beCutThing.name + "(1)");
        GameObject part2 = new GameObject(beCutThing.name + "(2)");
        part1.AddComponent<MeshFilter>();
        part1.AddComponent<MeshRenderer>();
        part2.AddComponent<MeshFilter>();
        part2.AddComponent<MeshRenderer>();

        part1.GetComponent<MeshFilter>().mesh.vertices = newVertices1.ToArray();
        part1.GetComponent<MeshFilter>().mesh.normals = newNormals1.ToArray();
        part1.GetComponent<MeshFilter>().mesh.triangles = newTriangles1.ToArray();

        part2.GetComponent<MeshFilter>().mesh.vertices = newVertices2.ToArray();
        part2.GetComponent<MeshFilter>().mesh.normals = newNormals2.ToArray();
        part2.GetComponent<MeshFilter>().mesh.triangles = newTriangles2.ToArray();

        //切面缝合
        Debug.Log("切面缝合");
        //Suture suture = new Suture();
        int startTime = System.Environment.TickCount;
        Debug.Log("缝合前三角面数量" + part1.GetComponent<MeshFilter>().mesh.triangles.Length);
        Isolate.IsolateMesh(part1);
        Isolate.IsolateMesh(part2);
        //Suture.CutHull_Suture(ref part1, VerticesIntersection1,  newVertices1);//该切割方法存在问题 暂不使用
        //Suture.CutHull_Suture(ref part2, VerticesIntersection2,  newVertices2);
        Debug.Log("缝合后三角面数量" + part1.GetComponent<MeshFilter>().mesh.triangles.Length);
        Debug.Log("缝合花费时间："+(System.Environment.TickCount - startTime).ToString());
        part1.GetComponent<MeshRenderer>().material = beCutThing.GetComponent<MeshRenderer>().material;
        part2.GetComponent<MeshRenderer>().material = beCutThing.GetComponent<MeshRenderer>().material;

        part1.AddComponent<MeshCollider>();
        part1.GetComponent<MeshCollider>().convex = true;
        part1.GetComponent<MeshCollider>().cookingOptions = MeshColliderCookingOptions.InflateConvexMesh;
        part1.AddComponent<Interactable>();
        part1.AddComponent<Choosable>();
        part2.AddComponent<MeshCollider>();
        part2.GetComponent<MeshCollider>().convex = true;
        part2.GetComponent<MeshCollider>().cookingOptions = MeshColliderCookingOptions.InflateConvexMesh;
        part2.AddComponent<Interactable>();
        part2.AddComponent<Choosable>();

        part1.transform.position = beCutThing.transform.position + cutPlane.normal * 0.01f;
        part2.transform.position = beCutThing.transform.position - cutPlane.normal * 0.01f;
        part1.transform.localScale = beCutThing.transform.localScale;
        part2.transform.localScale = beCutThing.transform.localScale;
        part1.transform.rotation = beCutThing.transform.rotation;
        part2.transform.rotation = beCutThing.transform.rotation;
        //Suture s = new Suture();
        //s.CutHull_Suture(part1,VerticesIntersection1,ref newVertices1 );
        //s.CutHull_Suture(part2,VerticesIntersection2,ref newVertices2 );
        part1.tag = "Objects";
        part2.tag = "Objects";

        Debug.Log("总耗时:" + (System.Environment.TickCount - start).ToString());
    }


    /*
     * 三角形切割原理？
     * 返回值：交点 交点法向量
     */
    protected void SplitTriangle(Vector3 pointOnPlane, Vector3 planeNormal, Vector3[] vertices, Vector3[] normals, int top, int cw, int ccw, out Vector3 cwIntersection, out Vector3 ccwIntersection, out Vector3 cwNormal, out Vector3 ccwNormal)
    {

        Vector3 v0 = vertices[top];
        Vector3 v1 = vertices[cw];
        Vector3 v2 = vertices[ccw];
        
        // Intersect the top-cw edge with the plane
        float cwDenominator = Vector3.Dot(v1 - v0, planeNormal);
        float cwScalar = Mathf.Clamp01(Vector3.Dot(pointOnPlane - v0, planeNormal) / cwDenominator);

        // Intersect the top-ccw edge with the plane
        float ccwDenominator = Vector3.Dot(v2 - v0, planeNormal);
        float ccwScalar = Mathf.Clamp01(Vector3.Dot(pointOnPlane - v0, planeNormal) / ccwDenominator);//ccw标量

        // Interpolate vertex positions                               计算新点的位置
        Vector3 cwVertex = new Vector3();

        cwVertex.x = v0.x + (v1.x - v0.x) * cwScalar;
        cwVertex.y = v0.y + (v1.y - v0.y) * cwScalar;
        cwVertex.z = v0.z + (v1.z - v0.z) * cwScalar;

        Vector3 ccwVertex = new Vector3();

        ccwVertex.x = v0.x + (v2.x - v0.x) * ccwScalar;
        ccwVertex.y = v0.y + (v2.y - v0.y) * ccwScalar;
        ccwVertex.z = v0.z + (v2.z - v0.z) * ccwScalar;

        // Interpolate normals

        Vector3 n0 = normals[top];
        Vector3 n1 = normals[cw];
        Vector3 n2 = normals[ccw];

        Vector3 cwNormalx = new Vector3();              //计算新点的法线

        cwNormalx.x = n0.x + (n1.x - n0.x) * cwScalar;
        cwNormalx.y = n0.y + (n1.y - n0.y) * cwScalar;
        cwNormalx.z = n0.z + (n1.z - n0.z) * cwScalar;

        cwNormalx.Normalize();

        Vector3 ccwNormalx = new Vector3();

        ccwNormalx.x = n0.x + (n2.x - n0.x) * ccwScalar;
        ccwNormalx.y = n0.y + (n2.y - n0.y) * ccwScalar;
        ccwNormalx.z = n0.z + (n2.z - n0.z) * ccwScalar;

        ccwNormalx.Normalize();
        cwNormal = cwNormalx;
        ccwNormal = ccwNormalx;


        cwIntersection = cwVertex;
        ccwIntersection = ccwVertex;
    }



    private void WriteIntoTxt(List<Vector3> one,List<int> triangle1, List<Vector3> two,List<int> triangle2,List<Vector3> cross)//数据写入TXT文档
    {
        string path = @"D:\\Cutting_result.txt";
        //创建StreamWriter 类的实例
        StreamWriter streamWriter = new StreamWriter(path);
        //向文件中写入数据
        Debug.Log("1");
        streamWriter.WriteLine("物体1的坐标点");
        for(int i=0;i<one.Count;i++)
        {
            streamWriter.WriteLine(one[i].x+" "+one[i].y+" "+one[i].z);
        }
        Debug.Log("2");
        streamWriter.WriteLine("物体1的三角关系");
        for (int i = 0; i < triangle1.Count; i++)
        {
            streamWriter.Write(triangle1[i]);
            streamWriter.WriteLine("");
        }
        Debug.Log("3");
        streamWriter.WriteLine("物体2的坐标点");
        for (int i = 0; i < two.Count; i++)
        {
            streamWriter.WriteLine(two[i].x + " " + two[i].y + " " + two[i].z);
        }
        Debug.Log("4");
        streamWriter.WriteLine("物体2的三角关系");
        for (int i = 0; i < triangle2.Count; i++)
        {
            streamWriter.Write(triangle2[i]);
            streamWriter.WriteLine("");
        }
        Debug.Log("5");
        streamWriter.WriteLine("横截面的坐标点");
        for (int i = 0; i < cross.Count; i++)
        {
            streamWriter.WriteLine(cross[i].x + " " + cross[i].y + " " + cross[i].z);
        }
        
        //刷新缓存
        streamWriter.Flush();
        //关闭流
        streamWriter.Close();
    }

}
