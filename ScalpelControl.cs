using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;
/// <summary>
/// 实现
/// </summary>
public class ScalpelControl : MonoBehaviour
{

    // Use this for initialization
    GameObject beCutThing;
    GameObject cutHelper;

    static bool cutModel = true;
    //Plane[] planes = new Plane[10];
    //static int planenum = 0;
    List<Vector3> CutPlanePosition = new List<Vector3>(4);
    //List<Vector3> cutPlanePoints = new List<Vector3>(4);

    static bool Cut_state = false;
    int n;
    //Quaternion fixedRotation;
    Vector3 fixedRotation;

    Plane cutPlane;
    Plane move_Helper;

    //提示
    public GameObject tips;
    //List<Vector3> cutHullmark = null;
    //Mesh mesh_mark1;
    //Mesh mesh_mark2;
    //用于记录normal
    List<Vector3> temp_Nor_List = new List<Vector3>();

    //画线标记
    //private LineRenderer[] line = new LineRenderer[2];
    private LineRenderer[] line;
    public Material material;
    public static Material Transparent;
    void Start()
    {
        /*
        //碰撞过滤 2020.7.25
        GameObject Father = GameObject.Find("Player").transform.GetChild(6).gameObject;
        Debug.Log("Father");
        for (int i = 0; i < 2; i++)
        {
            GameObject temp = Father.transform.GetChild(i).gameObject;
            n = temp.transform.childCount;
            //Debug.Log(n);
            for (int j = 0; j < n; j++)
            {
                //Debug.Log(j); 
                Physics.IgnoreCollision(temp.transform.GetChild(j).GetComponent<SphereCollider>(),
                    GetComponent<MeshCollider>());
            }
        }
        Father = GameObject.Find("Player").transform.GetChild(7).gameObject;
        Debug.Log("Father");
        for (int i = 0; i < 2; i++)
        {
            GameObject temp = Father.transform.GetChild(i).gameObject;
            int n = temp.transform.childCount;
            //Debug.Log(n);
            for (int j = 0; j < n; j++)
            {
                //Debug.Log(j);
                Physics.IgnoreCollision(temp.transform.GetChild(j).GetComponent<SphereCollider>(),
                    GetComponent<MeshCollider>());
            }
        }
        */
    }
    void Update()
    {
        //    if (SteamVR_Actions.default_GrabPinch.GetStateDown(SteamVR_Input_Sources.Any))
        //    {
        //        Debug.Log("default_GrabPinch.GetStateDown");
        //    }
        //    if (SteamVR_Actions.default_Teleport.state)
        //    {
        //        Debug.Log("default_Teleport.state");
        //    }

        if (SteamVR_Actions.default_Teleport.GetStateDown(SteamVR_Input_Sources.Any)
            && SteamVR_Actions.default_GrabPinch.state
             && transform.GetComponent<Choosable>().isChosen)
        {
            Debug.Log("双按");
        }
        if (SteamVR_Actions.default_Teleport.GetStateUp(SteamVR_Input_Sources.Any)
            && SteamVR_Actions.default_GrabPinch.state
             && transform.GetComponent<Choosable>().isChosen)
        {
            Debug.Log("取消双按");
        }
        if (SteamVR_Actions.default_GrabPinch.state
             && transform.GetComponent<Choosable>().isChosen
             && SteamVR_Actions.default_GrabGrip.GetStateDown(SteamVR_Input_Sources.Any))
        {
            Debug.Log("手柄切换键按下");
        }
        //第一次按键 切割准备 切割平面+两个局部切割点
        if (SteamVR_Actions.default_Teleport.GetStateDown(SteamVR_Input_Sources.Any)
    && SteamVR_Actions.default_GrabPinch.state
    && transform.GetComponent<Choosable>().isChosen)//手术刀是否被抓取
        {
            //CutPlanePosition.Clear();//清理遗留数据
            Debug.Log("111");
            if (!Cut_state)
            {
                //清除LineRenderer数据
                line = new LineRenderer[2];

                //固定手术刀移动方向
                //transform.GetComponent<Rigidbody>().freezeRotation = true;
                //fixedRotation = Quaternion.Euler(transform.TransformDirection(transform.rotation.eulerAngles));
                //fixedRotation = transform.TransformDirection(transform.rotation.eulerAngles);
                move_Helper = new Plane(transform.TransformVector(new Vector3(0, 0, 1)), transform.position);

                Cut_state = true;
                //cutHelper = transform.GetChild(0).gameObject;//有父节点 坐标为局部坐标
                //Debug.Log("cutHelper: " + cutHelper.name);
                //Debug.Log("cutHelper世界坐标位置: " + transform.TransformPoint(cutHelper.transform.position));
                //Debug.Log("cutHelper世界坐标位置: " + transform.TransformPoint(transform.TransformPoint(cutHelper.transform.position)));
                //Vector3[] cutHelper_vertices = cutHelper.GetComponent<MeshFilter>().sharedMesh.vertices;
                //创建切割平面
                //cutPlane = new Plane(cutHelper.transform.TransformPoint(cutHelper_vertices[0]),
                //    cutHelper.transform.TransformPoint(cutHelper_vertices[1]),
                //    cutHelper.transform.TransformPoint(cutHelper_vertices[cutHelper_vertices.Length - 1]));
                cutPlane = new Plane(transform.TransformPoint(transform.GetComponent<MeshFilter>().sharedMesh.vertices[761]),
                    transform.TransformPoint(transform.GetComponent<MeshFilter>().sharedMesh.vertices[728]),
                    transform.TransformPoint(transform.GetComponent<MeshFilter>().sharedMesh.vertices[767]));
                //cutPlane = new Plane(cutHelper_vertices[0],cutHelper_vertices[1],cutHelper_vertices[cutHelper_vertices.Length - 1]);

                //Debug.Log("===========");
                //Debug.Log("父： " + transform.position);
                //Debug.Log("总数： " + cutHelper_vertices.Length);
                //Debug.Log("cutHelper_vertices[0]  世界：  " + cutHelper.transform.TransformPoint(cutHelper_vertices[0]) + "局部:  " + cutHelper_vertices[0]);
                //Debug.Log("cutHelper_vertices[10]  世界：  " + cutHelper.transform.TransformPoint(cutHelper_vertices[10]) + "局部:  " + cutHelper_vertices[0]);
                //Debug.Log("cutHelper_vertices[110]  世界：  " + cutHelper.transform.TransformPoint(cutHelper_vertices[110]) + "局部:  " + cutHelper_vertices[0]);
                //Debug.Log("cutHelper_vertices[120]  世界：  " + cutHelper.transform.TransformPoint(cutHelper_vertices[120]) + "局部:  " + cutHelper_vertices[0]);
                //Debug.Log("===========");
                //CutPlanePosition.Add(cutHelper.transform.TransformPoint(cutHelper_vertices[0]));//world
                //CutPlanePosition.Add(cutHelper.transform.TransformPoint(cutHelper_vertices[120]));
                CutPlanePosition.Add(transform.TransformPoint(transform.GetComponent<MeshFilter>().sharedMesh.vertices[728]));//world
                CutPlanePosition.Add(transform.TransformPoint(transform.GetComponent<MeshFilter>().sharedMesh.vertices[767]));
                //Debug.Log("===========");
                //Debug.Log("=====判断第一次的切割直线直线是否在平面cutPlane上======");
                //Vector3 vector3 = CutPlanePosition[CutPlanePosition.Count - 1] - CutPlanePosition[CutPlanePosition.Count - 2];
                //bool res = Vector3.Dot(vector3, cutPlane.normal) == 0 ? true : false;
                //if (!res)
                //    Debug.Log("Vector3.Dot(vector3, cutPlane.normal): " + Vector3.Dot(vector3, cutPlane.normal));
                //Debug.Log("结果： " + res);
                //Debug.Log("=====================================================");
                //Debug.Log("===========");
                //Debug.Log("总数1: " + CutPlanePosition.Count);
                //Debug.Log("CutPlanePosition[" + (CutPlanePosition.Count - 2) + "]: " + CutPlanePosition[CutPlanePosition.Count - 2]);
                //Debug.Log("CutPlanePosition[" + (CutPlanePosition.Count - 1) + "]: " + CutPlanePosition[CutPlanePosition.Count - 1]);
                //Debug.Log("===========");
                //CutPlanePosition.Add(cutHelper_vertices[0]);
                //CutPlanePosition.Add(cutHelper_vertices[120]);
                //Vector3[] v = new Vector3[4];
                //记录两个局部切割点
                //Debug.Log("cutHelper_vertices.Length:  " + cutHelper_vertices.Length);
                //float q1 = (cutHelper_vertices[0] - cutHelper_vertices[10]).sqrMagnitude;
                //Debug.Log("q1: " + q1);
                //float q2 = (cutHelper_vertices[0] - cutHelper_vertices[110]).sqrMagnitude;
                //Debug.Log("q2: " + q2);
                //if (q1 > q2)
                //{
                //    //CutPlanePosition.Add(cutHelper.transform.TransformPoint((cutHelper_vertices[0] + cutHelper_vertices[110]) / 2));
                //    //CutPlanePosition.Add(cutHelper.transform.TransformPoint((cutHelper_vertices[10] + cutHelper_vertices[120]) / 2));
                //    CutPlanePosition.Add(cutHelper.transform.TransformPoint(cutHelper_vertices[0]));//0 + 120,100
                //    CutPlanePosition.Add(cutHelper.transform.TransformPoint(cutHelper_vertices[120]));
                //}
                //else
                //{
                //    //CutPlanePosition.Add(cutHelper.transform.TransformPoint((cutHelper_vertices[0] + cutHelper_vertices[120]) / 2));
                //    //CutPlanePosition.Add(cutHelper.transform.TransformPoint((cutHelper_vertices[10] + cutHelper_vertices[110]) / 2));
                //    CutPlanePosition.Add(cutHelper.transform.TransformPoint(cutHelper_vertices[10]));//
                //    CutPlanePosition.Add(cutHelper.transform.TransformPoint(cutHelper_vertices[110]));
                //}
                //CutPlanePosition.Add(cutHelper.transform.TransformPoint(cutHelper_vertices[10]));//
                //CutPlanePosition.Add(cutHelper.transform.TransformPoint(cutHelper_vertices[110]));
                //Debug.Log("记录下刀位置");

                //Cut_state = true;
            }
        }

        //else if (SteamVR_Actions.default_Teleport.GetStateUp(SteamVR_Input_Sources.Any)
        if (SteamVR_Actions.default_Teleport.GetStateUp(SteamVR_Input_Sources.Any)//松开圆盘键
             && SteamVR_Actions.default_GrabPinch.state
             && transform.GetComponent<Choosable>().isChosen)
        {
            //Debug.Log("222");
            if (Cut_state)
            {
                Debug.Log("Lets go to cut");
                Cut_state = false;
                //第二次按键 切割开始 两个局部切割点->构成封闭局部四边形区域
                //cutHelper = transform.GetChild(0).gameObject;
                //Vector3[] cutHelper_vertices = cutHelper.GetComponent<MeshFilter>().mesh.vertices;
                //Debug.Log("cutHelper_vertices[0]  世界：  " + cutHelper.transform.TransformPoint(cutHelper_vertices[0]) 
                //    + "局部:  " + cutHelper_vertices[0]);
                //Debug.Log("cutHelper_vertices[120]  世界：  " + cutHelper.transform.TransformPoint(cutHelper_vertices[120]) 
                //    + "局部:  " + cutHelper_vertices[120]);
                //CutPlanePosition.Add(cutHelper.transform.TransformPoint(cutHelper.GetComponent<MeshFilter>().sharedMesh.vertices[0]));
                //CutPlanePosition.Add(cutHelper.transform.TransformPoint(cutHelper.GetComponent<MeshFilter>().sharedMesh.vertices[120]));

                //此处 后获取的两个点不在之前创造的平面上 所以须要做一次投影 
                Vector3 temp = cutPlane.ClosestPointOnPlane(transform.TransformPoint(transform.GetComponent<MeshFilter>().sharedMesh.vertices[767]));
                CutPlanePosition.Add(temp);
                temp = cutPlane.ClosestPointOnPlane(transform.TransformPoint(transform.GetComponent<MeshFilter>().sharedMesh.vertices[728]));
                CutPlanePosition.Add(temp);
                //Debug.Log("===========");
                //Debug.Log("总数2: " + CutPlanePosition.Count);
                //Debug.Log("CutPlanePosition[" + (CutPlanePosition.Count - 2) + "]: " + CutPlanePosition[CutPlanePosition.Count - 2]);
                //Debug.Log("CutPlanePosition[" + (CutPlanePosition.Count - 1) + "]: " + CutPlanePosition[CutPlanePosition.Count - 1]);
                //Debug.Log("===========");
                //Debug.Log("=====判断最后的切割直线直线是否在平面cutPlane上======");
                //Vector3 vector3 = CutPlanePosition[CutPlanePosition.Count - 1] - CutPlanePosition[CutPlanePosition.Count - 2];
                //bool res = Vector3.Dot(vector3, cutPlane.normal) == 0 ? true : false;
                //if (!res)
                //    Debug.Log("Vector3.Dot(vector3, cutPlane.normal): " + Vector3.Dot(vector3, cutPlane.normal));
                //Debug.Log("结果： " + res);
                //Debug.Log("=====================================================");



                //CutPlanePosition.Add(cutHelper_vertices[0]);
                //CutPlanePosition.Add(cutHelper_vertices[120]);
                //记录两个局部切割点
                //float q1 = (cutHelper_vertices[0] - cutHelper_vertices[10]).sqrMagnitude;
                //float q2 = (cutHelper_vertices[0] - cutHelper_vertices[110]).sqrMagnitude;                
                //float q1 = (cutHelper_vertices[0] - cutHelper_vertices[10]).sqrMagnitude;
                //float q2 = (cutHelper_vertices[0] - cutHelper_vertices[110]).sqrMagnitude;
                //Debug.Log("q1: " + q1 + "  q2: " + q2);
                //if (q1 > q2)
                //{
                //    CutPlanePosition.Add(cutHelper.transform.TransformPoint((cutHelper_vertices[0] + cutHelper_vertices[110]) / 2));
                //    CutPlanePosition.Add(cutHelper.transform.TransformPoint((cutHelper_vertices[10] + cutHelper_vertices[120]) / 2));
                //}
                //else
                //{
                //    CutPlanePosition.Add(cutHelper.transform.TransformPoint((cutHelper_vertices[0] + cutHelper_vertices[120]) / 2));
                //    CutPlanePosition.Add(cutHelper.transform.TransformPoint((cutHelper_vertices[10] + cutHelper_vertices[110]) / 2));
                //}
                //if (q1 > q2)
                //{
                //    //CutPlanePosition.Add(cutHelper.transform.TransformPoint((cutHelper_vertices[0] + cutHelper_vertices[110]) / 2));
                //    //CutPlanePosition.Add(cutHelper.transform.TransformPoint((cutHelper_vertices[10] + cutHelper_vertices[120]) / 2));
                //    CutPlanePosition.Add(cutHelper.transform.TransformPoint(cutHelper_vertices[0]));//0 + 120,100
                //    CutPlanePosition.Add(cutHelper.transform.TransformPoint(cutHelper_vertices[120]));
                //}
                //else
                //{
                //    //CutPlanePosition.Add(cutHelper.transform.TransformPoint((cutHelper_vertices[0] + cutHelper_vertices[120]) / 2));
                //    //CutPlanePosition.Add(cutHelper.transform.TransformPoint((cutHelper_vertices[10] + cutHelper_vertices[110]) / 2));
                //    CutPlanePosition.Add(cutHelper.transform.TransformPoint(cutHelper_vertices[10]));//
                //    CutPlanePosition.Add(cutHelper.transform.TransformPoint(cutHelper_vertices[110]));
                //}
                Debug.Log("记录第二次下刀位置");

                //Debug.Log("=========");
                //for(int i=0;i<CutPlanePosition.Count;i++)
                //{
                //    Debug.Log("CutPlanePosition[" + i + "]:" + CutPlanePosition[i]);
                //}
                //Debug.Log("=========");
                //取消手术刀方向限制
                //transform.GetComponent<Rigidbody>().freezeRotation = false;
                Debug.Log("开始切割");
                beCutThing = GameObject.FindWithTag("ChosenObject");
                //if (beCutThing.GetComponent<LineRenderer>() == null)
                //    beCutThing.AddComponent<LineRenderer>();
                if (cutModel)
                {
                    Debug.Log("执行局部切割");
                    Cut();
                }
                else
                {
                    Debug.Log("执行平面切割");
                    WholeCut();
                }
                //Cut();
                //WholeCut();
                Debug.Log("切割完成");
            }
        }
        //else
        //{
        //    Debug.Log("第二次判断中断");
        //}
        //位置控制 
        if (SteamVR_Actions.default_Teleport.state
            && SteamVR_Actions.default_GrabPinch.state
            && transform.GetComponent<Choosable>().isChosen)
        {
            //transform.position = cutPlane.ClosestPointOnPlane(transform.position);//投影向向上移动
            //transform.rotation = Quaternion.Euler(transform.parent.InverseTransformDirection(fixedRotation.eulerAngles));//固定旋转角度
            transform.position = cutPlane.ClosestPointOnPlane(transform.position);
            //transform.rotation = Quaternion.Euler(transform.parent.parent.transform.InverseTransformVector(fixedRotation));
        }
        //提示框操作
        if (SteamVR_Actions.default_GrabPinch.state
             && transform.GetComponent<Choosable>().isChosen
             && SteamVR_Actions.default_GrabGrip.GetStateDown(SteamVR_Input_Sources.Any))
        {
            //Debug.Log("手柄切换键按下");
            cutModel = !cutModel;
            if (cutModel)
            {
                tips.GetComponent<TextMesh>().text = "当前模式 : 局部切割";
                tips.SetActive(true);
            }
            else
            {
                tips.GetComponent<TextMesh>().text = "当前模式 : 平面切割";
            }
        }
    }



    private void Cut()
    {
        //Debug.Log("=========");
        //Debug.Log(beCutThing);
        //Debug.Log(beCutThing.GetType());
        //Debug.Log("=========");
        //记录当前切割物体 用于回退 2020.08.12
        RollBackHelper.pre = beCutThing;

        int startTime = System.Environment.TickCount;
        //根据本切割工具网格顶点形成切割面
        //Vector3[] v = transform.GetComponent<MeshFilter>().mesh.vertices;

        //Plane cutPlane = new Plane(transform.TransformPoint(v[0]), transform.TransformPoint(v[1]), transform.TransformPoint(v[v.Length - 1]));


        //Plane cutPlane = new Plane(cutPlanePoints[0], cutPlanePoints[1], cutPlanePoints[3]);


        //Debug.Log("平面向量"+cutPlane.normal);
        //Vector3[] cutPlanePoints = new Vector3[4];
        //cutPlanePoints[0] = transform.TransformPoint(v[10]);//切割平面4个顶点
        //cutPlanePoints[1] = transform.TransformPoint(v[0]);
        //cutPlanePoints[2] = transform.TransformPoint(v[110]);
        //cutPlanePoints[3] = transform.TransformPoint(v[120]);


        //把切割面转化为被切割物体的当地坐标 (cutPlane.normal * -cutPlane.distance)表示？
        Vector3 localPoint = beCutThing.transform.InverseTransformPoint(cutPlane.normal * -cutPlane.distance);
        //Debug.Log(localPoint);
        Vector3 localSplitPlaneNormal = beCutThing.transform.InverseTransformDirection(cutPlane.normal);
        localSplitPlaneNormal.Normalize();//生成法向量
        List<Vector3> cutPlanePoints = new List<Vector3>();
        cutPlanePoints.Add(beCutThing.transform.InverseTransformPoint(CutPlanePosition[0]));
        cutPlanePoints.Add(beCutThing.transform.InverseTransformPoint(CutPlanePosition[1]));
        cutPlanePoints.Add(beCutThing.transform.InverseTransformPoint(CutPlanePosition[2]));
        cutPlanePoints.Add(beCutThing.transform.InverseTransformPoint(CutPlanePosition[3]));

        //测试赋值
        //cutPlanePoints.Add(new Vector3(-0.3f, 0.3f, 0.5f));
        //cutPlanePoints.Add(new Vector3(-0.3f, 0.3f, 0.5f));
        //cutPlanePoints.Add(new Vector3(-0.4f, 0.3f, 0.5f));
        //cutPlanePoints.Add(new Vector3(-0.4f, 0.3f, 0.5f));

        //Debug.Log("====");
        //for (int i = 0; i < 4; i++)
        //{
        //    Debug.Log("cutPlanePoints[" + i + "]: " + cutPlanePoints[i]);
        //}
        //Debug.Log("====");

        //根据切割面，把顶点分成两部分
        List<Vector3> CutAnchorPoints = new List<Vector3>(cutPlanePoints);//存储切割角点到CutPosition
        CutPosition.cutAnchorPoints.Add(CutAnchorPoints);//记录切割角点
        Debug.Log("测试：cutPlanePoints=" + cutPlanePoints.Count);
        Debug.Log("测试：CutPosition.cutAnchorPoints[0]=" + CutPosition.cutAnchorPoints[0].Count);
        //Debug.Log("-----=" + CutPosition.cutAnchorPoints[0][0]);

        Vector3[] vertices = beCutThing.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] normals = beCutThing.GetComponent<MeshFilter>().mesh.normals;
        bool[] vertexAbovePlane;
        vertexAbovePlane = new bool[vertices.Length];
        //int[] oldToNewVertexMap = new int[vertices.Length];

        List<Vector3> newVertices1 = new List<Vector3>(vertices);//为了避免数组多次动态增加容量，给最大值
        //List<Vector3> newVertices2 = new List<Vector3>(vertices.Length);
        List<Vector3> newNormals1 = new List<Vector3>(normals);
        //List<Vector3> newNormals2 = new List<Vector3>(normals.Length);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            bool abovePlane = Vector3.Dot(vertex - localPoint, localSplitPlaneNormal) >= 0.0f;
            //Debug.Log("abovePlane" + i + ": " + abovePlane);

            vertexAbovePlane[i] = abovePlane;

        }
        //if (newVertices1.Count == 0 || newVertices2.Count == 0)
        //{
        //    Debug.Log("nothing be cut,over");
        //    return;
        //}
        //分三角面

        int[] indices = beCutThing.GetComponent<MeshFilter>().mesh.triangles;
        int triangleCount = indices.Length / 3;
        List<int> newTriangles1 = new List<int>(indices.Length);
        //List<int> newTriangles2 = new List<int>(indices.Length);
        List<Vector3> cutEdges1 = new List<Vector3>();
        List<Vector3> cutEdges2 = new List<Vector3>();
        for (int i = 0; i < triangleCount; i++) // 遍历每一个三角面
        {
            int index0 = indices[i * 3 + 0];
            int index1 = indices[i * 3 + 1];
            int index2 = indices[i * 3 + 2];

            bool above0 = vertexAbovePlane[index0];
            bool above1 = vertexAbovePlane[index1];
            bool above2 = vertexAbovePlane[index2];

            if (above0 && above1 && above2)
            {
                //Debug.Log("同向 不切割");
                newTriangles1.Add(index0);
                newTriangles1.Add(index1);
                newTriangles1.Add(index2);
            }
            else if (!above0 && !above1 && !above2)
            {
                //Debug.Log("同向 不切割");
                newTriangles1.Add(index0);
                newTriangles1.Add(index1);
                newTriangles1.Add(index2);
            }
            else
            {
                //Debug.Log("不同向 需要切割");
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
                //Debug.Log("第" + i + "三角形" + newVertices1[top].x + " " + newVertices1[top].y + " " + newVertices1[top].z + " " + newVertices1[cw].x +" "+ newVertices1[cw].y +" "+ newVertices1[cw].z + " " + newVertices1[ccw].x +" "+ newVertices1[ccw].y+" " + newVertices1[ccw].z);
                if (vertexAbovePlane[top])
                {
                    SplitTriangletop1(localPoint, localSplitPlaneNormal, vertices, normals, top, cw, ccw, out cwIntersection, out ccwIntersection, out cwNormal, out ccwNormal);
                    //newTriangles1.Add(top);
                    //newTriangles1.Add(newVertices1.Count);
                    //newTriangles1.Add(newVertices1.Count + 1);

                    //newTriangles2.Add(newVertices2.Count + 1);//cwIntersection
                    //newTriangles2.Add(cw);
                    //newTriangles2.Add(ccw);

                    //newTriangles2.Add(ccw);
                    //newTriangles2.Add(newVertices2.Count);//ccwIntersection
                    //newTriangles2.Add(newVertices2.Count + 1);// cwIntersection




                    //切割点是否在切割四边形内部  否：正常添加该三角面    是：删除该三角面，并添加切割点和切割面，记录切割点
                    Vector3 v0 = Vector3.Cross(cutPlanePoints[0] - cutPlanePoints[3], cwIntersection - cutPlanePoints[3]);
                    Vector3 v1 = Vector3.Cross(cutPlanePoints[1] - cutPlanePoints[0], cwIntersection - cutPlanePoints[0]);
                    Vector3 v2 = Vector3.Cross(cutPlanePoints[2] - cutPlanePoints[1], cwIntersection - cutPlanePoints[1]);
                    Vector3 v3 = Vector3.Cross(cutPlanePoints[3] - cutPlanePoints[2], cwIntersection - cutPlanePoints[2]);
                    if (Vector3.Dot(v0, v1) < 0 || Vector3.Dot(v0, v2) < 0 || Vector3.Dot(v0, v3) < 0)
                    //if (Vector3.Dot(v0, v1) < Min_Judger || Vector3.Dot(v0, v2) < Min_Judger || Vector3.Dot(v0, v3) < Min_Judger)
                    {
                        //Debug.Log("========打印出需要分割但不符合条件的结果的值=============");
                        //Debug.Log("Vector3.Dot(v0, v1)" + Vector3.Dot(v0, v1));
                        //Debug.Log("Vector3.Dot(v0, v2)" + Vector3.Dot(v0, v2));
                        //Debug.Log("Vector3.Dot(v0, v3)" + Vector3.Dot(v0, v3));
                        //Debug.Log("=========================================================");
                        //Debug.Log("不在四边形区域内！！");
                        //该点不在四边形内    
                        //（Temp)(cwIntersection点)-->OK 已解决
                        newTriangles1.Add(index0);//
                        newTriangles1.Add(index1);//
                        newTriangles1.Add(index2);//
                    }
                    else
                    {
                        //Debug.Log("四边形区域内！！");
                        newTriangles1.Add(top);
                        newTriangles1.Add(newVertices1.Count);
                        newTriangles1.Add(newVertices1.Count + 1);

                        newVertices1.Add(cwIntersection);
                        newVertices1.Add(ccwIntersection);
                        newNormals1.Add(cwNormal);
                        newNormals1.Add(ccwNormal);



                        newTriangles1.Add(newVertices1.Count + 1);//cwIntersection
                        newTriangles1.Add(cw);
                        newTriangles1.Add(ccw);

                        newTriangles1.Add(ccw);
                        newTriangles1.Add(newVertices1.Count);//ccwIntersection
                        newTriangles1.Add(newVertices1.Count + 1);// cwIntersection

                        //Vector3 cwIntersectionDiffer = GetDifferentVector3(cwIntersection);
                        //Vector3 ccwIntersectionDiffer = GetDifferentVector3(ccwIntersection);
                        Vector3 cwIntersectionDiffer = GetDifferentVector3one(cwIntersection, vertices[cw]);
                        Vector3 ccwIntersectionDiffer = GetDifferentVector3one(ccwIntersection, vertices[ccw]);

                        newVertices1.Add(ccwIntersectionDiffer);
                        newVertices1.Add(cwIntersectionDiffer);
                        newNormals1.Add(ccwNormal);
                        newNormals1.Add(cwNormal);


                        cutEdges1.Add(cwIntersection);
                        cutEdges1.Add(ccwIntersection);
                        //Debug.Log("top-cw" + vertices[top].x + " " + vertices[top].y + " " + vertices[top].z + " " + vertices[cw].x + " " + vertices[cw].y + " " + vertices[cw].z + "cwIntersection" + cwIntersection.x + " " + cwIntersection.y + " " + cwIntersection.z);
                        //Debug.Log("top-ccw" + vertices[top].x + " " + vertices[top].y + " " + vertices[top].z + " " + vertices[ccw].x + " " + vertices[ccw].y + " " + vertices[ccw].z + "cwIntersection" + ccwIntersection.x + " " + ccwIntersection.y + " " + ccwIntersection.z);
                        //Debug.Log("cwIntersection" + cwIntersection.x + " " + cwIntersection.y + " " + cwIntersection.z + "cwIntersectionDiffer" + cwIntersectionDiffer.x + " " + cwIntersectionDiffer.y + " " + cwIntersectionDiffer.z);
                        //Debug.Log("ccwIntersection" + ccwIntersection.x + " " + ccwIntersection.y + " " + ccwIntersection.z + "ccwIntersectionDiffer" + ccwIntersectionDiffer.x + " " + ccwIntersectionDiffer.y + " " + ccwIntersectionDiffer.z);
                        cutEdges2.Add(cwIntersectionDiffer);
                        cutEdges2.Add(ccwIntersectionDiffer);
                    }
                    //newVertices1.Add(cwIntersection);
                    //newVertices1.Add(ccwIntersection);
                    //newNormals1.Add(cwNormal);
                    //newNormals1.Add(ccwNormal);

                    //newVertices2.Add(ccwIntersection);
                    //newVertices2.Add(cwIntersection);
                    //newNormals2.Add(ccwNormal);
                    //newNormals2.Add(cwNormal);

                }
                else
                {
                    SplitTriangletop0(localPoint, localSplitPlaneNormal, vertices, normals, top, cw, ccw, out cwIntersection, out ccwIntersection, out cwNormal, out ccwNormal);


                    //切割点是否在切割四边形内部  否：正常添加该三角面    是：删除该三角面，并添加切割点和切割面，记录切割点
                    Vector3 v0 = Vector3.Cross(cutPlanePoints[0] - cutPlanePoints[3], cwIntersection - cutPlanePoints[3]);
                    Vector3 v1 = Vector3.Cross(cutPlanePoints[1] - cutPlanePoints[0], cwIntersection - cutPlanePoints[0]);
                    Vector3 v2 = Vector3.Cross(cutPlanePoints[2] - cutPlanePoints[1], cwIntersection - cutPlanePoints[1]);
                    Vector3 v3 = Vector3.Cross(cutPlanePoints[3] - cutPlanePoints[2], cwIntersection - cutPlanePoints[2]);
                    if (Vector3.Dot(v0, v1) < 0 || Vector3.Dot(v0, v2) < 0 || Vector3.Dot(v0, v3) < 0)//即：在平面外不切割
                    //if (Vector3.Dot(v0, v1) < Min_Judger || Vector3.Dot(v0, v2) < Min_Judger || Vector3.Dot(v0, v3) < Min_Judger)//即：在平面外不切割
                    {
                        //Debug.Log("========打印出需要分割但不符合条件的结果的值=============");
                        //Debug.Log("Vector3.Dot(v0, v1)" + Vector3.Dot(v0, v1));
                        //Debug.Log("Vector3.Dot(v0, v2)" + Vector3.Dot(v0, v2));
                        //Debug.Log("Vector3.Dot(v0, v3)" + Vector3.Dot(v0, v3));
                        //Debug.Log("=========================================================");
                        //该点不在四边形内
                        //Debug.Log("不在四边形区域内！！");
                        newTriangles1.Add(index0);
                        newTriangles1.Add(index1);
                        newTriangles1.Add(index2);
                    }
                    else
                    {
                        //if (newVertices1[cw] != cwIntersection ||newVertices1[ccw]!=ccwIntersection)
                        //{
                        //Debug.Log("四边形区域内！！");
                        newTriangles1.Add(newVertices1.Count + 1);//cwIntersection
                        newTriangles1.Add(cw);
                        newTriangles1.Add(ccw);

                        newTriangles1.Add(ccw);
                        newTriangles1.Add(newVertices1.Count);//ccwIntersection
                        newTriangles1.Add(newVertices1.Count + 1);// cwIntersection

                        newVertices1.Add(ccwIntersection);
                        newVertices1.Add(cwIntersection);
                        newNormals1.Add(ccwNormal);
                        newNormals1.Add(cwNormal);

                        //}
                        newTriangles1.Add(top);
                        newTriangles1.Add(newVertices1.Count);
                        newTriangles1.Add(newVertices1.Count + 1);
                        //Vector3 cwIntersectionDiffer = GetDifferentVector3(cwIntersection);
                        //Vector3 ccwIntersectionDiffer = GetDifferentVector3(ccwIntersection);
                        Vector3 cwIntersectionDiffer = GetDifferentVector3one(cwIntersection, vertices[top]);
                        Vector3 ccwIntersectionDiffer = GetDifferentVector3one(ccwIntersection, vertices[top]);

                        newVertices1.Add(cwIntersectionDiffer);
                        newVertices1.Add(ccwIntersectionDiffer);
                        newNormals1.Add(cwNormal);
                        newNormals1.Add(ccwNormal);


                        cutEdges1.Add(ccwIntersection);
                        cutEdges1.Add(cwIntersection);
                        //Debug.Log("top-ccw" + vertices[top].x + " " + vertices[top].y + " " + vertices[top].z + " " + vertices[ccw].x + " " + vertices[ccw].y + " " + vertices[ccw].z + "ccwIntersection" + ccwIntersection.x + " " + ccwIntersection.y + " " + ccwIntersection.z);
                        //Debug.Log("top-cw" + vertices[top].x + " " + vertices[top].y + " " + vertices[top].z + " " + vertices[cw].x + " " + vertices[cw].y + " " + vertices[cw].z + "cwIntersection" + cwIntersection.x + " " + cwIntersection.y + " " + cwIntersection.z);
                        //Debug.Log("ccwIntersection" + ccwIntersection.x + " " + ccwIntersection.y + " " + ccwIntersection.z + "ccwIntersectionDiffer" + ccwIntersectionDiffer.x + " " + ccwIntersectionDiffer.y + " " + ccwIntersectionDiffer.z);
                        //Debug.Log("cwIntersection" + cwIntersection.x + " " + cwIntersection.y + " " + cwIntersection.z + "cwIntersectionDiffer" + cwIntersectionDiffer.x + " " + cwIntersectionDiffer.y + " " + cwIntersectionDiffer.z);
                        cutEdges2.Add(ccwIntersectionDiffer);
                        cutEdges2.Add(cwIntersectionDiffer);
                    }

                }

            }
        }
        Debug.Log("cutEdges1.Count: " + cutEdges1.Count);
        Debug.Log("cutEdges2.Count: " + cutEdges2.Count);


        /*****************半圆缝合开始*********************/


        //第一个面
        //Debug.Log("第一个切面边数" + cutEdges1.Count / 2);
        //for (int i = 0; i < cutEdges1.Count / 2; i++)
        //{
        //    Debug.Log("第" + i.ToString() + "条边顶点坐标" + cutEdges1[2 * i].x + "  " + cutEdges1[2 * i].y + "  " + cutEdges1[2 * i].z + " " + cutEdges1[2 * i + 1].x + "  " + cutEdges1[2 * i + 1].y + "  " + cutEdges1[2 * i + 1].z);
        //}
        Dictionary<Vector3, int> dictionary1 = new Dictionary<Vector3, int>();
        UnionFind union = new UnionFind(cutEdges1.Count);
        for (int i = 0; i < cutEdges1.Count / 2; i++)
        {
            union.union(2 * i + 0, 2 * i + 1);
        }
        for (int i = 0; i < cutEdges1.Count; i++) //连通重复点
        {

            if (dictionary1.ContainsKey(cutEdges1[i]))
            {
                union.union(i, dictionary1[cutEdges1[i]]);
            }
            else
            {
                dictionary1.Add(cutEdges1[i], i);
            }
        }
        int count = union.getCount();
        Debug.Log("第一个切面连通区域个数" + count);
        if (count >= 1)
        {
            List<int>[] arrays = union.getResult();
            int[] oldToNewVertexMap = new int[cutEdges1.Count];
            int[] partNumber = new int[cutEdges1.Count];

            List<Vector3>[] newVerticesArrays = new List<Vector3>[count];

            for (int i = 0; i < count; i++) //初始化新顶点数组
            {
                newVerticesArrays[i] = new List<Vector3>(cutEdges1.Count / 2);
            }

            for (int i = 0; i < count; i++)//映射 旧顶点与新顶点
            {
                List<int> array = arrays[i];
                for (int j = 0; j < array.Count; j++)
                {
                    oldToNewVertexMap[array[j]] = j;
                    partNumber[array[j]] = i;

                    newVerticesArrays[i].Add(cutEdges1[array[j]]);
                }
            }
            //找到并连接端点的两个点
            for (int j = 0; j < count; j++)
            {
                Dictionary<Vector3, int> dictionary2 = new Dictionary<Vector3, int>();
                Vector3 point_start = new Vector3();
                Vector3 point_end = new Vector3();
                int startpointcount = 0;
                int endpointcount = 0;
                for (int i = 0; i < newVerticesArrays[j].Count; i++)
                {
                    if (dictionary2.ContainsKey(newVerticesArrays[j][i]))
                    {
                        dictionary2[newVerticesArrays[j][i]] = 2;
                    }
                    else
                    {
                        if (i % 2 == 0)
                        {
                            dictionary2.Add(newVerticesArrays[j][i], 0);
                        }
                        else
                        {
                            dictionary2.Add(newVerticesArrays[j][i], 1);
                        }
                    }
                }
                foreach (KeyValuePair<Vector3, int> item in dictionary2)
                {
                    if (item.Value == 0)
                    {
                        point_end = item.Key;
                        endpointcount++;
                    }
                    else if (item.Value == 1)
                    {
                        point_start = item.Key;
                        startpointcount++;
                    }
                }
                Debug.Log("第" + j + "个连通区域startpointcount=" + startpointcount + " endpointcount=" + endpointcount);
                if (startpointcount == 1 && endpointcount == 1)
                {
                    cutEdges1.Add(point_start);
                    cutEdges1.Add(point_end);
                    //Debug.Log("第一个面端点起点坐标" + point_start.x + " " + point_start.y + " " + point_start.z + "端点终点坐标" + point_end.x + " " + point_end.y + " " + point_end.z);
                }
            }
        }

        //第二个面
        dictionary1 = new Dictionary<Vector3, int>();
        union = new UnionFind(cutEdges2.Count);
        for (int i = 0; i < cutEdges2.Count / 2; i++)
        {
            union.union(2 * i + 0, 2 * i + 1);
        }
        for (int i = 0; i < cutEdges2.Count; i++) //连通重复点
        {

            if (dictionary1.ContainsKey(cutEdges2[i]))
            {
                union.union(i, dictionary1[cutEdges2[i]]);
            }
            else
            {
                dictionary1.Add(cutEdges2[i], i);
            }
        }
        count = union.getCount();
        Debug.Log("第二个切面连通区域个数" + count);
        if (count >= 1)
        {
            List<int>[] arrays = union.getResult();
            int[] oldToNewVertexMap = new int[cutEdges2.Count];
            int[] partNumber = new int[cutEdges2.Count];

            List<Vector3>[] newVerticesArrays = new List<Vector3>[count];

            for (int i = 0; i < count; i++) //初始化新顶点数组
            {
                newVerticesArrays[i] = new List<Vector3>(cutEdges2.Count / 2);
            }

            for (int i = 0; i < count; i++)//映射 旧顶点与新顶点
            {
                List<int> array = arrays[i];
                for (int j = 0; j < array.Count; j++)
                {
                    oldToNewVertexMap[array[j]] = j;
                    partNumber[array[j]] = i;

                    newVerticesArrays[i].Add(cutEdges2[array[j]]);
                }
            }
            //找到并连接端点的两个点
            for (int j = 0; j < count; j++)
            {
                Dictionary<Vector3, int> dictionary2 = new Dictionary<Vector3, int>();
                Vector3 point_start = new Vector3();
                Vector3 point_end = new Vector3();
                int startpointcount = 0;
                int endpointcount = 0;
                for (int i = 0; i < newVerticesArrays[j].Count; i++)
                {
                    if (dictionary2.ContainsKey(newVerticesArrays[j][i]))
                    {
                        dictionary2[newVerticesArrays[j][i]] = 2;
                    }
                    else
                    {
                        if (i % 2 == 0)
                        {
                            dictionary2.Add(newVerticesArrays[j][i], 0);
                        }
                        else
                        {
                            dictionary2.Add(newVerticesArrays[j][i], 1);
                        }
                    }
                }
                foreach (KeyValuePair<Vector3, int> item in dictionary2)
                {
                    if (item.Value == 0)
                    {
                        point_end = item.Key;
                        endpointcount++;
                    }
                    else if (item.Value == 1)
                    {
                        point_start = item.Key;
                        startpointcount++;
                    }
                }
                Debug.Log("第" + j + "个连通区域startpointcount=" + startpointcount + " endpointcount=" + endpointcount);
                if (startpointcount == 1 && endpointcount == 1)
                {
                    cutEdges2.Add(point_start);
                    cutEdges2.Add(point_end);
                    //Debug.Log("第二个面端点起点坐标" + point_start.x + " " + point_start.y + " " + point_start.z + "端点终点坐标" + point_end.x + " " + point_end.y + " " + point_end.z);
                }
            }
        }

        /******************半圆缝合结束*********************/

        int cutOverTime = System.Environment.TickCount;
        //------添加切割信息到Cut Position中
        Vector3 cutPosition = new Vector3();
        for (int i = 0; i < cutEdges1.Count; i = i + 2)
        {
            cutPosition += cutEdges1[i];
        }
        cutPosition = cutPosition / (cutEdges1.Count / 2);//平均法求中心点坐标
                                                          //Plane plane = new Plane(localSplitPlaneNormal, cutPosition);
        GameObject t = new GameObject();
        t.transform.position = beCutThing.transform.position;
        t.transform.rotation = beCutThing.transform.rotation;
        Mesh mesh = new Mesh();
        mesh.vertices = beCutThing.GetComponent<MeshFilter>().mesh.vertices;
        mesh.normals = beCutThing.GetComponent<MeshFilter>().mesh.normals;
        mesh.triangles = beCutThing.GetComponent<MeshFilter>().mesh.triangles;
        t.AddComponent<MeshFilter>().mesh = mesh;
        CutPosition.cutObject.Add(t);
        //CutPosition.cutObject.Add(beCutThing);
        CutPosition.cutPoint.Add(cutPosition);
        CutPosition.cutPlaneNormal.Add(localSplitPlaneNormal);
        //------

        //FillCutEdges(a, b, cutEdges, localSplitPlaneNormal);
        int edgeCount = cutEdges1.Count / 2;

        List<Vector3> points = new List<Vector3>(edgeCount);// T 边
        List<int> outline = new List<int>(edgeCount * 2);//T 轮廓

        int start = 0;






        //T 遍历边
        for (int current = 0; current < edgeCount; current++)
        {
            int next = current + 1;

            // Find the next edge
            int nearest = start;
            float nearestDistance = (cutEdges1[current * 2 + 1] - cutEdges1[start * 2 + 0]).sqrMagnitude;

            for (int other = next; other < edgeCount; other++)
            {
                float distance = (cutEdges1[current * 2 + 1] - cutEdges1[other * 2 + 0]).sqrMagnitude;//列表中的下条边的第一个点距离当前边的第一个点的距离

                if (distance < nearestDistance)
                {
                    nearest = other;
                    nearestDistance = distance;
                }
            }




            // Is the current edge the last edge in this edge loop?
            if (nearest == start && current > start)//T 围成一个环
            {
                int pointStart = points.Count;
                int pointCounter = pointStart;

                // Add this edge loop to the triangulation lists
                for (int edge = start; edge < current; edge++)
                {
                    points.Add(cutEdges1[edge * 2 + 0]);//加入 入点
                    outline.Add(pointCounter++);
                    outline.Add(pointCounter);
                }

                points.Add(cutEdges1[current * 2 + 0]);
                outline.Add(pointCounter);
                outline.Add(pointStart);

                // Start a new edge loop
                start = next;
            }
            else if (next < edgeCount)//若还不是一个环 交换位置
            {
                // Move the nearest edge so that it follows the current edge
                Vector3 n0 = cutEdges1[next * 2 + 0];
                Vector3 n1 = cutEdges1[next * 2 + 1];

                cutEdges1[next * 2 + 0] = cutEdges1[nearest * 2 + 0];
                cutEdges1[next * 2 + 1] = cutEdges1[nearest * 2 + 1];

                cutEdges1[nearest * 2 + 0] = n0;
                cutEdges1[nearest * 2 + 1] = n1;
            }
            //尝试解决半圆缝合问题2020.04.07
            //else
            //{
            //    //两件事 加点 加边
            //    cutEdges1.Add(cutEdges2[current]);
            //    cutEdges1.Add(cutEdges2[start]);
            //}

        }
        //临时变量存储
        CutPosition.CutPoints.Add(points);
        List<Vector3> ns = new List<Vector3>();
        for (int i = 0; i < points.Count; i++)
        {
            ns.Add(-localSplitPlaneNormal);
        }
        CutPosition.CutNors.Add(ns);
        CutPosition.CutLine.Add(outline);

        //取消切面缝合
        ///*
        //用作标记切面
        //Mesh mesh = new Mesh();
        //切割边数量大于1
        if (points.Count > 0)             //调用triangulator
        {
            // Triangulate the outline
            int[] newEdges, newTriangles, newTriangleEdges;

            ITriangulator triangulator = new Triangulator(points, outline, localSplitPlaneNormal);

            triangulator.Fill(out newEdges, out newTriangles, out newTriangleEdges);

            // Add the new vertices
            int offsetA = newVertices1.Count;
            //int offsetB = newVertices2.Count;

            newVertices1.AddRange(points);
            //newVertices2.AddRange(points);


            Vector3 normalA = -localSplitPlaneNormal;
            Vector3 normalB = localSplitPlaneNormal;



            temp_Nor_List.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                newNormals1.Add(normalA);
                //newNormals2.Add(normalB);

                //临时记录
                //temp_Nor_List.Add(normalA);

            }

            ///*
            Debug.Log("==========");
            Debug.Log("temp_Nor_List.Count: " + temp_Nor_List.Count);
            Debug.Log("points.Count:  " + points.Count);
            Debug.Log("newTriangles.Length:  " + newTriangles.Length);
            Debug.Log("==========");
            //获得切面顶点法向量
            //mesh.normals = temp_Nor_List.ToArray();
            ////获得切面顶点坐标
            //mesh.vertices = points.ToArray();
            ////获得切面三角关系
            //mesh.triangles = new int[newTriangles.Length];
            //mesh.triangles = newTriangles;
            ////绘制
            //cutPlaneMark(mesh);
            Debug.Log("绘制结束！！！！");


            //Debug.Log("==========");
            //cutPlaneMark(points, temp_Nor_List, newTriangles);
            //Debug.Log("绘制结束！！！！");
            //Debug.Log("==========");

            // Add the new triangles
            int newTriangleCount = newTriangles.Length / 3;

            for (int i = 0; i < newTriangleCount; i++)
            {
                newTriangles1.Add(offsetA + newTriangles[i * 3 + 0]);
                newTriangles1.Add(offsetA + newTriangles[i * 3 + 2]);
                newTriangles1.Add(offsetA + newTriangles[i * 3 + 1]);

                //newTriangles2.Add(offsetB + newTriangles[i * 3 + 0]);
                //newTriangles2.Add(offsetB + newTriangles[i * 3 + 1]);
                //newTriangles2.Add(offsetB + newTriangles[i * 3 + 2]);
            }

            //*/
        }
        //*/

        GameObject line1 = new GameObject("line1");
        //Debug.Log("===========uouououo1111=========");
        //Debug.Log(line1);
        //Debug.Log("===========uouououo11111=========");
        Debug.Log("line1" + line1);
        line1.tag = "temp_cutplane_mark";
        line1.transform.SetParent(beCutThing.transform);
        line[0] = line1.AddComponent<LineRenderer>();
        line[0].material = material;
        line1.GetComponent<LineRenderer>().useWorldSpace = false;
        //line1.transform.localScale.Set(1, 1, 1);
        line1.transform.localScale = new Vector3(1f, 1f, 1f);
        //line1.GetComponent<>()
        line[0].positionCount = points.Count + 1;
        line[0].startWidth = 0.0003f;
        line[0].endWidth = 0.0003f;
        for (int i = 0; i < points.Count; i++)
            line[0].SetPosition(i, points[i]);//设置渲染顶点
        line[0].SetPosition(points.Count, points[0]);
        //line1.transform.position = Vector3.zero;
        //line1.transform.rotation = Quaternion.Euler(Vector3.zero);

        //--------------------------------------------------------------
        ///<summary>2020.04.07想法
        ///         此处可以做优化
        ///</summary>
        int edgeCount2 = cutEdges2.Count / 2;

        points = new List<Vector3>(edgeCount2);
        outline = new List<int>(edgeCount2 * 2);

        start = 0;

        for (int current = 0; current < edgeCount2; current++)
        {
            int next = current + 1;

            // Find the next edge
            int nearest = start;
            float nearestDistance = (cutEdges2[current * 2 + 1] - cutEdges2[start * 2 + 0]).sqrMagnitude;

            for (int other = next; other < edgeCount2; other++)
            {
                float distance = (cutEdges2[current * 2 + 1] - cutEdges2[other * 2 + 0]).sqrMagnitude;

                if (distance < nearestDistance)
                {
                    nearest = other;
                    nearestDistance = distance;
                }
            }

            // Is the current edge the last edge in this edge loop?
            if (nearest == start && current > start)
            {
                int pointStart = points.Count;
                int pointCounter = pointStart;

                // Add this edge loop to the triangulation lists
                for (int edge = start; edge < current; edge++)
                {
                    points.Add(cutEdges2[edge * 2 + 0]);
                    outline.Add(pointCounter++);
                    outline.Add(pointCounter);
                }

                points.Add(cutEdges2[current * 2 + 0]);
                outline.Add(pointCounter);
                outline.Add(pointStart);

                // Start a new edge loop
                start = next;
            }
            else if (next < edgeCount2)
            {
                // Move the nearest edge so that it follows the current edge
                Vector3 n0 = cutEdges2[next * 2 + 0];
                Vector3 n1 = cutEdges2[next * 2 + 1];

                cutEdges2[next * 2 + 0] = cutEdges2[nearest * 2 + 0];
                cutEdges2[next * 2 + 1] = cutEdges2[nearest * 2 + 1];

                cutEdges2[nearest * 2 + 0] = n0;
                cutEdges2[nearest * 2 + 1] = n1;
            }
            //尝试解决半圆缝合问题2020.04.07
            //else
            //{
            //    //两件事 加点 加边
            //    cutEdges2.Add(cutEdges2[current]);
            //    cutEdges2.Add(cutEdges2[start]);
            //}
        }
        ///*
        if (points.Count > 0)             //调用triangulator
        {
            // Triangulate the outline
            int[] newEdges, newTriangles, newTriangleEdges;

            ITriangulator triangulator = new Triangulator(points, outline, localSplitPlaneNormal);

            triangulator.Fill(out newEdges, out newTriangles, out newTriangleEdges);

            int offsetB = newVertices1.Count;

            newVertices1.AddRange(points);


            Vector3 normalA = -localSplitPlaneNormal;
            Vector3 normalB = localSplitPlaneNormal;

            for (int i = 0; i < points.Count; i++)
            {

                newNormals1.Add(normalB);
            }


            // Add the new triangles
            int newTriangleCount = newTriangles.Length / 3;

            for (int i = 0; i < newTriangleCount; i++)
            {


                newTriangles1.Add(offsetB + newTriangles[i * 3 + 0]);
                newTriangles1.Add(offsetB + newTriangles[i * 3 + 1]);
                newTriangles1.Add(offsetB + newTriangles[i * 3 + 2]);
            }


        }
        //*/
        GameObject line2 = new GameObject("line2");
        //Debug.Log("===========uouououo222=========");
        //Debug.Log(line2);
        //Debug.Log("===========uouououo222=========");
        //Debug.Log("line2" + line2);
        line2.tag = "temp_cutplane_mark";
        line2.transform.SetParent(beCutThing.transform);
        line[1] = line2.AddComponent<LineRenderer>();
        line[1].material = material;
        //line[1].material = new Material(Shader.Find("snapTurnArrow"));
        line2.GetComponent<LineRenderer>().useWorldSpace = false;
        //line2.transform.localScale.Set(1, 1, 1);//设置其大小，避免出现过大情况 就不会附着在父物体上
        line2.transform.localScale = new Vector3(1f, 1f, 1f);
        line[1].positionCount = points.Count + 1;
        line[1].startWidth = 0.0003f;
        line[1].endWidth = 0.0003f;
        for (int i = 0; i < points.Count; i++)
            line[1].SetPosition(i, points[i]);//设置渲染顶点
        line[1].SetPosition(points.Count, points[0]);

        //修正标记的位置和旋转角

        //line1.transform.position = Vector3.zero;
        //line2.transform.position = Vector3.zero;
        //Debug.Log("line1.transform.position: " + line1.transform.position);
        //Debug.Log("line2.transform.position: " + line2.transform.position);
        //Debug.Log("=========修正位置&&角度==========");
        //line1.transform.position = Vector3.zero;
        //line2.transform.position = Vector3.zero;
        line1.transform.localPosition = Vector3.zero;
        line2.transform.localPosition = Vector3.zero;
        //Debug.Log("line1.transform.position: " + line1.transform.position);
        //Debug.Log("line[0].positionCount: " + line[0].positionCount);
        //Debug.Log("line2.transform.position: " + line2.transform.position);
        //Debug.Log("line[1].positionCount: " + line[1].positionCount);
        //line1.transform.rotation = Quaternion.Euler(Vector3.zero);
        //line2.transform.rotation = Quaternion.Euler(Vector3.zero);
        line1.transform.localRotation = Quaternion.Euler(Vector3.zero);
        line2.transform.localRotation = Quaternion.Euler(Vector3.zero);
        //Debug.Log("============修正完成=============");

        //边标记
        //cutHullmark = new List<Vector3>(cutEdges1);
        //cutHullmark.AddRange(cutEdges2);
        //标注切割面
        //if (cutHullmark != null)
        //{
        //    Debug.Log("开始画线");
        //    Debug.Log("共有" + cutHullmark.Count / 2 + "条线");
        //    for (int i = 0; i < cutHullmark.Count - 1; i++)
        //    {
        //        Debug.DrawLine(transform.TransformPoint(cutHullmark[i]), transform.TransformPoint(cutHullmark[i + 1]), Color.red);
        //        //Debug.DrawLine()
        //    }
        //}

        int trianglationOverTime = System.Environment.TickCount;


        beCutThing.GetComponent<MeshFilter>().mesh.Clear();
        beCutThing.GetComponent<MeshFilter>().mesh.vertices = newVertices1.ToArray();
        beCutThing.GetComponent<MeshFilter>().mesh.normals = newNormals1.ToArray();
        beCutThing.GetComponent<MeshFilter>().mesh.triangles = newTriangles1.ToArray();



        int isolateStartTime = System.Environment.TickCount;
        IsolateMesh(beCutThing);

        //结束后清空CutPlanePosition链表数据
        //CutPlanePosition = new List<Vector3>(4);
        CutPlanePosition.Clear();
        cutPlanePoints.Clear();
        //Isolate.IsolateMesh(part2);
        //part1.tag = "Objects";
        //part2.tag = "Objects";
        int overTime = System.Environment.TickCount;
        //Debug.Log("切割总耗时：" + (overTime - startTime) + "      三角面分割耗时：" + (cutOverTime - startTime) + "            缝补耗时：" + (trianglationOverTime - cutOverTime) + "           分离耗时：" + (overTime - trianglationOverTime));
        //Debug.Log("======test=======");
        //cutPlaneMark(beCutThing.GetComponent<MeshFilter>().mesh);//无效
        //Debug.Log("======test=======");
        //Debug.Log("===========uouououo=========");
        //Debug.Log(line);
        //Debug.Log(line1);
        //line1.transform.localScale.Set(1.0f, 1.0f, 1.0f);
        //Debug.Log(line1.transform.localScale);
        //Debug.Log(line2);
        //line2.transform.localScale.Set(1.0f, 1.0f, 1.0f);
        //Debug.Log(line2.transform.localScale);
        //Debug.Log("===========uouououo=========");
        Debug.Log("目前的记录数：" + CutPosition.cutAnchorPoints.Count);
        Debug.Log("CutPosition.cutAnchorPoints[0]：" + CutPosition.cutAnchorPoints[0].Count);
        if (CutPosition.cutAnchorPoints.Count > 1)
            Debug.Log("CutPosition.cutAnchorPoints[1]：" + CutPosition.cutAnchorPoints[0].Count);
    }

    protected void SplitTriangletop1(Vector3 pointOnPlane, Vector3 planeNormal, Vector3[] vertices, Vector3[] normals, int top, int cw, int ccw, out Vector3 cwIntersection, out Vector3 ccwIntersection, out Vector3 cwNormal, out Vector3 ccwNormal)
    {

        Vector3 v0 = vertices[top];
        Vector3 v1 = vertices[cw];
        Vector3 v2 = vertices[ccw];

        // Intersect the top-cw edge with the plane
        float cwDenominator = Vector3.Dot(v1 - v0, planeNormal);
        float cwScalar = Vector3.Dot(pointOnPlane - v0, planeNormal) / cwDenominator;//比例

        // Intersect the top-ccw edge with the plane
        float ccwDenominator = Vector3.Dot(v2 - v0, planeNormal);
        float ccwScalar = Vector3.Dot(pointOnPlane - v0, planeNormal) / ccwDenominator;


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
    protected void SplitTriangletop0(Vector3 pointOnPlane, Vector3 planeNormal, Vector3[] vertices, Vector3[] normals, int top, int cw, int ccw, out Vector3 cwIntersection, out Vector3 ccwIntersection, out Vector3 cwNormal, out Vector3 ccwNormal)
    {

        Vector3 v0 = vertices[top];
        Vector3 v1 = vertices[cw];
        Vector3 v2 = vertices[ccw];

        // Intersect the top-cw edge with the plane
        float cwDenominator = Vector3.Dot(v0 - v1, planeNormal);
        float cwScalar = Vector3.Dot(pointOnPlane - v1, planeNormal) / cwDenominator;

        // Intersect the top-ccw edge with the plane
        float ccwDenominator = Vector3.Dot(v0 - v2, planeNormal);
        float ccwScalar = Vector3.Dot(pointOnPlane - v2, planeNormal) / ccwDenominator;

        // Interpolate vertex positions                               计算新点的位置
        Vector3 cwVertex = new Vector3();

        cwVertex.x = v1.x + (v0.x - v1.x) * cwScalar;
        cwVertex.y = v1.y + (v0.y - v1.y) * cwScalar;
        cwVertex.z = v1.z + (v0.z - v1.z) * cwScalar;

        Vector3 ccwVertex = new Vector3();

        ccwVertex.x = v2.x + (v0.x - v2.x) * ccwScalar;
        ccwVertex.y = v2.y + (v0.y - v2.y) * ccwScalar;
        ccwVertex.z = v2.z + (v0.z - v2.z) * ccwScalar;

        // Interpolate normals

        Vector3 n0 = normals[top];
        Vector3 n1 = normals[cw];
        Vector3 n2 = normals[ccw];

        Vector3 cwNormalx = new Vector3();              //计算新点的法线

        cwNormalx.x = n1.x + (n0.x - n1.x) * cwScalar;
        cwNormalx.y = n1.y + (n0.y - n1.y) * cwScalar;
        cwNormalx.z = n1.z + (n0.z - n1.z) * cwScalar;

        cwNormalx.Normalize();

        Vector3 ccwNormalx = new Vector3();

        ccwNormalx.x = n2.x + (n0.x - n2.x) * ccwScalar;
        ccwNormalx.y = n2.y + (n0.y - n2.y) * ccwScalar;
        ccwNormalx.z = n2.z + (n0.z - n2.z) * ccwScalar;

        ccwNormalx.Normalize();
        cwNormal = cwNormalx;
        ccwNormal = ccwNormalx;


        cwIntersection = cwVertex;
        ccwIntersection = ccwVertex;
    }

    private Vector3 GetDifferentVector3(Vector3 v)//在float有效数字最后一位加1（使切割产生的重复点不重复）
    {
        Vector3 v1 = new Vector3();
        float x = v.x;
        float n = float.Epsilon;
        float y = x + n;
        while (x == y) { n = n * 2; y += n; }
        v1.x = y;
        v1.y = v.y;
        v1.z = v.z;
        return v1;
    }
    private Vector3 GetDifferentVector3one(Vector3 v1, Vector3 v2)//（使切割产生的重复点不重复）
    {
        Vector3 v = new Vector3();
        Vector3 dir = new Vector3();
        v.x = v1.x;
        v.y = v1.y;
        v.z = v1.z;
        dir.x = v2.x - v1.x;
        dir.y = v2.y - v1.y;
        dir.z = v2.z - v1.z;
        v.x += 0.01f * dir.x;
        v.y += 0.01f * dir.y;
        v.z += 0.01f * dir.z;
        return v;
    }
    /// <summary>
    /// 分离网格
    /// </summary>
    /// <param name="beIsolatedThing"></param>
    public void IsolateMesh(GameObject beIsolatedThing)
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
        Debug.Log(union.getCount());
        int num = 0;

        int count = union.getCount();

        if (count >= 2)              //分离
        {
            ///*
            ///<summary>
            ///先清除切片标记
            ///</summary>
            GameObject[] cutplanemark = GameObject.FindGameObjectsWithTag("temp_cutplane_mark");
            for (int i = 0; i < cutplanemark.Length; i++)
            {
                DestroyImmediate(cutplanemark[i]);//删除标记
            }
            //*/
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
            Debug.Log("分离耗时：" + (endTime - startTime));
            //用于记录较小的那部分 用于创建双导板时使用
            int n = int.MaxValue;
            //int n;

            //清理所有被选中物体 因为可能存在复制问题 导致标签重复 2020.08.08
            GameObject[] ChosenObjects = GameObject.FindGameObjectsWithTag("ChosenObject");
            for (int i = 0; i < ChosenObjects.Length; i++)
                ChosenObjects[i].tag = "Objects";

            GameObject temp = null;
            for (int i = 0; i < count; i++)//
            {
                if (newTrianglesArrays[i].Count < tri.Length / 500) continue; //小于原网格1%的小碎片忽视掉（但可能有些小碎片是有用的）
                //GameObject part = new GameObject(beIsolatedThing.name + i.ToString());
                ///<summary>需要删除原物体碰撞器 再重新生成</summary>
                GameObject part = Instantiate(beIsolatedThing, null);

                ////初始化n
                //n = part.GetComponent<MeshFilter>().mesh.vertices.Length;

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
                DestroyImmediate(part.GetComponent<MeshCollider>());
                part.AddComponent<MeshCollider>();
                //记录最小的物体
                Debug.Log("part.GetComponent<MeshFilter>().mesh.vertices: " + part.GetComponent<MeshFilter>().mesh.vertices.Length);
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
            //记录数据
            if (num > 1)
            {
                CutPosition.DoubleHolderToolHelper = GameObject.Instantiate(temp);
                CutPosition.DoubleHolderToolHelper.name = "DoubleHolderToolHelper";
                CutPosition.DoubleHolderToolHelper.GetComponent<MeshRenderer>().material = Transparent;
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



    //平面切割-->用于测试切割平面是否正确
    private void WholeCut()
    {
        //==================
        int startTime = System.Environment.TickCount;
        //根据本切割工具网格顶点形成切割面
        Vector3[] v = transform.GetComponent<MeshFilter>().mesh.vertices;
        //Plane cutPlane = new Plane(transform.TransformPoint(v[0]), transform.TransformPoint(v[1]), transform.TransformPoint(v[v.Length - 1]));
        Debug.Log(cutPlane.normal);

        //把切割面转化为被切割物体的当地坐标
        Vector3 localPoint = beCutThing.transform.InverseTransformPoint(cutPlane.normal * -cutPlane.distance);
        Debug.Log(localPoint);
        Vector3 localSplitPlaneNormal = beCutThing.transform.InverseTransformDirection(cutPlane.normal);
        localSplitPlaneNormal.Normalize();

        //根据切割面，把顶点分成两部分

        Vector3[] vertices = beCutThing.GetComponent<MeshFilter>().mesh.vertices;
        Debug.Log("被切割物体的顶点数：" + vertices.Length);
        Vector3[] normals = beCutThing.GetComponent<MeshFilter>().mesh.normals;

        bool[] vertexAbovePlane;
        vertexAbovePlane = new bool[vertices.Length];
        int[] oldToNewVertexMap = new int[vertices.Length];

        List<Vector3> newVertices1 = new List<Vector3>(vertices.Length);//为了避免数组多次动态增加容量，给最大值
        Debug.Log("newVertices1:" + newVertices1.Count);
        List<Vector3> newVertices2 = new List<Vector3>(vertices.Length);
        List<Vector3> newNormals1 = new List<Vector3>(normals.Length);
        List<Vector3> newNormals2 = new List<Vector3>(normals.Length);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            bool abovePlane = Vector3.Dot(vertex - localPoint, localSplitPlaneNormal) >= 0.0f;
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
        if (newVertices1.Count == 0 || newVertices2.Count == 0)
        {
            Debug.Log("nothing be cut,over");
            return;
        }
        //分三角面

        int[] indices = beCutThing.GetComponent<MeshFilter>().mesh.triangles;
        Debug.Log("被切割物体的三角面数：" + indices.Length);
        int triangleCount = indices.Length / 3;
        List<int> newTriangles1 = new List<int>(indices.Length);
        List<int> newTriangles2 = new List<int>(indices.Length);
        List<Vector3> cutEdges = new List<Vector3>();
        for (int i = 0; i < triangleCount; i++) // 遍历每一个三角面
        {
            int index0 = indices[i * 3 + 0];
            int index1 = indices[i * 3 + 1];
            int index2 = indices[i * 3 + 2];

            bool above0 = vertexAbovePlane[index0];
            bool above1 = vertexAbovePlane[index1];
            bool above2 = vertexAbovePlane[index2];

            if (above0 && above1 && above2)
            {
                newTriangles1.Add(oldToNewVertexMap[index0]);
                newTriangles1.Add(oldToNewVertexMap[index1]);
                newTriangles1.Add(oldToNewVertexMap[index2]);
            }
            else if (!above0 && !above1 && !above2)
            {
                newTriangles2.Add(oldToNewVertexMap[index0]);
                newTriangles2.Add(oldToNewVertexMap[index1]);
                newTriangles2.Add(oldToNewVertexMap[index2]);
            }
            else
            {
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
                    newTriangles1.Add(oldToNewVertexMap[top]);
                    newTriangles1.Add(newVertices1.Count);
                    newTriangles1.Add(newVertices1.Count + 1);

                    newTriangles2.Add(newVertices2.Count + 1);//cwIntersection
                    newTriangles2.Add(oldToNewVertexMap[cw]);
                    newTriangles2.Add(oldToNewVertexMap[ccw]);

                    newTriangles2.Add(oldToNewVertexMap[ccw]);
                    newTriangles2.Add(newVertices2.Count);//ccwIntersection
                    newTriangles2.Add(newVertices2.Count + 1);// cwIntersection


                    cutEdges.Add(cwIntersection);
                    cutEdges.Add(ccwIntersection);

                    newVertices1.Add(cwIntersection);
                    newVertices1.Add(ccwIntersection);
                    newNormals1.Add(cwNormal);
                    newNormals1.Add(ccwNormal);

                    newVertices2.Add(ccwIntersection);
                    newVertices2.Add(cwIntersection);
                    newNormals2.Add(ccwNormal);
                    newNormals2.Add(cwNormal);

                }
                else
                {
                    SplitTriangle(localPoint, localSplitPlaneNormal, vertices, normals, top, cw, ccw, out cwIntersection, out ccwIntersection, out cwNormal, out ccwNormal);
                    newTriangles1.Add(newVertices1.Count + 1);//cwIntersection
                    newTriangles1.Add(oldToNewVertexMap[cw]);
                    newTriangles1.Add(oldToNewVertexMap[ccw]);

                    newTriangles1.Add(oldToNewVertexMap[ccw]);
                    newTriangles1.Add(newVertices1.Count);//ccwIntersection
                    newTriangles1.Add(newVertices1.Count + 1);// cwIntersection

                    newTriangles2.Add(oldToNewVertexMap[top]);
                    newTriangles2.Add(newVertices2.Count);
                    newTriangles2.Add(newVertices2.Count + 1);

                    cutEdges.Add(ccwIntersection);//获取切面切点
                    cutEdges.Add(cwIntersection);

                    newVertices1.Add(ccwIntersection);
                    newVertices1.Add(cwIntersection);
                    newNormals1.Add(ccwNormal);
                    newNormals1.Add(cwNormal);

                    newVertices2.Add(cwIntersection);
                    newVertices2.Add(ccwIntersection);
                    newNormals2.Add(cwNormal);
                    newNormals2.Add(ccwNormal);

                }

            }
        }
        ///<summary>cutEdges中存放交点坐标
        ///<param name="cutEdges">交点坐标</para>
        ///</summary>

        ////缝合切面
        int cutOverTime = System.Environment.TickCount;
        //FillCutEdges(a, b, cutEdges, localSplitPlaneNormal);
        int edgeCount = cutEdges.Count / 2;

        List<Vector3> points = new List<Vector3>(edgeCount);
        List<int> outline = new List<int>(edgeCount * 2);

        int start = 0;

        for (int current = 0; current < edgeCount; current++)
        {
            int next = current + 1;

            // Find the next edge
            int nearest = start;
            float nearestDistance = (cutEdges[current * 2 + 1] - cutEdges[start * 2 + 0]).sqrMagnitude;//少精度的距离

            for (int other = next; other < edgeCount; other++)
            {
                float distance = (cutEdges[current * 2 + 1] - cutEdges[other * 2 + 0]).sqrMagnitude;

                if (distance < nearestDistance)
                {
                    nearest = other;
                    nearestDistance = distance;
                }
            }

            // Is the current edge the last edge in this edge loop?
            if (nearest == start && current > start)
            {
                int pointStart = points.Count;
                int pointCounter = pointStart;

                // Add this edge loop to the triangulation lists
                for (int edge = start; edge < current; edge++)
                {
                    points.Add(cutEdges[edge * 2 + 0]);
                    outline.Add(pointCounter++);
                    outline.Add(pointCounter);
                }

                points.Add(cutEdges[current * 2 + 0]);
                outline.Add(pointCounter);
                outline.Add(pointStart);

                // Start a new edge loop
                start = next;
            }
            else if (next < edgeCount)
            {
                // Move the nearest edge so that it follows the current edge
                Vector3 n0 = cutEdges[next * 2 + 0];
                Vector3 n1 = cutEdges[next * 2 + 1];

                cutEdges[next * 2 + 0] = cutEdges[nearest * 2 + 0];
                cutEdges[next * 2 + 1] = cutEdges[nearest * 2 + 1];

                cutEdges[nearest * 2 + 0] = n0;
                cutEdges[nearest * 2 + 1] = n1;
            }
        }

        if (points.Count > 0)             //调用triangulator
        {
            // Triangulate the outline
            int[] newEdges, newTriangles, newTriangleEdges;

            ITriangulator triangulator = new Triangulator(points, outline, localSplitPlaneNormal);

            triangulator.Fill(out newEdges, out newTriangles, out newTriangleEdges);

            // Add the new vertices
            int offsetA = newVertices1.Count;
            int offsetB = newVertices2.Count;

            newVertices1.AddRange(points);
            newVertices2.AddRange(points);


            Vector3 normalA = -localSplitPlaneNormal;
            Vector3 normalB = localSplitPlaneNormal;

            for (int i = 0; i < points.Count; i++)
            {
                newNormals1.Add(normalA);
                newNormals2.Add(normalB);
            }


            // Add the new triangles
            int newTriangleCount = newTriangles.Length / 3;

            for (int i = 0; i < newTriangleCount; i++)
            {
                newTriangles1.Add(offsetA + newTriangles[i * 3 + 0]);
                newTriangles1.Add(offsetA + newTriangles[i * 3 + 2]);
                newTriangles1.Add(offsetA + newTriangles[i * 3 + 1]);

                newTriangles2.Add(offsetB + newTriangles[i * 3 + 0]);
                newTriangles2.Add(offsetB + newTriangles[i * 3 + 1]);
                newTriangles2.Add(offsetB + newTriangles[i * 3 + 2]);
            }
        }

        //分割
        int trianglationOverTime = System.Environment.TickCount;

        //GameObject part1 = new GameObject(beCutThing.name + "(1)");
        //GameObject part2 = new GameObject(beCutThing.name + "(2)");
        //part1.AddComponent<MeshFilter>();
        //part1.AddComponent<MeshRenderer>();
        //part2.AddComponent<MeshFilter>();
        //part2.AddComponent<MeshRenderer>();
        //part1.GetComponent<MeshRenderer>().material = beCutThing.GetComponent<MeshRenderer>().material;
        //part2.GetComponent<MeshRenderer>().material = beCutThing.GetComponent<MeshRenderer>().material;

        GameObject part1 = Instantiate(beCutThing, null);//(T)用于复制物体？    
        part1.name = beCutThing.name + "A";
        GameObject part2 = Instantiate(beCutThing, null);
        part2.name = beCutThing.name + "B";


        part1.GetComponent<MeshFilter>().mesh.Clear();
        part1.GetComponent<MeshFilter>().mesh.vertices = newVertices1.ToArray();
        part1.GetComponent<MeshFilter>().mesh.normals = newNormals1.ToArray();
        part1.GetComponent<MeshFilter>().mesh.triangles = newTriangles1.ToArray();

        part2.GetComponent<MeshFilter>().mesh.Clear();
        part2.GetComponent<MeshFilter>().mesh.vertices = newVertices2.ToArray();
        part2.GetComponent<MeshFilter>().mesh.normals = newNormals2.ToArray();
        part2.GetComponent<MeshFilter>().mesh.triangles = newTriangles2.ToArray();



        //part1.AddComponent<MeshCollider>();
        //part1.GetComponent<MeshCollider>().convex = true;
        //part1.GetComponent<MeshCollider>().cookingOptions = MeshColliderCookingOptions.InflateConvexMesh;
        //part1.AddComponent<Interactable>();
        //part1.AddComponent<Choosable>();
        //part2.AddComponent<MeshCollider>();
        //part2.GetComponent<MeshCollider>().convex = true;
        //part2.GetComponent<MeshCollider>().cookingOptions = MeshColliderCookingOptions.InflateConvexMesh;
        //part2.AddComponent<Interactable>();
        //part2.AddComponent<Choosable>();
        beCutThing.SetActive(false);
        //part1.transform.position = beCutThing.transform.position + cutPlane.normal * 0.01f;//切割结束让两部分分开一点点，以看到切割的结果
        //part2.transform.position = beCutThing.transform.position - cutPlane.normal * 0.01f;
        part1.transform.localScale = beCutThing.transform.localScale;
        part2.transform.localScale = beCutThing.transform.localScale;
        part1.transform.rotation = beCutThing.transform.rotation;
        part2.transform.rotation = beCutThing.transform.rotation;

        //分离
        int isolateStartTime = System.Environment.TickCount;
        IsolateMesh(part1);
        IsolateMesh(part2);

        part1.tag = "Objects";
        part2.tag = "Objects";

        int overTime = System.Environment.TickCount;
        // Debug.Log("切割总耗时：" + (overTime - startTime) + "      三角面分割耗时：" + (cutOverTime - startTime) + "            缝补耗时：" + (trianglationOverTime - cutOverTime) + "           分离耗时：" + (overTime - trianglationOverTime));

    }

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
        float ccwScalar = Mathf.Clamp01(Vector3.Dot(pointOnPlane - v0, planeNormal) / ccwDenominator);

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

    /// <summary>
    /// 用于画球 做mesh标记
    /// </summary>
    void OnDrawGizmos()
    {
        //Debug.Log("beging to draw");
        Gizmos.color = Color.red;
        // Debug.Log(gameObject.GetComponent<MeshFilter>().sharedMesh.vertices.Length);
        //GameObject o = GameObject.Find("yoyotest");
        //Gizmos.DrawSphere(o.transform.TransformPoint(o.GetComponent<MeshFilter>().sharedMesh.vertices[0]), 0.01f);
        //Gizmos.DrawSphere(o.transform.TransformPoint(o.GetComponent<MeshFilter>().sharedMesh.vertices[10]), 0.01f);
        //Gizmos.DrawSphere(o.transform.TransformPoint(o.GetComponent<MeshFilter>().sharedMesh.vertices[110]), 0.01f);
        //Gizmos.DrawSphere(o.transform.TransformPoint(o.GetComponent<MeshFilter>().sharedMesh.vertices[120]),0.01f);
        /*
        cutHelper = transform.GetChild(0).gameObject;
        //Gizmos.DrawSphere(cutHelper.transform.TransformPoint(cutHelper.GetComponent<MeshFilter>().sharedMesh.vertices[0]), 0.0001f);
        Gizmos.DrawSphere(cutHelper.transform.TransformPoint(cutHelper.GetComponent<MeshFilter>().sharedMesh.vertices[10]), 0.0001f);
        //Gizmos.DrawSphere(cutHelper.transform.TransformPoint(cutHelper.GetComponent<MeshFilter>().sharedMesh.vertices[110]), 0.0001f);
        Gizmos.DrawSphere(cutHelper.transform.TransformPoint(cutHelper.GetComponent<MeshFilter>().sharedMesh.vertices[120]), 0.0001f);
        */
        //Gizmos.DrawSphere(transform.TransformPoint(gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[755]), 0.001f);
        //Gizmos.DrawSphere(transform.TransformPoint(gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[750]), 0.001f);
        //Gizmos.DrawSphere(transform.TransformPoint(gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[761]), 0.001f);
        //Gizmos.DrawSphere(transform.TransformPoint(gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[752]), 0.001f);
        //Gizmos.DrawSphere(transform.TransformPoint(gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[753]), 0.001f);
        //Gizmos.DrawSphere(transform.TransformPoint(gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[754]), 0.001f);



        Gizmos.DrawSphere(transform.TransformPoint(gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[767]), 0.001f);
        Gizmos.DrawSphere(transform.TransformPoint(gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[728]), 0.001f);

        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[728], gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[761]);
        //for (int i = 729; i < 730; i += 2)
        //{
        //    //Gizmos.DrawSphere(transform.TransformPoint(gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[i - 1]), 0.001f);
        //    Gizmos.DrawSphere(transform.TransformPoint(gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[i]), 0.001f);
        //}


        ////标注切割面
        //if (cutHullmark != null)
        //{
        //    Debug.Log("开始画线");
        //    Debug.Log("共有" + cutHullmark.Count / 2 + "条线");
        //    for (int i = 0; i < cutHullmark.Count - 1; i++)
        //    {
        //        //Gizmos.DrawLine(cutHullmark[i], cutHullmark[i + 1]);

        //    }
        //}

    }

    void cutPlaneMark(List<Vector3> ver, List<Vector3> nor, int[] tri)
    {
        //Mesh meshToMark = new Mesh(mesh);
        //Mesh meshToMark = mesh;
        //meshToMark.vertices = mesh.vertices;
        //meshToMark.triangles = mesh.triangles;
        //meshToMark.normals = mesh.normals;
        //Graphics.DrawMeshNow(meshToMark, Vector3.zero,Quaternion.identity);
        //生成新的网格面(有颜色) 用于做标记
        GameObject PlaneMark = new GameObject("PlaneMark");
        PlaneMark.transform.SetParent(beCutThing.transform);
        PlaneMark.AddComponent<MeshFilter>();
        PlaneMark.GetComponent<MeshFilter>().mesh.vertices = ver.ToArray();
        PlaneMark.GetComponent<MeshFilter>().mesh.triangles = tri;
        PlaneMark.GetComponent<MeshFilter>().mesh.normals = nor.ToArray();
        PlaneMark.transform.position = Vector3.zero;
        PlaneMark.transform.rotation = Quaternion.Euler(Vector3.zero);
        PlaneMark.tag = "temp_cutplane_mark";
        PlaneMark.AddComponent<MeshRenderer>();
        PlaneMark.GetComponent<MeshRenderer>().material.color = Color.red;
        GameObject planeMark = GameObject.Instantiate(PlaneMark);
        Vector3[] temp_nor = planeMark.GetComponent<MeshFilter>().mesh.normals;
        for (int i = 0; i < temp_nor.Length; i++)
        {
            temp_nor[i] = -temp_nor[i];
        }
        planeMark.transform.position = Vector3.zero;
        planeMark.transform.rotation = Quaternion.Euler(Vector3.zero);
        planeMark.GetComponent<MeshFilter>().mesh.normals = temp_nor;



    }

    //private void OnRenderObject()
    //{

    //}
    //void MeshMarkGenerate(Mesh mesh)
    //{

    //}
    /*
    [ContextMenu("根据MeshFilter组件生成Line数据")]
    private void GetMeshesData()
    {
#if UNITY_EDITOR
        GameObject o = UnityEditor.Selection.activeGameObject;
        if (o == null)
            return;
        if (meshs != null)
            meshs.Clear();
        else
            meshs = new List<Mesh>();
        MeshFilter[] meshFilters = o.GetComponentsInChildren<MeshFilter>(true);
        if (meshFilters != null && meshFilters.Length > 0)
        {
            foreach (var mf in meshFilters)
            {
                meshs.Add(mf.sharedMesh);
            }
            GenerateLines();
        }
        else
        {
            Debug.LogError("选中物体及子物体没有MeshFilter组件，请添加");
        }
        SkinnedMeshRenderer[] skinnedMeshRenderers = o.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (skinnedMeshRenderers != null && skinnedMeshRenderers.Length > 0)
        {
            foreach (var sr in skinnedMeshRenderers)
            {
                meshs.Add(sr.sharedMesh);
            }
            GenerateLines();
        }
        else
        {
            Debug.LogError("选中物体及子物体没有SkinnedMeshRenderer组件，请添加");
        }
#endif
    }
    */
};