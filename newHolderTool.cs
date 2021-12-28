using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

/// <summary>
/// 存储角点 用于缝合侧面
/// </summary>
public class AnchorPoints
{
    public int[] left_out = new int[4];
    public int[] rightt_out = new int[4];
    public int[] left_in = new int[4];
    public int[] right_in = new int[4];
}

/// <summary>
/// ----针对颅骨的手术导板生成----暂停
/// 
/// </summary>
public class newHolderTool : MonoBehaviour
{
    //
    static bool DrawTag = false;

    public GameObject LeftHolderTool;
    public GameObject RightHolderTool;

    public float modelThickness = 0.003f;   //模型厚度 预设0.003
    public float distance;         //表面网格提取范围
    public Material material;
    public Material transparency;
    public Transform LHT;//left holder transform
    public Transform RHT;

    //public Transform test;
    //标记材质
    public Material Mark_Material;

    private bool cut = false;
    private GameObject beCutThing;
    private Vector3 cutVector;                //下刀的轨迹方向
    private bool inner;
    private Plane abovePlaneOfOsteotome;
    private Plane underPlaneOfOsteotome;
    private Plane forwardPlaneOfOsteotome;
    private Plane backPlaneOfOsteotome;
    private Plane outsideForwardPlaneOfOsteotome;
    private Plane outsideBackPlaneOfOsteotome;

    public GameObject shell;
    //内外表面网格
    Mesh innerMesh;
    Mesh outterMesh;
    //记录三个面的边
    List<Vector3> testPoints = new List<Vector3>(4);

    private List<Vector3> Inner_linePoints1;
    private List<Vector3> Inner_linePoints2;
    private List<Vector3> Inner_linePoints3;

    static int numCounter = 0;
    // Use this for initialization


    //定义基于导板的六个方向 forward left up 剩下三个取反即可  以世界坐标形式存储
    Vector3 LeftHolder_up = new Vector3(), LeftHolder_forward = new Vector3(), LetfHolder_left = new Vector3();//先声明
    Vector3 RightHolder_up = new Vector3(), RightHolder_forward = new Vector3(), RightHolder_left = new Vector3();//先声明

    //存储角点的索引
    AnchorPoints anchorPoints = new AnchorPoints();
    void Start()
    {

        //添加元素
        //linePoints.Add(linePoints1);
        //linePoints.Add(linePoints2);
        //linePoints.Add(linePoints3);
        //linePoints[0] = new List<Vector3>();
        //linePoints[1] = new List<Vector3>();
        //linePoints[2] = new List<Vector3>();
        innerMesh = new Mesh();
        outterMesh = new Mesh();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))//按键B激活导板  用于调试
        //if (SteamVR_Actions.default_Teleport.GetStateDown(SteamVR_Input_Sources.Any)
        //    && !SteamVR_Actions.default_GrabPinch.state)
        {
            shell = new GameObject("DoubleHolder");
            List<int> triUp = new List<int>();
            List<int> triDown = new List<int>();
            beCutThing = GameObject.FindGameObjectWithTag("ChosenObject");
            Debug.Log("物体" + (beCutThing == null ? "找到！！" : "未找到！！"));
            GenerateShell(out shell, out triDown, out triUp);
        }
    }
    private void GenerateShell(out GameObject shell, out List<int> triDown, out List<int> triUp)
    {
        Debug.Log("ExtractMesh");
        triDown = new List<int>();
        triUp = new List<int>();
        shell = new GameObject("DoubleHolder");
        shell.AddComponent<MeshFilter>();
        shell.AddComponent<MeshRenderer>();
        //Mesh newMesh = new Mesh(); //为防止变量赋值时直接引用原物体网格，导致修改变量会改变原物体网格，先new个mesh把原物体网格复制过来再赋值给变量
        //Debug.Log("=====");
        //Debug.Log(CutPosition.DoubleHolderToolHelper);
        //Debug.Log("=====");
        //GameObject OriginObject = GameObject.Instantiate(CutPosition.DoubleHolderToolHelper);
        GameObject OriginObject = CutPosition.DoubleHolderToolHelper;
        //OriginObject.GetComponent<MeshRenderer>().material = transparency;
        //获得其未缝合的表面  进行三次切割
        //Plane plane1;
        //Plane plane2;
        //Plane plane3;
        ///*
        //Plane[] planes = new Plane[3];
        Vector3[] localPoints = new Vector3[3];
        Vector3[] localSplitNormals = new Vector3[3];
        Vector3 vertex = OriginObject.GetComponent<MeshFilter>().mesh.vertices[10];


        //保存表面信息(优先为内表面)
        List<Vector3> in_ver = new List<Vector3>(OriginObject.GetComponent<MeshFilter>().mesh.vertices);
        List<Vector3> in_nor = new List<Vector3>(OriginObject.GetComponent<MeshFilter>().mesh.normals);
        List<int> in_tri = new List<int>(OriginObject.GetComponent<MeshFilter>().mesh.triangles);
        List<int> Inner_line1 = null, Inner_line2 = null, Inner_line3 = null;
        List<Vector3> Inner_lineNor1 = null, Inner_lineNor2 = null, Inner_lineNor3 = null;
        for (int i = CutPosition.cutPoint.Count - 1; i > CutPosition.cutPoint.Count - 1 - 3 && i >= 0; i--)//进行三次切割 后三个记录的切割面
        {
            Vector3 temp_cutPoint = CutPosition.cutPoint[i];
            Vector3 temp_cutNormal = CutPosition.cutPlaneNormal[i];
            //确定平面可能有问题  08.22
            //if (Vector3.Dot(vertex - CutPosition.cutPoint[i], CutPosition.cutPlaneNormal[i]) >= 0.0f)//判断切割面与物体的关系

            float Dot_Res = Vector3.Dot(temp_cutPoint - vertex, temp_cutNormal);
            Debug.Log(Dot_Res);
            if (Dot_Res >= 0.0f)//判断切割面与物体的关系(与法向量是否同向？)
            {
                //同向 则顺着做平移
                //planes[i] = new Plane(temp_cutNormal, temp_cutPoint + 0.001f * temp_cutNormal);
                //localPoints[i] = temp_cutPoint + 0.00008f * -temp_cutNormal;
                //temp_cutPoint = temp_cutPoint + 0.00005f * -temp_cutNormal;
                temp_cutPoint = temp_cutPoint + 0.0002f * -temp_cutNormal;
                localSplitNormals[i] = temp_cutNormal;
                Debug.Log("plane[" + i + "]已经生成");
                Debug.Log(Vector3.Dot(temp_cutPoint - vertex, temp_cutNormal));
            }
            else
            {
                //planes[i] = new Plane(temp_cutNormal, temp_cutPoint - 0.001f * temp_cutNormal);
                //localPoints[i] = temp_cutPoint + 0.00008f * temp_cutNormal;
                //temp_cutPoint = temp_cutPoint + 0.00005f * temp_cutNormal;
                temp_cutPoint = temp_cutPoint + 0.0002f * temp_cutNormal;
                localSplitNormals[i] = temp_cutNormal;
                Debug.Log("plane[" + i + "]已经生成");
                Debug.Log(Vector3.Dot(temp_cutPoint - vertex, temp_cutNormal));
                CutPosition.cutPlaneNormal[i] = -CutPosition.cutPlaneNormal[i];//使法向量为渲染方向 避免多次计算判断****
            }

            CutPosition.cutPoint[i] = temp_cutPoint;//更新localCutPoint
            //Mesh temp_Mesh = OriginObject.GetComponent<MeshFilter>().mesh;
            if (i == 0)
            //Cut(ref in_ver, ref in_nor, ref in_tri, temp_cutPoint, localSplitNormals[i], out linePoints1, out nor1);
            {
                WholeCut(temp_cutPoint, localSplitNormals[i], ref in_ver, ref in_nor, ref in_tri,
                    out Inner_linePoints1, out Inner_lineNor1, out Inner_line1);
                Debug.Log("====外网格第1次切割=======");
            }
            else if (i == 1)
            //Cut(ref in_ver, ref in_nor, ref in_tri, temp_cutPoint, localSplitNormals[i], out linePoints2, out nor2);
            {
                WholeCut(temp_cutPoint, localSplitNormals[i], ref in_ver, ref in_nor, ref in_tri,
                    out Inner_linePoints2, out Inner_lineNor2, out Inner_line2);
                Debug.Log("======外网格第2次切割======");
            }
            //针对颅骨有第三次切割
            //else 
            ////Cut(ref in_ver, ref in_nor, ref in_tri, temp_cutPoint, localSplitNormals[i], out linePoints3, out nor3);
            //{
            //    WholeCut(temp_cutPoint, localSplitNormals[i], ref in_ver, ref in_nor, ref in_tri,
            //        out Inner_linePoints3, out Inner_lineNor3, out Inner_line3);
            //    Debug.Log("======外网格第3次切割========");
            //}
        }
        //showMesh(in_ver, in_nor, in_tri);
        //DrawLinePoints(OriginObject, linePoints1);
        //DrawLinePoints(OriginObject, linePoints2);

        //Debug.Log("========");
        //Debug.Log("0:" + CutPosition.cutPoint[0]);
        //Debug.Log("1:" + CutPosition.cutPoint[1]);
        //Debug.Log("========");

        //showMesh(in_ver,in_nor,in_tri,"内网格");


        //for (int i = 0; i < in_nor.Count; i++)//法向量需要反转?  不需要
        //    in_nor[i] = -in_nor[i];
        //innerMesh.vertices = in_ver.ToArray();
        //innerMesh.normals = in_nor.ToArray();
        //innerMesh.triangles = in_tri.ToArray();

        //Debug.Log("innerMesh.vertexCount:" + innerMesh.vertexCount);
        //outterMesh = CutPosition.DoubleHolderToolHelper.GetComponent<MeshFilter>().mesh;
        //表面网格复制 形成外表面
        int innerMesh_Count = in_ver.Count;

        List<Vector3> out_ver = new List<Vector3>(innerMesh_Count * 2);
        List<Vector3> out_nor = new List<Vector3>(in_nor);
        List<int> out_tri = new List<int>(in_tri);
        for (int i = 0; i < innerMesh_Count; i++)
        {
            //Vector3 v = innerMesh.vertices[i];
            //Vector3 n = innerMesh.normals[i];

            //out_ver.Add(v + modelThickness * n);//法向量是反向的!!!？？？？
            out_ver.Add(in_ver[i] + modelThickness * in_nor[i]);//法向量是反向的!!!？？？？
                                                                //Debug.Log("======");
                                                                //Debug.Log("原：" + v);
                                                                //Debug.Log("现：" + ver[ver.Count - 1]);
                                                                //Debug.Log("======");
        }
        //Debug.DrawRay()
        //outterMesh.vertices = out_ver.ToArray();
        //outterMesh.normals = out_nor.ToArray();
        //outterMesh.triangles = out_tri.ToArray();

        //List<int> temp = new List<int>(innerMesh.triangles);
        //temp.Reverse();
        in_tri.Reverse();//内网格渲染翻转
                         //innerMesh.triangles = temp.ToArray();
        
        /*
        //将内外表面合并 测试
        Debug.Log("out_nor:" + out_nor.Count + "out_ver" + out_ver.Count);
        Debug.Log("in_nor:" + in_nor.Count + "in_ver:" + in_ver.Count);
        int temp_count = out_ver.Count;
        out_ver.AddRange(in_ver);
        out_nor.AddRange(in_nor);
        for (int i = 0; i < in_tri.Count; i += 3)
        {
            out_tri.Add(in_tri[i + 0] + in_ver.Count);
            out_tri.Add(in_tri[i + 1] + in_ver.Count);
            out_tri.Add(in_tri[i + 2] + in_ver.Count);
        }
        */

        //添加夹板
        //固定导板生成工具 依次确定切割面
        int pointNum = CutPosition.cutPoint.Count;

        ///<summary>是否为头骨</summary>
        bool IsSkull = pointNum > 2;
        //Debug.Log("========");
        //Debug.Log("0:" + CutPosition.cutPoint[0]);
        //Debug.Log("1:" + CutPosition.cutPoint[1]);
        //Debug.Log("========");
        Vector3 CP1;
        Vector3 CP2;
        int CP1_n = 0;
        //if (pointNum > 2)
        if (IsSkull)
        {
            CP1 = CutPosition.cutPoint[pointNum - 3];
            CP2 = CutPosition.cutPoint[pointNum - 2];
            CP1_n = pointNum - 3;
        }
        else
        {
            CP1 = CutPosition.cutPoint[pointNum - 2];
            CP2 = CutPosition.cutPoint[pointNum - 1];
            CP1_n = pointNum - 2;
        }
        //固定导板位置
        CP1 = CutPosition.cutObject[CutPosition.cutObject.Count - 1].transform.TransformPoint(CP1);
        CP2 = CutPosition.cutObject[CutPosition.cutObject.Count - 1].transform.TransformPoint(CP2);


        //float Dis1 = (LHT.TransformPoint(LHT.position) - CP1).magnitude;
        //float Dis2 = (LHT.TransformPoint(LHT.position) - CP2).magnitude;
        //Debug.Log("CP1:" + CP1 + "CP2:" + CP2);
        //Debug.Log("Dis1:" + Dis1 + "Dis2" + Dis2);
        DrawTag = true;//是否做标记

        LHT.position = CP1;//先切的为左
        RHT.position = CP2;
        //LHT.position = CutPosition.cutObject[CutPosition.cutObject.Count - 1].transform.TransformPoint(CutPosition.cutPoint[1]);
        Debug.Log("位置调整完毕");
        //LeftHolderTool.transform.rotation = CutPosition.cutObject[CutPosition.cutObject.Count - 1].transform.rotation;
        //RightHolderTool.transform.rotation = CutPosition.cutObject[CutPosition.cutObject.Count - 1].transform.rotation;
        LeftHolderTool.transform.rotation = Quaternion.LookRotation(CutPosition.cutObject[CP1_n].
            transform.TransformVector(CutPosition.cutPlaneNormal[CP1_n]));
        RightHolderTool.transform.rotation = Quaternion.LookRotation(CutPosition.cutObject[CP1_n].
            transform.TransformVector(CutPosition.cutPlaneNormal[CP1_n + 1]));
        Debug.Log("旋转角调整完毕");
        DestroyImmediate(LeftHolderTool.GetComponent<BoxCollider>());
        DestroyImmediate(RightHolderTool.GetComponent<BoxCollider>());
        //DestroyImmediate(beCutThing.GetComponent<MeshCollider>());
        //LeftHolderTool.transform.parent.SetParent(beCutThing.transform);
        //定义切割平面相关:  点+法向量
        //Vector3 LeftHolder_InnerNor = OriginObject.transform.InverseTransformVector(LHT.TransformVector(new Vector3(0, 0, 1)));
        Vector3 LeftHolder_OutterNor = LHT.TransformVector(new Vector3(1, 0, 0));//这个法向量有问题  重新想办法生成 by gh 2020.10.26



        //Vector3 LeftHolder_OutterNor = LHT.TransformVector((LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[0] - LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[6]));
        Vector3 LeftHolder_OutterPoint = OriginObject.transform.InverseTransformPoint(LHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[6]));//转化到骨网格的局部坐标
        Vector3 RightHolder_OutterPoint = OriginObject.transform.InverseTransformPoint(RHT.TransformPoint(RightHolderTool.GetComponent<MeshFilter>().mesh.vertices[0]));
        Vector3 RightHolder_OutterNor = RHT.TransformVector(new Vector3(1, 0, 0));
        List<Vector3> LeftHolder_OutterLinePoints;
        List<Vector3> RightHolder_OutterLinePoints;
        //切割左、右导板外表面  需要转化切割点
        //List<Vector3> Left_lineNor;
        //List<Vector3> Right_lineNor;
        Debug.Log("外网格切割开始");

        //Cut(ref out_ver, ref out_nor, ref out_tri, LeftHolder_InnerPoint, LeftHolder_InnerNor, out LeftHolder_OutterLinePoints, out Left_lineNor);
        //Debug.Log("LeftHolder_OutterLinePoints:" + LeftHolder_OutterLinePoints.Count);
        //List<int> tt = new List<int>();
        //GameObject o = new GameObject("test gameobject");
        //Mesh mesh = o.AddComponent<MeshFilter>().mesh;
        //o.AddComponent<MeshRenderer>();
        //mesh.vertices = out_ver.ToArray();
        //mesh.normals = out_nor.ToArray();
        //mesh.triangles = out_tri.ToArray();
        List<int> line1;//存储切面点的关系
        List<int> line2;
        List<Vector3> LeftHolder_OutterLineNor;
        List<Vector3> RightHolder_OutterLineNor; 
        WholeCut(LeftHolder_OutterPoint, -LeftHolder_OutterNor, ref out_ver, ref out_nor, ref out_tri, out LeftHolder_OutterLinePoints, out LeftHolder_OutterLineNor, out line1);
        //WholeCut(RightHolder_OutterPoint, RightHolder_OutterNor, ref out_ver, ref out_nor, ref out_tri, out RightHolder_OutterLinePoints, out RightHolder_OutterLineNor, out line2);
        Debug.Log("外表面已经切割完成!!!");
        Debug.Log(LeftHolder_OutterLinePoints);
        showMesh(out_ver, out_nor, out_tri);
        Debug.Log("完成Mesh显示");
        return;
        //Debug.Log(RightHolder_OutterLinePoints);//常出错 Right报空
        DrawLinePoints(OriginObject, LeftHolder_OutterLinePoints);//打印出来 发现有误
                                                                  //DrawLinePoints(OriginObject, RightHolder_OutterLinePoints);


        //缝合左右外表面
        //Debug.Log("=======原顶点数======：" + out_ver.Count);

        //对方向变量进行赋值****
        LeftHolder_up = LHT.TransformDirection(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[3] - LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[1]);
        LeftHolder_forward = LHT.TransformDirection(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[2] - LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[3]);
        LetfHolder_left = LHT.TransformDirection(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[3] - LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[5]);

        RightHolder_up = LHT.TransformDirection(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[1] - LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[3]);
        RightHolder_forward = LHT.TransformDirection(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[1] - LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[3]);
        RightHolder_left = LHT.TransformDirection(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[1] - LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[3]);

        Vector3 LeftHolder_OutterVector = OriginObject.transform.TransformVector(LHT.TransformDirection(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[3] - LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[1]));
        //Vector3 RightHolder_OutterVector = OriginObject.transform.TransformVector(LHT.TransformDirection(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[3] - LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[1]));
        Mend_Side(ref out_ver, ref out_nor, ref out_tri, true, true, LeftHolder_OutterLinePoints,
            LeftHolder_OutterLineNor, LeftHolder_OutterVector, line1);
        Debug.Log("==左外===");
        //Debug.Log(RightHolder_OutterLinePoints);
        //Debug.Log(RightHolder_OutterLineNor);
        //Debug.Log(line2);
        //Mend_Side(ref out_ver, ref out_nor, ref out_tri, false, true, RightHolder_OutterLinePoints,
        //    RightHolder_OutterLineNor, -RightHolder_OutterVector, line2);

        List<Vector3> vers;
        List<Vector3> nors;
        List<int> tris;
        //缝合内外连接处
        if (!MergeMeshInfo(out_ver, out_nor, out_tri, in_ver, in_nor, in_tri, out vers, out nors, out tris))
        {
            Debug.Log("融合失败！");
        }
        else
        {
            anchorPoints.left_in[0] += out_ver.Count;
            anchorPoints.left_in[1] += out_ver.Count;
            anchorPoints.left_in[2] += out_ver.Count;
            anchorPoints.left_in[3] += out_ver.Count;

            for (int i = 0; i < 4; ++i)
            {
                tris.Add(anchorPoints.left_out[i]);
                tris.Add(anchorPoints.left_in[i]);
                tris.Add(anchorPoints.left_in[(i + 1) % 3]);

                tris.Add(anchorPoints.left_out[i]);
                tris.Add(anchorPoints.left_out[(i + 1) % 3]);
                tris.Add(anchorPoints.left_in[(i + 1) % 3]);
            }
        }
        Debug.Log("==右外===");
        //Debug.Log("======现顶点数======：" + out_ver.Count);
        //showMesh(out_ver, out_nor, out_tri);
        //Debug.Log("外表面缝合结束");
        ////缝合左右内表面
        //Debug.Log("Inner_linePoints1" + (Inner_linePoints1 == null ? "是" : "不") + "为空");
        //Debug.Log("Inner_lineNor1" + (Inner_lineNor1 == null ? "是" : "不") + "为空");
        //Debug.Log("Inner_line1" + (Inner_line1 == null ? "是" : "不") + "为空");
        //Debug.Log("Inner_linePoints1.Count" + Inner_linePoints1.Count);
        Mend_Side(ref in_ver, ref in_nor, ref in_tri, true, false, Inner_linePoints1,
            Inner_lineNor1, -Inner_lineNor1[0], Inner_line1);
        //Debug.Log("左端缝合结束");
        //showMesh(in_ver, in_nor, in_tri);
        showMesh(vers, nors, tris);
        //Debug.Log("~~~~~~~~");
        //for (int i = 0; i < in_ver.Count / 3; ++i)
        //{
        //    Debug.Log("原:  " + OriginObject.transform.TransformPoint(OriginObject.GetComponent<MeshFilter>().mesh.vertices[i]) + "   现在:  " + OriginObject.transform.TransformPoint(in_ver[i]));
        //}
        //Debug.Log("~~~~~~~~");
        //Debug.Log("Inner_linePoints2.Count" + Inner_linePoints2.Count);
        //Mend_Side(ref in_ver, ref in_nor, ref in_tri, false, false, Inner_linePoints2,
        //    Inner_lineNor2, Inner_lineNor2[0], Inner_line2);
        //Debug.Log("右端缝合结束");

    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="ver">缝合网格的顶点</param>
    /// <param name="nor">缝合网格的法向量</param>
    /// <param name="tri">缝合网格的三角关系</param>
    /// <param name="IsLeft">缝合骨左侧还是右侧，true为左，false为右</param>
    /// <param name="IsOutter">缝合骨外侧 还是内侧，true为外，false为内</param>
    /// <param name="linePoints">切割面环上的点</param>
    /// <param name="lineNor">切割面环上的法向量</param>
    /// <param name="cutNor">切割面的法向量</param>
    /// <param name="line">切割面环上的点的顺序</param>
    void Mend_Side(ref List<Vector3> ver, ref List<Vector3> nor, ref List<int> tri, bool IsLeft, bool IsOutter, List<Vector3> linePoints, List<Vector3> lineNor, Vector3 cutNor, List<int> line)
    {
        //Debug.Log("lineNor: " + lineNor.Count);
        //Debug.Log("linePoints: " + linePoints.Count);
        List<Vector3> AnchorPoints = new List<Vector3>(4);
        Plane CutPlane = new Plane(lineNor[0], linePoints[0]);//用于做平面投影
        GameObject Ori = CutPosition.cutObject[CutPosition.cutObject.Count - 1];
        int meshCount = ver.Count;
        int lineNum = linePoints.Count;
        if (IsLeft)//左端角点
        {
            if (IsOutter)
            {
                Vector3 t1,t2,t3,t4;
                t1= LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[6];
                t1 = CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(LHT.TransformPoint(t1))) - LeftHolder_up * modelThickness + LeftHolder_forward * modelThickness;
                AnchorPoints.Add(t1);//加上四个角点

                t2 = LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[7];
                t2 = CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(LHT.TransformPoint(t2))) - LeftHolder_up * modelThickness - LeftHolder_forward * modelThickness;
                AnchorPoints.Add(t2);

                t3 = LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[5];
                t3 = CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(LHT.TransformPoint(t3))) + LeftHolder_up * modelThickness - LeftHolder_forward * modelThickness;
                AnchorPoints.Add(t3);
                
                t4 = LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[4];
                t4 = CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(LHT.TransformPoint(t4))) + LeftHolder_up * modelThickness + LeftHolder_forward * modelThickness;
                AnchorPoints.Add(t4);

                anchorPoints.left_out[0] = linePoints.Count + ver.Count + 4 - 1;
                anchorPoints.left_out[1] = anchorPoints.left_out[0] - 1;
                anchorPoints.left_out[2] = anchorPoints.left_out[0] - 2;
                anchorPoints.left_out[3] = anchorPoints.left_out[0] - 3;
            }
            else
            {
                //将四个点投影到切割面所在平面
                //linePoints.Reverse();
                AnchorPoints.Add(CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(LHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[4]))));//加上四个角点
                AnchorPoints.Add(CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(LHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[5]))));
                AnchorPoints.Add(CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(LHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[7]))));
                AnchorPoints.Add(CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(LHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[6]))));

                anchorPoints.left_in[0] = linePoints.Count + ver.Count;
                anchorPoints.left_in[1] = anchorPoints.left_in[0] + 1;
                anchorPoints.left_in[2] = anchorPoints.left_in[0] + 2;
                anchorPoints.left_in[3] = anchorPoints.left_in[0] + 3;
            }
        }
        else
        {
            if (IsOutter)//外表面
            {
                linePoints.Reverse();
                AnchorPoints.Add(CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(RHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[0]))));
                AnchorPoints.Add(CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(RHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[1]))));
                AnchorPoints.Add(CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(RHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[3]))));
                AnchorPoints.Add(CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(RHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[2]))));

            }
            else
            {
                linePoints.Reverse();
                AnchorPoints.Add(CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(RHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[2]))));
                AnchorPoints.Add(CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(RHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[3]))));
                AnchorPoints.Add(CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(RHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[1]))));
                AnchorPoints.Add(CutPlane.ClosestPointOnPlane(Ori.transform.InverseTransformPoint(RHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[0]))));

            }
        }

        //Debug.Log("11111");
        ////加入切环点集
        //ver.AddRange(linePoints);
        //nor.AddRange(lineNor);
        ////加入角点
        //ver.AddRange(AnchorPoints);
        //for (int i = 0; i < 4; i++)
        //    nor.Add(cutNor);
        line.Add(lineNum + 0); line.Add(lineNum + 1);//增加外环顺序
        line.Add(lineNum + 1); line.Add(lineNum + 2);
        line.Add(lineNum + 2); line.Add(lineNum + 3);
        line.Add(lineNum + 3); line.Add(lineNum + 0);
        linePoints.AddRange(AnchorPoints);
        //Debug.Log("22222");
        //耳切法缝合切面
        ITriangulator triangulator = new Triangulator(linePoints.ToArray(), line.ToArray(), cutNor);
        //Debug.Log("333333");
        int[] newEdges, newTri, newTrangleEdges;
        triangulator.Fill(out newEdges, out newTri, out newTrangleEdges);
        //if (!IsLeft)
        //{
        //    List<int> t = new List<int>(newTri);
        //    t.Reverse();
        //    newTri = t.ToArray();
        //}
        //List<Vector3> anchorNor = new List<Vector3>(4);
        for (int i = 0; i < 4; i++)
            lineNor.Add(lineNor[0]);
        //showMesh(linePoints, lineNor, new List<int>(newTri));
        Debug.Log("44444");
        //将切面加入主网格
        ver.AddRange(linePoints);
        nor.AddRange(lineNor);
        //nor.AddRange(anchorNor);
        //if (ver.Count - linePoints.Count == meshCount)
        //    Debug.Log("OKKKKKKK");
        for (int i = 0; i < newTri.Length; i += 3)
        {
            tri.Add(newTri[i + 0] + meshCount);
            tri.Add(newTri[i + 1] + meshCount);
            tri.Add(newTri[i + 2] + meshCount);
        }
        //for (int i = 0; i < newTri.Length; i++)
        //    tri.Add(newTri[i] + meshCount);
        //showMesh(ver, nor, tri);
        Debug.Log("缝合函数调用结束");
        //if(!IsOutter && IsLeft)
        //    linePoints.Reverse();//无关
    }
    
    
    /// <summary>
    /// 切割(不缝合版)
    /// 用于获取表面网格 且仅保留最多的那部分网格
    /// </summary>
    //private void Cut(GameObject thing, Vector3 localPoint, Vector3 localSplitPlaneNormal, out List<Vector3> linePoints)
    private void Cut(ref List<Vector3> vers, ref List<Vector3> nors, ref List<int> tris, Vector3 localPoint, Vector3 localSplitPlaneNormal, out List<Vector3> linePoints, out List<Vector3> lineNors)
    {
        //==================
        Debug.Log("切割前三角数:" + tris.Count);
        int startTime = System.Environment.TickCount;
        linePoints = null;
        lineNors = new List<Vector3>();
        //localPoint = thing.transform.InverseTransformPoint(localPoint);
        //localSplitPlaneNormal = thing.transform.InverseTransformVector(localSplitPlaneNormal);
        //localSplitPlaneNormal = localSplitPlaneNormal.normalized;


        //根据本切割工具网格顶点形成切割面
        //注意:传入的点已经是局部坐标


        //根据切割面，把顶点分成两部分

        //Vector3[] vertices = thing.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] vertices = vers.ToArray();
        Debug.Log("被切割物体的顶点数：" + vertices.Length);
        //Vector3[] normals = thing.GetComponent<MeshFilter>().mesh.normals;
        Vector3[] normals = nors.ToArray();

        bool[] vertexAbovePlane;
        vertexAbovePlane = new bool[vertices.Length];
        int[] oldToNewVertexMap = new int[vertices.Length];

        List<Vector3> newVertices1 = new List<Vector3>(vertices.Length);//为了避免数组多次动态增加容量，给最大值
        Debug.Log("newVertices1:" + newVertices1.Count);

        List<Vector3> newVertices2 = new List<Vector3>(vertices.Length);
        List<Vector3> newNormals1 = new List<Vector3>(normals.Length);
        List<Vector3> newNormals2 = new List<Vector3>(normals.Length);

        //int m = 0, n = 0;
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
                //++m;
            }
            else
            {
                // Assign vertex to 2
                oldToNewVertexMap[i] = newVertices2.Count;
                newVertices2.Add(vertex);
                newNormals2.Add(normals[i]);
                //++n;
            }
            Debug.Log(abovePlane);

        }
        if (newVertices1.Count == 0 || newVertices2.Count == 0)
        {
            //Debug.Log("m:" + m + "   n:" + n);
            Debug.Log("nothing be cut,over");
            linePoints = null;
            lineNors = null;
            return;
        }


        //仅记录较多的网格数据
        //List<Vector3> newVer;
        //List<Vector3> newNor;
        //List<int> newTri;
        //if (newVertices1.Count > newVertices2.Count)
        //{
        //    newVer = new List<Vector3>(newVertices1);
        //}
        //else
        //{
        //    newVer = new List<Vector3>(newVertices2);
        //}
        //分三角面
        //int[] indices = thing.GetComponent<MeshFilter>().mesh.triangles;
        int[] indices = tris.ToArray();
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
        //获取最多的网格数据模型
        Vector3 nor;
        if (newVertices1.Count >= newVertices2.Count)
        {
            nor = -localSplitPlaneNormal;//与切面法向量相反
            vers = newVertices1;
            nors = newNormals1;
            tris = newTriangles1;
        }
        else
        {
            nor = localSplitPlaneNormal;
            vers = newVertices2;
            nors = newNormals2;
            tris = newTriangles2;
        }

        //缝合切面

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

            }

        }




        if (points.Count > 0)             //调用triangulator
        {
            Debug.Log("=========切面节点数:==============" + points.Count);

            for (int i = 0; i < points.Count; i++)
            {
                //newNormals1.Add(normalA);
                //newNormals2.Add(normalB);
                //newNor.Add(nor);
                lineNors.Add(nor);
            }

            linePoints = new List<Vector3>(points);//复制
                                                   //lineNors = newNor;
                                                   // Add the new triangles
                                                   //int newTriangleCount = newTriangles.Length / 3;

        }
        earCutting(points, lineNors);
        DrawLinePoints(CutPosition.cutObject[0], points);
        //*/

        Debug.Log("切割后三角数:" + tris.Count);
        Debug.Log("cutEdges.Count/2:" + cutEdges.Count / 2);
        Debug.Log("points.Count:" + points.Count);
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

    private void IsolateMesh(GameObject beIsolatedThing)
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
        //Debug.Log("=====结果=====");
        //Debug.Log("分离结果个数:" + union.getCount());
        //Debug.Log("=====结果=====");
        int num = 0;

        int count = union.getCount();

        if (count >= 2)              //分离
        {
            ////清理标记
            //GameObject[] cutplanemark = GameObject.FindGameObjectsWithTag("temp_cutplane_mark");
            //for (int i = 0; i < cutplanemark.Length; i++)
            //{
            //    DestroyImmediate(cutplanemark[i]);//删除标记
            //}

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
            //先清空当前存储的分离子物体数组
            for (int i = 0; i < RollBackHelper.CutThings.Length; i++)
            {
                RollBackHelper.CutThings[i] = null;//静态无法重新分配内存来清空
            }

            for (int i = 0; i < count; i++)//
            {
                if (newTrianglesArrays[i].Count < tri.Length / 500) continue; //小于原网格1%的小碎片忽视掉（但可能有些小碎片是有用的）
                //GameObject part = new GameObject(beIsolatedThing.name + i.ToString());
                ///<summary>需要删除原物体碰撞器 再重新生成</summary>
                GameObject part = Instantiate(beIsolatedThing, null);
                DestroyImmediate(part.GetComponent<MeshCollider>());//清除碰撞器
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
                part.AddComponent<MeshCollider>();//重新生成MeshCollider
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
                //part.tag = "Objects";//均置为Objects
                //part.AddComponent<MeshCollider>();
                //part.GetComponent<MeshCollider>().convex = true;
                //part.GetComponent<MeshCollider>().cookingOptions = MeshColliderCookin`gOptions.InflateConvexMesh;

                RollBackHelper.CutThings[i] = part;//获取被分离的子物体 用于回滚
            }
            beIsolatedThing.SetActive(false);
        }
        else
        {
            int endTime = System.Environment.TickCount;
            Debug.Log("分离耗时：" + (endTime - startTime));
        }
        Debug.Log("===num===");
        Debug.Log("num: " + num);
        Debug.Log("===num===");
        if (num > 1)
        {
            //清理标记
            GameObject[] cutplanemark = GameObject.FindGameObjectsWithTag("temp_cutplane_mark");
            for (int i = 0; i < cutplanemark.Length; i++)
            {
                DestroyImmediate(cutplanemark[i]);//删除标记
            }
        }

        ////清理所有被选中物体 因为可能存在复制问题 导致标签重复
        //GameObject[] ChosenObjects = GameObject.FindGameObjectsWithTag("ChosenObject");
        //for (int i = 0; i < ChosenObjects.Length; i++)
        //    ChosenObjects[i].tag = "Objects";
    }
    float CaculateX(Vector3 OT, Vector3 l)
    {
        float x;
        float a = l.x * l.x + l.y * l.y + l.z * l.z;
        float b = 2 * (OT.x * l.x + OT.y * l.y + OT.z * l.z);
        float c = OT.x * OT.x + OT.y * OT.y + OT.z * OT.z - distance * distance;
        float x1 = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
        float x2 = (-b - Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
        if (x1 >= 0) { x = x1; }
        else if (x2 >= 0) { x = x2; }
        else
        {
            x = 0;
            Debug.Log("xxxxxx");
        }
        return x;
    }
    private void OnDrawGizmos()
    {
        GameObject o = CutPosition.cutObject[CutPosition.cutObject.Count - 1];
        //for (int i = 0; i < CutPosition.cutPoint.Count; i++)
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawSphere(CutPosition.cutObject[i].transform.TransformPoint(CutPosition.cutPoint[i]), 0.008f);
        //}
        Gizmos.color = Color.green;
        //Gizmos.DrawLine(CutPosition.cutObject[0].transform.TransformPoint(CutPosition.cutPoint[0]),
        //    CutPosition.cutObject[1].transform.TransformPoint(CutPosition.cutPoint[1]));
        //Gizmos.DrawSphere((CutPosition.cutObject[0].transform.TransformPoint(CutPosition.cutPoint[0]) + CutPosition.cutObject[0].transform.TransformPoint(CutPosition.cutPoint[1])) / 2, 0.001f);
        //Gizmos.DrawSphere(LeftHolderTool.transform.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[0]), 0.0008f);
        //Gizmos.DrawSphere(LeftHolderTool.transform.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[1]), 0.0008f);
        //Gizmos.DrawSphere(LeftHolderTool.transform.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[2]), 0.0008f);
        //Gizmos.DrawSphere(LeftHolderTool.transform.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[3]), 0.0008f);
        //Gizmos.DrawSphere(LHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().sharedMesh.vertices[4]), 0.008f);
        //Gizmos.DrawSphere(LeftHolderTool.transform.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[5]), 0.0008f);
        //Gizmos.DrawSphere(LeftHolderTool.transform.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().sharedMesh.vertices[6]), 0.0008f);
        //Gizmos.DrawSphere(LeftHolderTool.transform.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[7]), 0.0008f);
        //Gizmos.DrawSphere(LeftHolderTool.transform.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[], 0.0008f))
        //Gizmos.DrawSphere(CutPosition.cutObject[0].transform.TransformPoint(CutPosition.cutPoint[0]), 0.008f);
        //Gizmos.DrawSphere(CutPosition.cutObject[0].transform.TransformPoint(CutPosition.cutPoint[1]), 0.008f);
        //Gizmos.color = Color.blue;
        //Gizmos.DrawSphere(RHT.TransformPoint(RightHolderTool.GetComponent<MeshFilter>().mesh.vertices[4]), 0.008f);
        //Debug.Log("第一次画切点");
        if (DrawTag)
        {
            //Gizmos.color = Color.red;
            ////Gizmos.DrawSphere(CutPosition.cutPoint[0], 0.008f);
            ////Gizmos.DrawSphere(CutPosition.cutPoint[1], 0.008f);
            ////Gizmos.DrawSphere(LHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().sharedMesh.vertices[0]), 0.0008f);
            ////Gizmos.DrawRay(
            //// LHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[4]),
            //// LHT.TransformDirection(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[2] - LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[3]));///方向应该采用Direct来完成转换
            ////Debug.Log("射线已经绘制");
            ////Gizmos.DrawSphere(LHT.TransformPoint(LeftHolderTool.GetComponent<MeshFilter>().mesh.vertices[4]), 0.008f);

            //Gizmos.DrawSphere(CutPosition.cutObject[CutPosition.cutObject.Count - 1].transform.TransformPoint(testPoints[0]), 0.0008f);
            //Gizmos.DrawSphere(CutPosition.cutObject[CutPosition.cutObject.Count - 1].transform.TransformPoint(testPoints[2]), 0.0008f);

            //Gizmos.color = Color.blue;
            //Gizmos.DrawSphere(CutPosition.cutObject[CutPosition.cutObject.Count - 1].transform.TransformPoint(testPoints[1]), 0.001f);
            //Gizmos.DrawSphere(CutPosition.cutObject[CutPosition.cutObject.Count - 1].transform.TransformPoint(testPoints[3]), 0.001f);

            //Gizmos.color = Color.green;
            //Gizmos.DrawSphere(CutPosition.cutPoint[CutPosition.cutPoint.Count - 1], 0.0008f);
        }
        //GameObject o = GameObject.FindGameObjectWithTag("ChosenObject");
        //Gizmos.DrawLine(o.transform.TransformPoint(o.GetComponent<MeshFilter>().mesh.vertices[2]),
        //    o.transform.TransformVector(o.GetComponent<MeshFilter>().mesh.normals[1000])*10+ o.transform.TransformPoint(o.GetComponent<MeshFilter>().mesh.vertices[1000]));
        //Debug.Log("画线！！");



    }
    /// <summary>
    /// 标记环状切线
    /// </summary>
    /// <param name="Ori">父物体</param>
    /// <param name="linPoints">切面交点</param>
    void DrawLinePoints(GameObject Ori, List<Vector3> linPoints)
    {
        Debug.Log("linPoints顶点数:" + linPoints.Count);
        LineRenderer lines = new LineRenderer();
        GameObject line = new GameObject("line");

        line.transform.SetParent(Ori.transform);
        line.transform.position = Vector3.zero;
        line.transform.rotation = Quaternion.Euler(Vector3.zero);

        lines = line.AddComponent<LineRenderer>();
        lines.material = Mark_Material;
        line.GetComponent<LineRenderer>().useWorldSpace = false;
        //line1.transform.localScale.Set(1, 1, 1);
        line.transform.localScale = new Vector3(1f, 1f, 1f);
        //line1.GetComponent<>()
        lines.positionCount = linPoints.Count + 1;
        lines.startWidth = 0.0003f;
        lines.endWidth = 0.0003f;
        //设置渲染顶点
        for (int i = 0; i < linPoints.Count; i++)
            lines.SetPosition(i, linPoints[i]);
        lines.SetPosition(linPoints.Count, linPoints[0]);
    }
    void showMesh(List<Vector3> ver, List<Vector3> nor, List<int> tri)
    {
        string name = "tempShow";
        showMesh(ver, nor, tri, name);
    }
    void showMesh(List<Vector3> ver, List<Vector3> nor, List<int> tri, string name)
    {
        GameObject tempShow = new GameObject(name);
        tempShow.AddComponent<MeshRenderer>();
        tempShow.AddComponent<MeshCollider>();
        tempShow.AddComponent<Choosable>();
        tempShow.AddComponent<Interactable>();
        Mesh mesh = tempShow.AddComponent<MeshFilter>().mesh;
        mesh.vertices = ver.ToArray();
        mesh.normals = nor.ToArray();
        mesh.triangles = tri.ToArray();
    }
    void earCutting(List<Vector3> ver, List<Vector3> nors)
    {
        if (numCounter < 2)//仅缝合最后一个切面 linePoints区域
        {
            numCounter++;
            return;
        }

        //List<int> tris = new List<int>();
        List<int> edges = new List<int>();
        //List<Vector3> nors = new List<Vector3>();
        for (int i = 0; i < ver.Count - 1; i++)
        {
            edges.Add(i);
            edges.Add(i + 1);
        }
        edges.Add(ver.Count - 1);
        edges.Add(0);

        //for(int i=0;i<edges.Count/2;i++)
        //{
        //    Debug.Log("第" + i + "条边:  " + edges[i * 2] + "  " + edges[2 * i + 1]);
        //}
        ITriangulator triangulator = new Triangulator(ver.ToArray(), edges.ToArray(), nors[0]);
        int[] newEdges, newTri, newTrangleEdges;
        triangulator.Fill(out newEdges, out newTri, out newTrangleEdges);

        GameObject tempShow = new GameObject("tempShow" + numCounter++);
        tempShow.AddComponent<MeshRenderer>();
        tempShow.AddComponent<MeshFilter>();
        tempShow.AddComponent<MeshCollider>();
        tempShow.AddComponent<Choosable>();
        tempShow.AddComponent<Interactable>();
        Mesh mesh = tempShow.GetComponent<MeshFilter>().mesh;
        mesh.vertices = ver.ToArray();
        mesh.normals = nors.ToArray();
        mesh.triangles = newTri;
    }
    /// <summary>
    /// 暂做测试使用  缝合函数
    /// </summary>
    /// <param name="theMesh"></param>
    /// <param name="cutPointUp"></param>
    /// <param name="cutPointDown"></param>
    /// <param name="leftOrRight"></param>
    /// <returns></returns>
    protected ArrayList Mend(Mesh theMesh, ArrayList cutPointUp, ArrayList cutPointDown, string leftOrRight)
    {
        ArrayList vertices = new ArrayList(theMesh.vertices);           // 根据切割点，添加顶点连接三角面形成导板表面
        ArrayList normal = new ArrayList(theMesh.normals);
        ArrayList newTriangle = new ArrayList(theMesh.triangles);
        ArrayList angularPoints = new ArrayList();
        Plane leftPlane = backPlaneOfOsteotome;//因为赋值语句在switch中编译不让使用未赋值的变量，随便赋个值先
        Plane rightPlane = forwardPlaneOfOsteotome;
        Vector3 pointsNormal;
        switch (leftOrRight)
        {
            case "left":
                if (inner)
                {
                    leftPlane = backPlaneOfOsteotome;
                    rightPlane = forwardPlaneOfOsteotome;
                }
                else
                {
                    leftPlane = outsideForwardPlaneOfOsteotome;
                    rightPlane = outsideBackPlaneOfOsteotome;
                }

                break;

            case "right":
                if (inner)
                {
                    leftPlane = forwardPlaneOfOsteotome;
                    rightPlane = backPlaneOfOsteotome;
                }
                else
                {
                    leftPlane = outsideBackPlaneOfOsteotome;
                    rightPlane = outsideForwardPlaneOfOsteotome;
                }

                break;


        }
        Debug.Log("cut points up=" + cutPointUp.Count + "cut points down=" + cutPointDown.Count);
        vertices.Add(leftPlane.ClosestPointOnPlane((Vector3)vertices[(int)cutPointUp[0]]));//距离切割平面最近的点
        pointsNormal = Vector3.Cross((Vector3)vertices[(int)cutPointUp[1]] - (Vector3)vertices[(int)cutPointUp[0]], (Vector3)vertices[vertices.Count - 1] - (Vector3)vertices[(int)cutPointUp[0]]); //确定该面顶点的法向量
        normal.Add(pointsNormal);
        cutPointUp.Insert(0, vertices.Count - 1);
        cutPointDown.Add(vertices.Count - 1);
        vertices.Add(rightPlane.ClosestPointOnPlane((Vector3)vertices[(int)cutPointDown[0]]));
        normal.Add(pointsNormal);
        cutPointUp.Add(vertices.Count - 1);
        cutPointDown.Insert(0, vertices.Count - 1);
        //上
        for (int i = 0; i < cutPointUp.Count - 1; i++)
        {
            vertices.Add((Vector3)vertices[(int)cutPointUp[i]]);
            normal.Add(pointsNormal);
            vertices.Add(abovePlaneOfOsteotome.ClosestPointOnPlane((Vector3)vertices[(int)cutPointUp[i]]));
            normal.Add(pointsNormal);
            if (i == 0) { angularPoints.Add(vertices.Count - 1); }
            Debug.Log("mending~");
            newTriangle.Add(vertices.Count - 2);
            newTriangle.Add(vertices.Count - 1);
            newTriangle.Add(vertices.Count);

            newTriangle.Add(vertices.Count);
            newTriangle.Add(vertices.Count - 1);
            newTriangle.Add(vertices.Count + 1);
        }
        vertices.Add((Vector3)vertices[(int)cutPointUp[cutPointUp.Count - 1]]);
        normal.Add(pointsNormal);
        vertices.Add(abovePlaneOfOsteotome.ClosestPointOnPlane((Vector3)vertices[(int)cutPointUp[cutPointUp.Count - 1]]));
        normal.Add(pointsNormal);
        angularPoints.Add(vertices.Count - 1);
        //下
        for (int i = 0; i < cutPointDown.Count - 1; i++)
        {
            vertices.Add((Vector3)vertices[(int)cutPointDown[i]]);
            normal.Add(pointsNormal);
            vertices.Add(underPlaneOfOsteotome.ClosestPointOnPlane((Vector3)vertices[(int)cutPointDown[i]]));
            normal.Add(pointsNormal);
            if (i == 0) { angularPoints.Add(vertices.Count - 1); }
            newTriangle.Add(vertices.Count - 2);
            newTriangle.Add(vertices.Count - 1);
            newTriangle.Add(vertices.Count);

            newTriangle.Add(vertices.Count);
            newTriangle.Add(vertices.Count - 1);
            newTriangle.Add(vertices.Count + 1);
        }
        vertices.Add((Vector3)vertices[(int)cutPointDown[cutPointDown.Count - 1]]);
        normal.Add(pointsNormal);
        vertices.Add(underPlaneOfOsteotome.ClosestPointOnPlane((Vector3)vertices[(int)cutPointDown[cutPointDown.Count - 1]]));
        normal.Add(pointsNormal);
        angularPoints.Add(vertices.Count - 1);




        theMesh.vertices = (Vector3[])vertices.ToArray(typeof(Vector3));
        theMesh.normals = (Vector3[])normal.ToArray(typeof(Vector3));
        theMesh.triangles = (int[])newTriangle.ToArray(typeof(int));
        return angularPoints;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="localPoint"></param>
    /// <param name="localSplitPlaneNormal"></param>
    /// <param name="vers"></param>
    /// <param name="nors"></param>
    /// <param name="tris"></param>
    /// <param name="linePoints"></param>
    /// <param name="lineNor"></param>
    /// <param name="line"></param>
    private void WholeCut(Vector3 localPoint, Vector3 localSplitPlaneNormal, ref List<Vector3> vers, ref List<Vector3> nors, ref List<int> tris, out List<Vector3> linePoints, out List<Vector3> lineNor, out List<int> line)
    {
        //==================
        //根据本切割工具网格顶点形成切割面

        localSplitPlaneNormal.Normalize();//单位化
        line = null;
        lineNor = null;
        //根据切割面，把顶点分成两部分


        Vector3[] vertices = vers.ToArray();
        Debug.Log("被切割物体的顶点数：" + vertices.Length);
        Vector3[] normals = nors.ToArray();

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
            linePoints = null;
            return;
        }
        //分三角面

        int[] indices = tris.ToArray();
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
        //重新保存mesh网格数据
        bool tag = newVertices1.Count > newVertices2.Count;
        vers = tag ? newVertices1 : newVertices2;
        nors = tag ? newNormals1 : newNormals2;
        tris = tag ? newTriangles1 : newTriangles2;


        ///<summary>cutEdges中存放交点坐标
        ///<param name="cutEdges">交点坐标</para>
        ///</summary>
        //缝合切面
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
        line = new List<int>(outline);
        //添加法向量
        lineNor = new List<Vector3>();
        for (int i = 0; i < points.Count; i++)
            lineNor.Add(localSplitPlaneNormal);

        linePoints = new List<Vector3>(points);



    }
    /// <summary>
    /// 合并内外网格
    /// </summary>
    /// <param name="vers1"></param>
    /// <param name="nors1"></param>
    /// <param name="tris1"></param>
    /// <param name="vers2"></param>
    /// <param name="nors2"></param>
    /// <param name="tris2"></param>
    /// <param name="vers"></param>
    /// <param name="nors"></param>
    /// <param name="tris"></param>
    /// <returns></returns>
    bool MergeMeshInfo(List<Vector3> vers1,List<Vector3> nors1,List<int> tris1, List<Vector3> vers2, List<Vector3> nors2, List<int> tris2,out List<Vector3> vers,out List<Vector3> nors,out List<int> tris)
    {
        vers = null;
        nors = null;
        tris = null;
        if (vers1.Count == 0 || vers2.Count == 0)
            return false;
        vers = new List<Vector3>(vers1);
        nors = new List<Vector3>(nors1);
        tris = new List<int>(tris1);

        vers.AddRange(vers2);
        nors.AddRange(nors2);
        int n = vers1.Count;
        for(int i=0;i<tris2.Count;i+=3)
        {
            tris.Add(tris2[i] + n);
            tris.Add(tris2[i + 1] + n);
            tris.Add(tris2[i + 2] + n);
        }

        return true;
    }

}
