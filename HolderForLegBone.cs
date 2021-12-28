using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
/// <summary>
/// 为腿骨生成双导板切割工具
/// </summary>
public class HolderForLegBone : MonoBehaviour {
    static bool GenerateTag = true;
    public Material test;
    List<Vector3> GizomsPoint = new List<Vector3>();
    private int numCounter = 0;
    public float distance;         //表面网格提取范围
    Vector3[] leftFace = new Vector3[4], rightFace = new Vector3[4];
	Mesh innerFace;
	Mesh outterFace;
	GameObject Lholder, Rholder;
    public Material Mark_Material;//标记材质
    //记录新的切割面的环
    //左侧
    private List<Vector3> Inner_linePoints1;
    List<Vector3>  Inner_lineNor1;
    List<int> Inner_line1;
    //右侧
    private List<Vector3> Inner_linePoints2;
    List<Vector3>  Inner_lineNor2;
    List<int> Inner_line2;

    float thickness = 0.004f;


    List<Vector3> cutPointsOut2, cutNorsOut2;
    List<int> cutLinesOut2;

    /// <summary>
    /// 两次切割产生的环上的点
    /// </summary>
    List<Vector3> firstCutLinePoints, secondCutLinePoints;
    /// <summary>
    /// 两次切割产生的环上的法向量
    /// </summary>
    List<Vector3> firstCutLineNormals, secondCutLineNormals;
    /// <summary>
    /// 两次切割产生的环上的三角面
    /// </summary>
    List<int> firstCutLineTriangles, secondCutLineTriangles;

    // Use this for initialization
    void Start () {
        GenerateTag = true;
        innerFace = new Mesh();
        outterFace = new Mesh();
		//innerFace.Clear();  
		//outterFace.Clear();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Y) || SteamVR_Actions.default_Teleport.GetStateDown(SteamVR_Input_Sources.Any))
        {
            //if (GenerateTag == false)
            //{
            //    return;
            //}
            //else
            //{
            //    GenerateTag = false;
            //}
            Debug.Log("CutPosition.cutAnchorPoints[0]=" + CutPosition.cutAnchorPoints[0].Count);
            Debug.Log("CutPosition.cutAnchorPoints[1]=" + CutPosition.cutAnchorPoints[1].Count);
            //CutPosition.cutObject[0].SetActive(true);
            //CutPosition.cutObject[0].name = "kkkkk";
            //CutPosition.cutObject[0].GetComponent<MeshRenderer>().material = test;//验证cutObject是否正确 正确
            //记录新的切割环
            //Debug.Log("*************" + "CutPosition.cutAnchorPoints.Count: " + CutPosition.cutAnchorPoints.Count);
            //precedure(ref Inner_linePoints1, ref Inner_linePoints2, ref Inner_lineNor1, ref Inner_lineNor2);//预处理 获得未缝合网格数据
            preprocessing();
            //return;
            //tempPreprocessing();
            //导板到指定位置
            Vector3 CP1 = CutPosition.cutPoint[CutPosition.cutPoint.Count - 2];
            Vector3 CP2 = CutPosition.cutPoint[CutPosition.cutPoint.Count - 1];
            //Debug.Log("cutobject的个数：" + CutPosition.cutObject.Count);
            transform.GetChild(0).position = CutPosition.cutObject[CutPosition.cutObject.Count - 2].transform.TransformPoint(CP1);//转化为cutTing的坐标系
            transform.GetChild(1).position = CutPosition.cutObject[CutPosition.cutObject.Count - 2].transform.TransformPoint(CP2);//转化为cutTing的坐标系
            //调整角度
            int index = 0;
            Vector3 point = CutPosition.cutObject[index].transform.TransformPoint(CutPosition.cutPoint[index]);
            Vector3 normal = CutPosition.cutObject[index].transform.TransformVector(CutPosition.cutPlaneNormal[index]);
            Plane cutPlane = new Plane(normal, point);
            Vector3 forward = -CutPosition.cutObject[index].transform.TransformVector(CutPosition.cutPlaneNormal[index]);
            Quaternion NextRot = Quaternion.LookRotation(forward, Vector3.Cross(forward, cutPlane.ClosestPointOnPlane(transform.position) - point));
            transform.GetChild(index).rotation = NextRot;
            index = 1;
            point = CutPosition.cutObject[index].transform.TransformPoint(CutPosition.cutPoint[index]);
            normal = CutPosition.cutObject[index].transform.TransformVector(CutPosition.cutPlaneNormal[index]);
            cutPlane = new Plane(normal, point);
            forward = CutPosition.cutObject[index].transform.TransformVector(CutPosition.cutPlaneNormal[index]);
            NextRot = Quaternion.LookRotation(forward, Vector3.Cross(forward, cutPlane.ClosestPointOnPlane(transform.position) - point));
            transform.GetChild(index).rotation = NextRot;

            //return;
            for (int i = 0; i < 4; ++i)
			{
				leftFace[i] = transform.GetChild(0).GetComponent<MeshFilter>().mesh.vertices[i];
			}
			//右
			for (int i = 0; i < 4; ++i)
			{
				leftFace[i] = transform.GetChild(0).GetComponent<MeshFilter>().mesh.vertices[i + 4];
			}
			//获取左右导板数据点
			Lholder = transform.GetChild(0).gameObject;
			Rholder = transform.GetChild(1).gameObject;
            //开始生成
            //GenerateInnerFace();
            GenerateLeftFace();
            //return;
            GenerateRightFace();
            //List<Vector3> v1 = new List<Vector3>(innerFace.vertices), n1 = new List<Vector3>(innerFace.normals);
            //List<int> t1 = new List<int>(innerFace.triangles);
            //List<Vector3> v2 = new List<Vector3>(outterFace.vertices), n2 = new List<Vector3>(outterFace.normals);
            //List<int> t2 = new List<int>(outterFace.triangles);
            //v1.AddRange(v2);
            //n1.AddRange(n2);
            //t1.AddRange(t2);
            //MeshMerge_A_2_B(ref v1, ref n1, ref t1, ref v2, ref n2, ref t2);
            //showMesh(v1, n1, t1);
            //Debug.Log("ver=" + innerFace.vertices.Length);
            //Debug.Log("nor=" + innerFace.normals.Length);
            GameObject Holder = new GameObject("Holder");
            Holder.AddComponent<MeshFilter>().mesh = innerFace;
            Holder.transform.position = CutPosition.cutObject[CutPosition.cutObject.Count - 2].transform.position;
            Holder.transform.rotation = CutPosition.cutObject[CutPosition.cutObject.Count - 2].transform.rotation;
            Holder.AddComponent<MeshRenderer>().material = test;
            Holder.tag = "Objects";
            Holder.AddComponent<MeshCollider>();
            Holder.AddComponent<Choosable>();
            Holder.AddComponent<Interactable>();
        }
	}
    /// <summary>
    /// 对innerFace = CutPosition.TempMeshSaver进行预处理 去掉缝合的两侧表面
    /// 得到骨骼网格
    /// </summary>
    private void preprocessing()//重新执行切割操作 获取网格数据
    {
        //根据第一第二次的切割位置进行预切割 获取网格
        Mesh mesh = CutPosition.cutObject[0].GetComponent<MeshFilter>().mesh;
        //showMesh(new List<Vector3>(mesh.vertices), new List<Vector3>(mesh.normals), new List<int>(mesh.triangles), "切割前");
        List<Vector3> ver = new List<Vector3>(mesh.vertices), nor = new List<Vector3>(mesh.normals);
        List<int> tri = new List<int>(mesh.triangles);
        //Transform t = CutPosition.cutObject[0].transform;
        //WholeCut(CutPosition.cutPoint[0], CutPosition.cutPlaneNormal[0], ref ver, ref nor, ref tri, out firstCutLinePoints, out firstCutLineNormals, out firstCutLineTriangles);
        //WholeCut(CutPosition.cutPoint[1], CutPosition.cutPlaneNormal[1], ref ver, ref nor, ref tri, out secondCutLinePoints, out secondCutLineNormals, out secondCutLineTriangles);
        GetMesh(ref ver, ref nor, ref tri);
        innerFace = new Mesh();
        innerFace.vertices = ver.ToArray();
        innerFace.normals = nor.ToArray();
        tri.Reverse();
        innerFace.triangles = tri.ToArray();
        tri.Reverse();
        //向外扩张节点
        for (int i = 0; i < ver.Count; ++i)
        {
            ver[i] += nor[i] * thickness;
        }
        outterFace = new Mesh();
        outterFace.vertices = ver.ToArray();
        outterFace.normals = nor.ToArray();
        outterFace.triangles = tri.ToArray();
        //showMesh(ver, nor, tri, "外表面");
        //showMesh(ver, nor, tri,"切割后");
        //Debug.Log("顶点数：" + ver.Count);
        //Debug.Log("三角面数：" + tri.Count);
    }
    /// <summary>
    /// 临时处理
    /// 借助不缝合的局部切割 先生成网格再说
    /// </summary>
    //void tempPreprocessing()
    //{
    //    innerFace = new Mesh();//获取未缝合的网格
    //    List<Vector3> in_ver = new List<Vector3>(CutPosition.TempMeshSaver.vertices), in_nor = new List<Vector3>(CutPosition.TempMeshSaver.normals);
    //    List<int> in_tri = new List<int>(CutPosition.TempMeshSaver.triangles);
    //    in_tri.Reverse();
    //    innerFace.vertices = in_ver.ToArray();
    //    innerFace.normals = in_nor.ToArray();
    //    innerFace.triangles = in_tri.ToArray();
    //    //左侧
    //    Inner_linePoints1 = CutPosition.CutPoints[0];
    //    Inner_line1 = CutPosition.CutLine[0];
    //    Inner_lineNor1 = CutPosition.CutNors[0];
    //    //右侧
    //    Inner_linePoints2 = CutPosition.CutPoints[1];
    //    Inner_line2 = CutPosition.CutLine[1];
    //    Inner_lineNor2 = CutPosition.CutNors[1];
    //    //处理内网格 生成内网格
    //    List<Vector3> out_ver = new List<Vector3>(CutPosition.TempMeshSaver.vertices);
    //    List<Vector3> out_nor = new List<Vector3>(CutPosition.TempMeshSaver.normals);
    //    List<int> out_tri = new List<int>(CutPosition.TempMeshSaver.triangles);
    //    //向外扩张节点
    //    for (int i = 0; i < out_ver.Count; ++i)
    //    {
    //        out_ver[i] += out_nor[i] * thickness;
    //    }
    //    outterFace = new Mesh();
    //    outterFace.vertices = out_ver.ToArray();
    //    outterFace.normals = out_nor.ToArray();
    //    outterFace.triangles = out_tri.ToArray();
    //    //MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref in_ver, ref in_nor, ref in_tri);
    //    //showMesh(out_ver, out_nor, out_tri);
    //    //showMesh(in_ver, in_nor, in_tri);
    //    //return;
    //}
    void CopyMesh(ref Mesh mesh, out Mesh cp)
    {
        cp = new Mesh
        {
            vertices = mesh.vertices,
            normals = mesh.normals,
            triangles = mesh.triangles
        };

    }
	/// <summary>
	/// 内表面生成 基于骨骼网格生成
	/// </summary>
	void GenerateLeftFace1()
    {
        List<Vector3> lineCirclesPointsL, lineCirclesPointsR;
        List<Vector3> allAhcorPoints = new List<Vector3>(16);
        //先缝合内表面
        List<Vector3> Inner_linePoints = new List<Vector3>(Inner_linePoints1);//获取环上的点
        List<Vector3> Inner_lineNors = new List<Vector3>(Inner_lineNor1);//获取环上的点
        List<int> Inner_line = new List<int>(Inner_line1);//获取环上的轮廓
        List<Vector3> in_ver = new List<Vector3>(innerFace.vertices), in_nor = new List<Vector3>(innerFace.normals);
        List<int> in_tri = new List<int>(innerFace.triangles);
        //in_tri.Reverse();
        Transform trans = CutPosition.cutObject[CutPosition.cutObject.Count - 1].transform;
        //获取左侧4个角点 逆时针顺序
        //Debug.Log("Lholder:" + Lholder == null ? "空" : "非空");
        Vector3[] v = Lholder.GetComponent<MeshFilter>().mesh.vertices;
		Vector3[] anchorPoints = new Vector3[4];
        //anchorPoints[0] = v[0];
        //anchorPoints[1] = v[1];
        //anchorPoints[2] = v[2];
        //anchorPoints[3] = v[3];
        //角点需要转化为骨骼所在的坐标系下
        anchorPoints[0] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[0]));
        anchorPoints[1] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[1]));
        anchorPoints[2] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[3]));
        anchorPoints[3] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[2]));
        allAhcorPoints.AddRange(anchorPoints);//内 左
        Plane plane = new Plane(anchorPoints[0], anchorPoints[1], anchorPoints[2]);//导板点平面
        //Plane plane = new Plane(CutPosition.CutPoints[0][0], CutPosition.CutPoints[0][1], CutPosition.CutPoints[0][2]);
        for (int i = 0; i < Inner_linePoints.Count; ++i)//投影切割环
        {
            Inner_linePoints[i] = plane.ClosestPointOnPlane(Inner_linePoints[i]);//投影到角点对应的平面上
        }
        lineCirclesPointsL = new List<Vector3>(Inner_linePoints);//获取第一个环
        //for (int i = 0; i < anchorPoints.Length; ++i)//投影到切割环所在平面
        //{
        //    anchorPoints[i] = plane.ClosestPointOnPlane(anchorPoints[i]);
        //}
        //合并面

        //耳切法缝合切面
        int lineNum = Inner_linePoints.Count;//环的长度
        //Inner_line.Reverse();
        Inner_linePoints.AddRange(anchorPoints);//环拼上4个角点
        for (int i = 0; i < 4; ++i)
            Inner_lineNors.Add(Inner_lineNors[0]);//增加角点对应的法向量
        //Inner_line.Add(lineNum + 0); Inner_line.Add(lineNum + 1);//增加外环顺序  顺
        //Inner_line.Add(lineNum + 1); Inner_line.Add(lineNum + 2);
        //Inner_line.Add(lineNum + 2); Inner_line.Add(lineNum + 3);
        //Inner_line.Add(lineNum + 3); Inner_line.Add(lineNum + 0);
        Inner_line.Add(lineNum + 3); Inner_line.Add(lineNum + 2);//增加外环顺序  逆
        Inner_line.Add(lineNum + 2); Inner_line.Add(lineNum + 1);
        Inner_line.Add(lineNum + 1); Inner_line.Add(lineNum + 0);
        Inner_line.Add(lineNum + 0); Inner_line.Add(lineNum + 3);
        ITriangulator triangulator = new Triangulator(Inner_linePoints, Inner_line, Inner_lineNors[0]);
        //Debug.Log("333333");
        int[] newEdges, newTri, newTrangleEdges;
        triangulator.Fill(out newEdges, out newTri, out newTrangleEdges);
        
        //添加三角关系
        List<int> ts = new List<int>(newTri);
        ts.Reverse();//面的渲染方向反了  调整一下
        //showMesh(Inner_linePoints, Inner_lineNors, ts);//面已有
        //return;
        MeshMerge_A_2_B(ref in_ver, ref in_nor, ref in_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);
        //showMesh(in_ver, in_nor, in_tri);
        //return;
        //处理下一个面
        //plane = new Plane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[4])),
        //    trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[5])),
        //    trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[6])));
        //Plane plane1 = new Plane(CutPosition.CutPoints[0][0], CutPosition.CutPoints[0][1], CutPosition.CutPoints[0][2]);//投影到切割环平面
        Plane plane1 = new Plane(CutPosition.cutPlaneNormal[0], CutPosition.cutPoint[0]);
        for (int i = 0; i < Inner_linePoints.Count; ++i)
        {
            Inner_linePoints[i] = plane1.ClosestPointOnPlane(Inner_linePoints[i]);
        }
        //for(int i=0;i< Inner_linePoints1.Count; ++i)
        //{
        //    lineCirclesPointsL.Add(Inner_linePoints[i]);//获取第二个环
        //}
        ts.Reverse();
        MeshMerge_A_2_B(ref in_ver, ref in_nor, ref in_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);
        for(int i = 0; i < 4; ++i)
        {
            allAhcorPoints.Add(plane1.ClosestPointOnPlane(allAhcorPoints[i]));//内 右
        }
        //showMesh(in_ver, in_nor, in_tri);
        //return;
        //添加内表面的三角关系 找到角点对应的索引
        int[] indexOfAnchorPoints = new int[8];
        int offset = in_ver.Count;
        for (int i = 0; i < 8; ++i)
        {
            indexOfAnchorPoints[i] = offset + i;
        }
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[0]))));
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[1]))));
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[2]))));
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[3]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[4]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[5]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[6]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[7]))));
        //缝合内表面 2侧面
        in_tri.Add(indexOfAnchorPoints[7]);
        in_tri.Add(indexOfAnchorPoints[5]);
        in_tri.Add(indexOfAnchorPoints[3]);

        in_tri.Add(indexOfAnchorPoints[1]);
        in_tri.Add(indexOfAnchorPoints[7]);
        in_tri.Add(indexOfAnchorPoints[3]);

        in_tri.Add(indexOfAnchorPoints[0]);
        in_tri.Add(indexOfAnchorPoints[4]);
        in_tri.Add(indexOfAnchorPoints[2]);

        in_tri.Add(indexOfAnchorPoints[0]);
        in_tri.Add(indexOfAnchorPoints[6]);
        in_tri.Add(indexOfAnchorPoints[4]);
        //showMesh(in_ver, in_nor, in_tri);
        //return;
        //生成外表面
        List<Vector3> out_ver = new List<Vector3>(outterFace.vertices);
        List<Vector3> out_nor = new List<Vector3>(outterFace.normals);
        List<int> out_tri = new List<int>(outterFace.triangles);
        Vector3 dir = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[5])) - trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[3]));
        dir.Normalize();
        Vector3 point = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[5])) + dir * thickness * 0.1f;
        plane = new Plane(Inner_lineNors[0], point);
        List<Vector3> cutPointsOut1, cutNorsOut1;
        List<int> cutLinesOut1;
        WholeCut(point, Inner_lineNors[0], ref out_ver, ref out_nor, ref out_tri, out cutPointsOut1, out cutNorsOut1, out cutLinesOut1);
        anchorPoints = new Vector3[4];
        Vector3 L, D, UP;
        L = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[1])) - trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[7]));
        D = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[1])) - trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[0]));
        UP = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[5])) - trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[7]));
        L.Normalize(); D.Normalize(); UP.Normalize();
        //anchorPoints[0] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[2])) + L * thickness - D * thickness;//L -D  //左侧面平移
        //anchorPoints[1] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[3])) + L * thickness + D * thickness;//L D
        //anchorPoints[2] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[1])) + L * thickness - D * thickness;//L -D
        //anchorPoints[3] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[0])) + L * thickness + D * thickness;//L D

        anchorPoints[0] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[5])) - L * thickness + D * thickness;//-L -D  //右侧面平移
        anchorPoints[1] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[4])) - L * thickness - D * thickness;//-L -D
        anchorPoints[2] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[6])) - L * thickness - D * thickness;//-L -D
        anchorPoints[3] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[7])) - L * thickness + D * thickness;//-L D
        //allAhcorPoints.AddRange(anchorPoints);//外 左
        int cnt = cutPointsOut1.Count;
        List<int> lines = new List<int>();
        lines.Add(cnt + 0); lines.Add(cnt + 1);
        lines.Add(cnt + 1); lines.Add(cnt + 2);
        lines.Add(cnt + 2); lines.Add(cnt + 3);
        lines.Add(cnt + 3); lines.Add(cnt + 0);
        lines.Reverse();

        //for (int i = 0; i < Inner_linePoints.Count; ++i)
        //{
        //    Inner_linePoints[i] = plane1.ClosestPointOnPlane(Inner_linePoints[i]);
        //}
        ////Debug.Log("out_ver:::" + out_ver.Count);
        //ts.Reverse();
        //MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);//左侧靠内
        //左侧靠外
        cutLinesOut1.Reverse();
        cutLinesOut1.AddRange(lines);
        //角点位置修正到平面位置
        for(int i = 0; i < 4; ++i)
        {
            anchorPoints[i] = plane.ClosestPointOnPlane(anchorPoints[i]);
        }
        allAhcorPoints.AddRange(anchorPoints);//外 左
        cutPointsOut1.AddRange(anchorPoints);
        triangulator = new Triangulator(cutPointsOut1, cutLinesOut1, Inner_lineNors[0]);
        triangulator.Fill(out newEdges, out newTri, out newTrangleEdges);
        List<int> ts1 = new List<int>(newTri);
        ts1.Reverse();
        MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref cutPointsOut1, ref Inner_lineNors, ref ts1);
        //MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref in_ver, ref in_nor, ref in_tri);
        //showMesh(out_ver, out_nor, out_tri);
        //return;

        //外左缝合
        plane = new Plane(Inner_lineNors[0], trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[2])) + L * thickness);//外左
        Inner_linePoints = new List<Vector3>(CutPosition.CutPoints[0]);//获取内网格切割环
        for (int i = 0; i < Inner_linePoints.Count; ++i)
        {
            Inner_linePoints[i] = plane.ClosestPointOnPlane(Inner_linePoints[i]);//进行投影
        }
        lineCirclesPointsL.AddRange(Inner_linePoints);

        //修改角点
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[0])) + L * thickness - D * thickness);
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[1])) + L * thickness + D * thickness);
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[3])) + L * thickness + D * thickness);
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[2])) + L * thickness - D * thickness);
        //showMesh(Inner_linePoints, Inner_lineNors, ts);
        //return;
        //Debug.Log("Inner_linePoints1=" + Inner_linePoints1.Count);//内外环的顶点数量不同 不可直接缝合 应使用内环的
        //Debug.Log("cutPointsOut1=" + cutPointsOut1.Count);
        //ts.Reverse();
        MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);
        //showMesh(out_ver, out_nor, out_tri);
        //return;
        for (int i = 0; i < 4; ++i)
        {
            Vector3 p = plane.ClosestPointOnPlane(anchorPoints[i]);
            allAhcorPoints.Add(p);
            //lineCirclesPointsL.Add(p);
        }
        MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref in_ver, ref in_nor, ref in_tri);//合并内外表面
        //缝合内外表面的上层、下层
        {
            cnt = out_ver.Count;
            out_ver.AddRange(allAhcorPoints);
            //上层
            out_tri.Add(cnt + 13);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 3);

            out_tri.Add(cnt + 3);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 7);

            out_tri.Add(cnt + 7);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 6);

            out_tri.Add(cnt + 6);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 8);

            out_tri.Add(cnt + 2);
            out_tri.Add(cnt + 6);
            out_tri.Add(cnt + 8);

            out_tri.Add(cnt + 2);
            out_tri.Add(cnt + 8);
            out_tri.Add(cnt + 12);

            out_tri.Add(cnt + 3);
            out_tri.Add(cnt + 2);
            out_tri.Add(cnt + 12);

            out_tri.Add(cnt + 13);
            out_tri.Add(cnt + 3);
            out_tri.Add(cnt + 12);

            //下
            out_tri.Add(cnt + 15);
            out_tri.Add(cnt + 11);
            out_tri.Add(cnt + 1);

            out_tri.Add(cnt + 1);
            out_tri.Add(cnt + 11);
            out_tri.Add(cnt + 5);

            out_tri.Add(cnt + 5);
            out_tri.Add(cnt + 11);
            out_tri.Add(cnt + 4);

            out_tri.Add(cnt + 11);
            out_tri.Add(cnt + 10);
            out_tri.Add(cnt + 4);

            out_tri.Add(cnt + 0);
            out_tri.Add(cnt + 4);
            out_tri.Add(cnt + 10);

            out_tri.Add(cnt + 0);
            out_tri.Add(cnt + 10);
            out_tri.Add(cnt + 14);

            out_tri.Add(cnt + 1);
            out_tri.Add(cnt + 0);
            out_tri.Add(cnt + 14);

            out_tri.Add(cnt + 15);
            out_tri.Add(cnt + 1);
            out_tri.Add(cnt + 14);
        }
        //侧面
        {
            out_tri.Add(cnt + 12);
            out_tri.Add(cnt + 8);
            out_tri.Add(cnt + 15);

            out_tri.Add(cnt + 8);
            out_tri.Add(cnt + 11);
            out_tri.Add(cnt + 15);

            out_tri.Add(cnt + 14);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 13);

            out_tri.Add(cnt + 14);
            out_tri.Add(cnt + 10);
            out_tri.Add(cnt + 9);
        }
        //夹层的两个环
        //最左边的环
        List<int> linetriL;
        connectCircleHull(lineCirclesPointsL, out linetriL);
        //linetriL.Reverse();
        List<Vector3> nor = new List<Vector3>(Inner_lineNor1);
        nor.AddRange(Inner_lineNor1);
        MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref lineCirclesPointsL, ref Inner_lineNors, ref linetriL);
        //showMesh(out_ver, out_nor, out_tri);
        //Debug.Log("lineCirclesPointsL:" + lineCirclesPointsL.Count);
        //Debug.Log("Inner_lineNors:" + Inner_lineNors.Count);
        //GizomsPoint = lineCirclesPointsL;
        //showMesh(lineCirclesPointsL, Inner_lineNor1, linetriL);
        //showMesh(Inner_linePoints, Inner_lineNors, ts);
        innerFace.vertices = out_ver.ToArray();
        innerFace.normals = out_nor.ToArray();
        innerFace.triangles = out_tri.ToArray();
    }
    //原版
    void GenerateRightFace1()
    {
        List<Vector3> lineCirclesPointsL, lineCirclesPointsR;
        List<Vector3> allAhcorPoints = new List<Vector3>(16);
        //先缝合内表面
        List<Vector3> Inner_linePoints = new List<Vector3>(Inner_linePoints2);//获取环上的点
        List<Vector3> Inner_lineNors = new List<Vector3>(Inner_lineNor2);//获取环上的点
        List<int> Inner_line = new List<int>(Inner_line2);//获取环上的轮廓
        List<Vector3> in_ver = new List<Vector3>(innerFace.vertices), in_nor = new List<Vector3>(innerFace.normals);
        List<int> in_tri = new List<int>(innerFace.triangles);
        //in_tri.Reverse();
        Transform trans = CutPosition.cutObject[CutPosition.cutObject.Count - 1].transform;
        //获取左侧4个角点 逆时针顺序
        //Debug.Log("Lholder:" + Lholder == null ? "空" : "非空");
        Vector3[] v = Rholder.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] anchorPoints = new Vector3[4];
        //anchorPoints[0] = v[0];
        //anchorPoints[1] = v[1];
        //anchorPoints[2] = v[2];
        //anchorPoints[3] = v[3];
        //角点需要转化为骨骼所在的坐标系下
        //anchorPoints[0] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[0]));
        //anchorPoints[1] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[1]));
        //anchorPoints[2] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[3]));
        //anchorPoints[3] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[2]));

        anchorPoints[0] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[2]));
        anchorPoints[1] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[3]));
        anchorPoints[2] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[1]));
        anchorPoints[3] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[0]));
        allAhcorPoints.AddRange(anchorPoints);//内 左
        Plane plane = new Plane(anchorPoints[0], anchorPoints[1], anchorPoints[2]);//导板点平面
        //Plane plane = new Plane(CutPosition.CutPoints[0][0], CutPosition.CutPoints[0][1], CutPosition.CutPoints[0][2]);
        for (int i = 0; i < Inner_linePoints.Count; ++i)//投影切割环
        {
            Inner_linePoints[i] = plane.ClosestPointOnPlane(Inner_linePoints[i]);//投影到角点对应的平面上
        }
        lineCirclesPointsL = new List<Vector3>(Inner_linePoints);//获取第一个环
        //for (int i = 0; i < anchorPoints.Length; ++i)//投影到切割环所在平面
        //{
        //    anchorPoints[i] = plane.ClosestPointOnPlane(anchorPoints[i]);
        //}
        //合并面

        //耳切法缝合切面
        int lineNum = Inner_linePoints.Count;//环的长度
        //Inner_line.Reverse();
        Inner_linePoints.AddRange(anchorPoints);//环拼上4个角点
        for (int i = 0; i < 4; ++i)
            Inner_lineNors.Add(Inner_lineNors[0]);//增加角点对应的法向量
        //Inner_line.Add(lineNum + 0); Inner_line.Add(lineNum + 1);//增加外环顺序  顺
        //Inner_line.Add(lineNum + 1); Inner_line.Add(lineNum + 2);
        //Inner_line.Add(lineNum + 2); Inner_line.Add(lineNum + 3);
        //Inner_line.Add(lineNum + 3); Inner_line.Add(lineNum + 0);
        Inner_line.Add(lineNum + 3); Inner_line.Add(lineNum + 2);//增加外环顺序  逆
        Inner_line.Add(lineNum + 2); Inner_line.Add(lineNum + 1);
        Inner_line.Add(lineNum + 1); Inner_line.Add(lineNum + 0);
        Inner_line.Add(lineNum + 0); Inner_line.Add(lineNum + 3);
        ITriangulator triangulator = new Triangulator(Inner_linePoints, Inner_line, Inner_lineNors[0]);
        //Debug.Log("333333");
        int[] newEdges, newTri, newTrangleEdges;
        triangulator.Fill(out newEdges, out newTri, out newTrangleEdges);

        //添加三角关系
        List<int> ts = new List<int>(newTri);
        //ts.Reverse();//面的渲染方向反了  调整一下
        //showMesh(Inner_linePoints, Inner_lineNors, ts);//面已有
        //return;
        MeshMerge_A_2_B(ref in_ver, ref in_nor, ref in_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);
        //showMesh(in_ver, in_nor, in_tri);
        //return;
        //处理下一个面
        //plane = new Plane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[4])),
        //    trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[5])),
        //    trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[6])));
        //Plane plane1 = new Plane(CutPosition.CutPoints[0][0], CutPosition.CutPoints[0][1], CutPosition.CutPoints[0][2]);//投影到切割环平面
        Plane plane1 = new Plane(CutPosition.cutPlaneNormal[1], CutPosition.cutPoint[1]);
        for (int i = 0; i < Inner_linePoints.Count; ++i)
        {
            Inner_linePoints[i] = plane1.ClosestPointOnPlane(Inner_linePoints[i]);
        }
        //for(int i=0;i< Inner_linePoints1.Count; ++i)
        //{
        //    lineCirclesPointsL.Add(Inner_linePoints[i]);//获取第二个环
        //}
        ts.Reverse();
        MeshMerge_A_2_B(ref in_ver, ref in_nor, ref in_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);
        for (int i = 0; i < 4; ++i)
        {
            allAhcorPoints.Add(plane1.ClosestPointOnPlane(allAhcorPoints[i]));//内 右
        }
        //showMesh(in_ver, in_nor, in_tri);
        //return;
        //添加内表面的三角关系 找到角点对应的索引
        int[] indexOfAnchorPoints = new int[8];
        int offset = in_ver.Count;
        for (int i = 0; i < 8; ++i)
        {
            indexOfAnchorPoints[i] = offset + i;
        }
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[0]))));
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[1]))));
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[2]))));
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[3]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[4]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[5]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[6]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[7]))));
        //缝合内表面 2侧面
        in_tri.Add(indexOfAnchorPoints[7]);
        in_tri.Add(indexOfAnchorPoints[5]);
        in_tri.Add(indexOfAnchorPoints[3]);

        in_tri.Add(indexOfAnchorPoints[1]);
        in_tri.Add(indexOfAnchorPoints[7]);
        in_tri.Add(indexOfAnchorPoints[3]);

        in_tri.Add(indexOfAnchorPoints[2]);
        in_tri.Add(indexOfAnchorPoints[4]);
        in_tri.Add(indexOfAnchorPoints[0]);

        in_tri.Add(indexOfAnchorPoints[4]);
        in_tri.Add(indexOfAnchorPoints[6]);
        in_tri.Add(indexOfAnchorPoints[0]);
        //showMesh(in_ver, in_nor, in_tri);
        //return;

        //生成外表面
        List<Vector3> out_ver = new List<Vector3>(outterFace.vertices);
        List<Vector3> out_nor = new List<Vector3>(outterFace.normals);
        List<int> out_tri = new List<int>(outterFace.triangles);
        Vector3 dir = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[5])) - trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[3]));
        dir.Normalize();
        Vector3 point = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[5])) + dir * thickness * 0.1f;
        plane = new Plane(Inner_lineNors[0], point);
        List<Vector3> cutPointsOut1, cutNorsOut1;
        List<int> cutLinesOut1;
        WholeCut(point, Inner_lineNors[0], ref out_ver, ref out_nor, ref out_tri, out cutPointsOut1, out cutNorsOut1, out cutLinesOut1);
        anchorPoints = new Vector3[4];
        Vector3 L, D, UP;
        L = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[1])) - trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[7]));
        D = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[1])) - trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[0]));
        UP = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[5])) - trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[7]));
        L.Normalize(); D.Normalize(); UP.Normalize();
        //anchorPoints[0] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[2])) + L * thickness - D * thickness;//L -D  //左侧面平移
        //anchorPoints[1] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[3])) + L * thickness + D * thickness;//L D
        //anchorPoints[2] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[1])) + L * thickness - D * thickness;//L -D
        //anchorPoints[3] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[0])) + L * thickness + D * thickness;//L D

        anchorPoints[3] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[5])) - L * thickness + D * thickness;//-L -D  //右侧面平移
        anchorPoints[2] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[4])) - L * thickness - D * thickness;//-L -D
        anchorPoints[1] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[6])) - L * thickness - D * thickness;//-L -D
        anchorPoints[0] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[7])) - L * thickness + D * thickness;//-L D
        //allAhcorPoints.AddRange(anchorPoints);//外 左
        int cnt = cutPointsOut1.Count;
        List<int> lines = new List<int>();
        lines.Add(cnt + 0); lines.Add(cnt + 1);
        lines.Add(cnt + 1); lines.Add(cnt + 2);
        lines.Add(cnt + 2); lines.Add(cnt + 3);
        lines.Add(cnt + 3); lines.Add(cnt + 0);
        lines.Reverse();

        //for (int i = 0; i < Inner_linePoints.Count; ++i)
        //{
        //    Inner_linePoints[i] = plane1.ClosestPointOnPlane(Inner_linePoints[i]);
        //}
        ////Debug.Log("out_ver:::" + out_ver.Count);
        //ts.Reverse();
        //MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);//左侧靠内
        //左侧靠外
        cutLinesOut1.Reverse();
        cutLinesOut1.AddRange(lines);
        //角点位置修正到平面位置
        for (int i = 0; i < 4; ++i)
        {
            anchorPoints[i] = plane.ClosestPointOnPlane(anchorPoints[i]);
        }
        allAhcorPoints.AddRange(anchorPoints);//外 左
        cutPointsOut1.AddRange(anchorPoints);
        triangulator = new Triangulator(cutPointsOut1, cutLinesOut1, Inner_lineNors[0]);
        triangulator.Fill(out newEdges, out newTri, out newTrangleEdges);
        List<int> ts1 = new List<int>(newTri);
        //ts1.Reverse();
        MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref cutPointsOut1, ref Inner_lineNors, ref ts1);
        //MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref in_ver, ref in_nor, ref in_tri);
        //showMesh(out_ver, out_nor, out_tri);
        //return;

        //外左缝合
        plane = new Plane(Inner_lineNors[0], trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[2])) + L * thickness);//外左
        Inner_linePoints = new List<Vector3>(CutPosition.CutPoints[1]);//获取内网格切割环
        for (int i = 0; i < Inner_linePoints.Count; ++i)
        {
            Inner_linePoints[i] = plane.ClosestPointOnPlane(Inner_linePoints[i]);//进行投影
        }
        lineCirclesPointsL.AddRange(Inner_linePoints);
        //Inner_linePoints.Reverse();
        //修改角点
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[2])) + L * thickness - D * thickness);
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[3])) + L * thickness + D * thickness);
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[1])) + L * thickness + D * thickness);
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[0])) + L * thickness - D * thickness);
        //ts.Reverse();
        //showMesh(Inner_linePoints, Inner_lineNors, ts);
        //return;
        //Debug.Log("Inner_linePoints1=" + Inner_linePoints1.Count);//内外环的顶点数量不同 不可直接缝合 应使用内环的
        //Debug.Log("cutPointsOut1=" + cutPointsOut1.Count);
        //ts.Reverse();
        MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);
        //showMesh(out_ver, out_nor, out_tri);
        //return;
        for (int i = 0; i < 4; ++i)
        {
            Vector3 p = plane.ClosestPointOnPlane(anchorPoints[i]);
            allAhcorPoints.Add(p);
            //lineCirclesPointsL.Add(p);
        }
        MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref in_ver, ref in_nor, ref in_tri);//合并内外表面
        //showMesh(out_ver, out_nor, out_tri);
        //return;
        //缝合内外表面的上层、下层
        {
            cnt = out_ver.Count;
            out_ver.AddRange(allAhcorPoints);
            //上层
            out_tri.Add(cnt + 3);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 13);

            out_tri.Add(cnt + 7);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 3);

            out_tri.Add(cnt + 6);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 7);

            out_tri.Add(cnt + 8);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 6);

            out_tri.Add(cnt + 8);
            out_tri.Add(cnt + 6);
            out_tri.Add(cnt + 2);

            out_tri.Add(cnt + 12);
            out_tri.Add(cnt + 8);
            out_tri.Add(cnt + 2);

            out_tri.Add(cnt + 12);
            out_tri.Add(cnt + 2);
            out_tri.Add(cnt + 3);

            out_tri.Add(cnt + 12);
            out_tri.Add(cnt + 3);
            out_tri.Add(cnt + 13);

            //下
            out_tri.Add(cnt + 1);
            out_tri.Add(cnt + 11);
            out_tri.Add(cnt + 15);

            out_tri.Add(cnt + 5);
            out_tri.Add(cnt + 11);
            out_tri.Add(cnt + 1);

            out_tri.Add(cnt + 4);
            out_tri.Add(cnt + 11);
            out_tri.Add(cnt + 5);

            out_tri.Add(cnt + 4);
            out_tri.Add(cnt + 10);
            out_tri.Add(cnt + 11);

            out_tri.Add(cnt + 10);
            out_tri.Add(cnt + 4);
            out_tri.Add(cnt + 0);

            out_tri.Add(cnt + 14);
            out_tri.Add(cnt + 10);
            out_tri.Add(cnt + 0);

            out_tri.Add(cnt + 14);
            out_tri.Add(cnt + 0);
            out_tri.Add(cnt + 1);

            out_tri.Add(cnt + 14);
            out_tri.Add(cnt + 1);
            out_tri.Add(cnt + 15);
        }
        //侧面
        {
            out_tri.Add(cnt + 15);
            out_tri.Add(cnt + 8);
            out_tri.Add(cnt + 12);

            out_tri.Add(cnt + 15);
            out_tri.Add(cnt + 11);
            out_tri.Add(cnt + 8);

            out_tri.Add(cnt + 13);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 14);

            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 10);
            out_tri.Add(cnt + 14);
        }
        //夹层的两个环
        //最左边的环
        List<int> linetriL;
        connectCircleHull(lineCirclesPointsL, out linetriL);
        linetriL.Reverse();
        List<Vector3> nor = new List<Vector3>(Inner_lineNor1);
        nor.AddRange(Inner_lineNor1);
        MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref lineCirclesPointsL, ref Inner_lineNors, ref linetriL);
        //showMesh(out_ver, out_nor, out_tri);
        //Debug.Log("lineCirclesPointsL:" + lineCirclesPointsL.Count);
        //Debug.Log("Inner_lineNors:" + Inner_lineNors.Count);
        //GizomsPoint = lineCirclesPointsL;
        //showMesh(lineCirclesPointsL, Inner_lineNor1, linetriL);
        //showMesh(Inner_linePoints, Inner_lineNors, ts);

        outterFace.vertices = out_ver.ToArray();
        outterFace.normals = out_nor.ToArray();
        outterFace.triangles = out_tri.ToArray();
    }

    //修改版  减少重复网格  over
    void GenerateLeftFace()
    {
        List<Vector3> lineCirclesPointsL, lineCirclesPointsR;
        List<Vector3> allAhcorPoints = new List<Vector3>(16);
        //先缝合内表面
        List<Vector3> Inner_linePoints = new List<Vector3>(Inner_linePoints1);//获取环上的点
        List<Vector3> Inner_lineNors = new List<Vector3>(Inner_lineNor1);//获取环上的点
        List<int> Inner_line = new List<int>(Inner_line1);//获取环上的轮廓
        List<Vector3> in_ver = new List<Vector3>(innerFace.vertices), in_nor = new List<Vector3>(innerFace.normals);
        List<int> in_tri = new List<int>(innerFace.triangles);
        //in_tri.Reverse();
        Transform trans = CutPosition.cutObject[CutPosition.cutObject.Count - 1].transform;
        //获取左侧4个角点 逆时针顺序
        //Debug.Log("Lholder:" + Lholder == null ? "空" : "非空");
        Vector3[] v = Lholder.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] anchorPoints = new Vector3[4];
        //anchorPoints[0] = v[0];
        //anchorPoints[1] = v[1];
        //anchorPoints[2] = v[2];
        //anchorPoints[3] = v[3];
        //角点需要转化为骨骼所在的坐标系下
        anchorPoints[0] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[0]));
        anchorPoints[1] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[1]));
        anchorPoints[2] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[3]));
        anchorPoints[3] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[2]));
        allAhcorPoints.AddRange(anchorPoints);//内 左
        Plane plane = new Plane(anchorPoints[0], anchorPoints[1], anchorPoints[2]);//导板点平面
        //Plane plane = new Plane(CutPosition.CutPoints[0][0], CutPosition.CutPoints[0][1], CutPosition.CutPoints[0][2]);
        for (int i = 0; i < Inner_linePoints.Count; ++i)//投影切割环
        {
            Inner_linePoints[i] = plane.ClosestPointOnPlane(Inner_linePoints[i]);//投影到角点对应的平面上
        }
        lineCirclesPointsL = new List<Vector3>(Inner_linePoints);//获取第一个环
        //for (int i = 0; i < anchorPoints.Length; ++i)//投影到切割环所在平面
        //{
        //    anchorPoints[i] = plane.ClosestPointOnPlane(anchorPoints[i]);
        //}
        //合并面

        //耳切法缝合切面
        int lineNum = Inner_linePoints.Count;//环的长度
        Inner_line.Reverse();
        Inner_linePoints.AddRange(anchorPoints);//环拼上4个角点
        //Debug.Log("Inner_lineNors::" + Inner_lineNors.Count);
        for (int i = 0; i < 4; ++i)
            Inner_lineNors.Add(Inner_lineNors[0]);//增加角点对应的法向量
        Inner_line.Add(lineNum + 0); Inner_line.Add(lineNum + 1);//增加外环顺序  顺
        Inner_line.Add(lineNum + 1); Inner_line.Add(lineNum + 2);
        Inner_line.Add(lineNum + 2); Inner_line.Add(lineNum + 3);
        Inner_line.Add(lineNum + 3); Inner_line.Add(lineNum + 0);
        //Inner_line.Add(lineNum + 3); Inner_line.Add(lineNum + 2);//增加外环顺序  逆
        //Inner_line.Add(lineNum + 2); Inner_line.Add(lineNum + 1);
        //Inner_line.Add(lineNum + 1); Inner_line.Add(lineNum + 0);
        //Inner_line.Add(lineNum + 0); Inner_line.Add(lineNum + 3);
        ITriangulator triangulator = new Triangulator(Inner_linePoints, Inner_line, Inner_lineNors[0]);
        //Debug.Log("333333");
        int[] newEdges, newTri, newTrangleEdges;
        triangulator.Fill(out newEdges, out newTri, out newTrangleEdges);

        //添加三角关系
        List<int> ts = new List<int>(newTri);
        //ts.Reverse();//面的渲染方向反了  调整一下  修改1
        //showMesh(Inner_linePoints, Inner_lineNors, ts);//面已有
        //return;
        MeshMerge_A_2_B(ref in_ver, ref in_nor, ref in_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);
        //处理下一个面
        //plane = new Plane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[4])),
        //    trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[5])),
        //    trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[6])));
        //Plane plane1 = new Plane(CutPosition.CutPoints[0][0], CutPosition.CutPoints[0][1], CutPosition.CutPoints[0][2]);//投影到切割环平面
        Plane plane1 = new Plane(CutPosition.cutPlaneNormal[0], CutPosition.cutPoint[0]);
        for (int i = 0; i < Inner_linePoints.Count; ++i)
        {
            Inner_linePoints[i] = plane1.ClosestPointOnPlane(Inner_linePoints[i]);
        }
        //for(int i=0;i< Inner_linePoints1.Count; ++i)
        //{
        //    lineCirclesPointsL.Add(Inner_linePoints[i]);//获取第二个环
        //}
        ts.Reverse();
        MeshMerge_A_2_B(ref in_ver, ref in_nor, ref in_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);
        for (int i = 0; i < 4; ++i)
        {
            allAhcorPoints.Add(plane1.ClosestPointOnPlane(allAhcorPoints[i]));//内 右
        }
        //showMesh(in_ver, in_nor, in_tri);
        //return;
        //添加内表面的三角关系 找到角点对应的索引
        int[] indexOfAnchorPoints = new int[8];
        int offset = in_ver.Count;
        for (int i = 0; i < 8; ++i)
        {
            indexOfAnchorPoints[i] = offset + i;
        }
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[0]))));
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[1]))));
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[2]))));
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[3]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[4]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[5]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[6]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[7]))));
        for(int i = 0; i < 8; ++i)
        {
            in_nor.Add(Inner_lineNors[0]);//补充法向量
        }
        //缝合内表面 2侧面
        //in_tri.Add(indexOfAnchorPoints[7]);//翻了
        //in_tri.Add(indexOfAnchorPoints[5]);
        //in_tri.Add(indexOfAnchorPoints[3]);

        //in_tri.Add(indexOfAnchorPoints[1]);
        //in_tri.Add(indexOfAnchorPoints[7]);
        //in_tri.Add(indexOfAnchorPoints[3]);

        //in_tri.Add(indexOfAnchorPoints[0]);
        //in_tri.Add(indexOfAnchorPoints[4]);
        //in_tri.Add(indexOfAnchorPoints[2]);

        //in_tri.Add(indexOfAnchorPoints[0]);
        //in_tri.Add(indexOfAnchorPoints[6]);
        //in_tri.Add(indexOfAnchorPoints[4]);

        in_tri.Add(indexOfAnchorPoints[7]);
        in_tri.Add(indexOfAnchorPoints[5]);
        in_tri.Add(indexOfAnchorPoints[3]);

        in_tri.Add(indexOfAnchorPoints[1]);
        in_tri.Add(indexOfAnchorPoints[7]);
        in_tri.Add(indexOfAnchorPoints[3]);

        in_tri.Add(indexOfAnchorPoints[2]);
        in_tri.Add(indexOfAnchorPoints[4]);
        in_tri.Add(indexOfAnchorPoints[0]);

        in_tri.Add(indexOfAnchorPoints[4]);
        in_tri.Add(indexOfAnchorPoints[6]);
        in_tri.Add(indexOfAnchorPoints[0]);

        //showMesh(in_ver, in_nor, in_tri);
        //return;


        //生成外表面
        List<Vector3> out_ver = new List<Vector3>(outterFace.vertices);
        List<Vector3> out_nor = new List<Vector3>(outterFace.normals);
        List<int> out_tri = new List<int>(outterFace.triangles);
        //showMesh(out_ver, out_nor, out_tri, "outface");
        Vector3 dir = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[5])) - trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[3]));
        dir.Normalize();
        Vector3 point = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[5])) + dir * thickness * 0.1f;
        plane = new Plane(Inner_lineNors[0], point);
        List<Vector3> cutPointsOut1, cutNorsOut1;
        List<int> cutLinesOut1;
        WholeCut(point, Inner_lineNors[0], ref out_ver, ref out_nor, ref out_tri, out cutPointsOut1, out cutNorsOut1, out cutLinesOut1);
        Debug.Log("cutNorsOut1::" + cutNorsOut1.Count);
        //对外层网格另一侧切割
        point = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[5])) + dir * thickness * 0.1f;
        WholeCut(point, Inner_lineNors[0], ref out_ver, ref out_nor, ref out_tri, out cutPointsOut2, out cutNorsOut2, out cutLinesOut2);
        anchorPoints = new Vector3[4];
        Vector3 L, D, UP;
        L = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[1])) - trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[7]));
        D = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[1])) - trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[0]));
        UP = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[5])) - trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[7]));
        L.Normalize(); D.Normalize(); UP.Normalize();
        //anchorPoints[0] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[2])) + L * thickness - D * thickness;//L -D  //左侧面平移
        //anchorPoints[1] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[3])) + L * thickness + D * thickness;//L D
        //anchorPoints[2] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[1])) + L * thickness - D * thickness;//L -D
        //anchorPoints[3] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[0])) + L * thickness + D * thickness;//L D

        anchorPoints[0] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[5])) - L * thickness + D * thickness;//-L -D  //右侧面平移
        anchorPoints[1] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[4])) - L * thickness - D * thickness;//-L -D
        anchorPoints[2] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[6])) - L * thickness - D * thickness;//-L -D
        anchorPoints[3] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[7])) - L * thickness + D * thickness;//-L D
        //allAhcorPoints.AddRange(anchorPoints);//外 左
        int cnt = cutPointsOut1.Count;
        List<int> lines = new List<int>();
        lines.Add(cnt + 0); lines.Add(cnt + 1);
        lines.Add(cnt + 1); lines.Add(cnt + 2);
        lines.Add(cnt + 2); lines.Add(cnt + 3);
        lines.Add(cnt + 3); lines.Add(cnt + 0);
        //lines.Reverse();//修改1

        //for (int i = 0; i < Inner_linePoints.Count; ++i)
        //{
        //    Inner_linePoints[i] = plane1.ClosestPointOnPlane(Inner_linePoints[i]);
        //}
        ////Debug.Log("out_ver:::" + out_ver.Count);
        //ts.Reverse();
        //MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);//左侧靠内
        //左侧靠外
        cutLinesOut1.Reverse();
        cutLinesOut1.AddRange(lines);
        //角点位置修正到平面位置
        for (int i = 0; i < 4; ++i)
        {
            anchorPoints[i] = plane.ClosestPointOnPlane(anchorPoints[i]);
        }
        allAhcorPoints.AddRange(anchorPoints);//外 左
        cutPointsOut1.AddRange(anchorPoints);
        for(int i = 0; i < 4; ++i)//增加角点法向量
        {
            cutNorsOut1.Add(cutNorsOut2[0]);
        }
        triangulator = new Triangulator(cutPointsOut1, cutLinesOut1, Inner_lineNors[0]);
        triangulator.Fill(out newEdges, out newTri, out newTrangleEdges);
        List<int> ts1 = new List<int>(newTri);
        //ts1.Reverse();//修改1
        MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref cutPointsOut1, ref cutNorsOut1, ref ts1);
        //MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref in_ver, ref in_nor, ref in_tri);
        //showMesh(out_ver, out_nor, out_tri);
        //return;

        //外左缝合
        plane = new Plane(Inner_lineNors[0], trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[2])) + L * thickness);//外左
        Inner_linePoints = new List<Vector3>(CutPosition.CutPoints[0]);//获取内网格切割环
        for (int i = 0; i < Inner_linePoints.Count; ++i)
        {
            Inner_linePoints[i] = plane.ClosestPointOnPlane(Inner_linePoints[i]);//进行投影
        }
        lineCirclesPointsL.AddRange(Inner_linePoints);

        //修改角点
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[0])) + L * thickness - D * thickness);
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[1])) + L * thickness + D * thickness);
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[3])) + L * thickness + D * thickness);
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[2])) + L * thickness - D * thickness);

        //showMesh(Inner_linePoints, Inner_lineNors, ts);
        //return;
        //Debug.Log("Inner_linePoints1=" + Inner_linePoints1.Count);//内外环的顶点数量不同 不可直接缝合 应使用内环的
        //Debug.Log("cutPointsOut1=" + cutPointsOut1.Count);
        //ts.Reverse();
        MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);
        //showMesh(out_ver, out_nor, out_tri);
        //return;
        for (int i = 0; i < 4; ++i)
        {
            Vector3 p = plane.ClosestPointOnPlane(anchorPoints[i]);
            allAhcorPoints.Add(p);
            //lineCirclesPointsL.Add(p);
        }
        MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref in_ver, ref in_nor, ref in_tri);//合并内外表面
        //缝合内外表面的上层、下层
        {
            cnt = out_ver.Count;
            out_ver.AddRange(allAhcorPoints);
            for(int i = 0; i < 16; ++i)
            {
                out_nor.Add(cutNorsOut2[0]);
            }
            //上层
            out_tri.Add(cnt + 13);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 3);

            out_tri.Add(cnt + 3);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 7);

            out_tri.Add(cnt + 7);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 6);

            out_tri.Add(cnt + 6);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 8);

            out_tri.Add(cnt + 2);
            out_tri.Add(cnt + 6);
            out_tri.Add(cnt + 8);

            out_tri.Add(cnt + 2);
            out_tri.Add(cnt + 8);
            out_tri.Add(cnt + 12);

            out_tri.Add(cnt + 3);
            out_tri.Add(cnt + 2);
            out_tri.Add(cnt + 12);

            out_tri.Add(cnt + 13);
            out_tri.Add(cnt + 3);
            out_tri.Add(cnt + 12);

            //下
            out_tri.Add(cnt + 15);
            out_tri.Add(cnt + 11);
            out_tri.Add(cnt + 1);

            out_tri.Add(cnt + 1);
            out_tri.Add(cnt + 11);
            out_tri.Add(cnt + 5);

            out_tri.Add(cnt + 5);
            out_tri.Add(cnt + 11);
            out_tri.Add(cnt + 4);

            out_tri.Add(cnt + 11);
            out_tri.Add(cnt + 10);
            out_tri.Add(cnt + 4);

            out_tri.Add(cnt + 0);
            out_tri.Add(cnt + 4);
            out_tri.Add(cnt + 10);

            out_tri.Add(cnt + 0);
            out_tri.Add(cnt + 10);
            out_tri.Add(cnt + 14);

            out_tri.Add(cnt + 1);
            out_tri.Add(cnt + 0);
            out_tri.Add(cnt + 14);

            out_tri.Add(cnt + 15);
            out_tri.Add(cnt + 1);
            out_tri.Add(cnt + 14);
        }
        //侧面
        {
            out_tri.Add(cnt + 12);
            out_tri.Add(cnt + 8);
            out_tri.Add(cnt + 15);

            out_tri.Add(cnt + 8);
            out_tri.Add(cnt + 11);
            out_tri.Add(cnt + 15);

            out_tri.Add(cnt + 14);
            out_tri.Add(cnt + 9);
            out_tri.Add(cnt + 13);

            out_tri.Add(cnt + 14);
            out_tri.Add(cnt + 10);
            out_tri.Add(cnt + 9);
        }
        //夹层的两个环
        //最左边的环
        List<int> linetriL;
        connectCircleHull(lineCirclesPointsL, out linetriL);
        //linetriL.Reverse();
        List<Vector3> nor = new List<Vector3>(Inner_lineNor1);
        nor.AddRange(Inner_lineNor1);
        MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref lineCirclesPointsL, ref nor, ref linetriL);
        //showMesh(out_ver, out_nor, out_tri);
        //Debug.Log("lineCirclesPointsL:" + lineCirclesPointsL.Count);
        //Debug.Log("Inner_lineNors:" + Inner_lineNors.Count);
        //GizomsPoint = lineCirclesPointsL;
        //showMesh(lineCirclesPointsL, Inner_lineNor1, linetriL);
        //showMesh(Inner_linePoints, Inner_lineNors, ts);
        //Debug.Log("out_ver=" + out_ver.Count);
        //Debug.Log("out_nor=" + out_nor.Count);
        //Debug.Log("=============");
        //Debug.Log("out_ver = " + out_ver.Count);
        //Debug.Log("out_nor = " + out_nor.Count);
        //Debug.Log("=============");
        innerFace.vertices = out_ver.ToArray();
        innerFace.normals = out_nor.ToArray();
        innerFace.triangles = out_tri.ToArray();
        //showMesh(out_ver, out_nor, out_tri);
    }
    void GenerateRightFace()
    {
        List<Vector3> lineCirclesPointsL, lineCirclesPointsR;
        List<Vector3> allAhcorPoints = new List<Vector3>(16);
        //先缝合内表面
        List<Vector3> Inner_linePoints = new List<Vector3>(Inner_linePoints2);//获取环上的点
        List<Vector3> Inner_lineNors = new List<Vector3>(Inner_lineNor2);//获取环上的点
        List<int> Inner_line = new List<int>(Inner_line2);//获取环上的轮廓
        List<Vector3> in_ver = new List<Vector3>(innerFace.vertices), in_nor = new List<Vector3>(innerFace.normals);
        List<int> in_tri = new List<int>(innerFace.triangles);//获取修改后的网格
        //Debug.Log("=============");
        //Debug.Log("in_ver = " + in_ver.Count);
        //Debug.Log("in_nor = " + in_nor.Count);
        //Debug.Log("=============");
        //in_tri.Reverse();
        Transform trans = CutPosition.cutObject[CutPosition.cutObject.Count - 1].transform;
        //获取左侧4个角点 逆时针顺序
        //Debug.Log("Lholder:" + Lholder == null ? "空" : "非空");
        Vector3[] v = Rholder.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] anchorPoints = new Vector3[4];
        //anchorPoints[0] = v[0];
        //anchorPoints[1] = v[1];
        //anchorPoints[2] = v[2];
        //anchorPoints[3] = v[3];
        //角点需要转化为骨骼所在的坐标系下
        //anchorPoints[0] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[0]));
        //anchorPoints[1] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[1]));
        //anchorPoints[2] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[3]));
        //anchorPoints[3] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[2]));

        anchorPoints[0] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[2]));
        anchorPoints[1] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[3]));
        anchorPoints[2] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[1]));
        anchorPoints[3] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[0]));
        allAhcorPoints.AddRange(anchorPoints);//内 左
        Plane plane = new Plane(anchorPoints[0], anchorPoints[1], anchorPoints[2]);//导板点平面
        //Plane plane = new Plane(CutPosition.CutPoints[0][0], CutPosition.CutPoints[0][1], CutPosition.CutPoints[0][2]);
        for (int i = 0; i < Inner_linePoints.Count; ++i)//投影切割环
        {
            Inner_linePoints[i] = plane.ClosestPointOnPlane(Inner_linePoints[i]);//投影到角点对应的平面上
        }
        lineCirclesPointsL = new List<Vector3>(Inner_linePoints);//获取第一个环
        //for (int i = 0; i < anchorPoints.Length; ++i)//投影到切割环所在平面
        //{
        //    anchorPoints[i] = plane.ClosestPointOnPlane(anchorPoints[i]);
        //}
        //合并面

        //耳切法缝合切面
        int lineNum = Inner_linePoints.Count;//环的长度
        Inner_line.Reverse();
        Inner_linePoints.AddRange(anchorPoints);//环拼上4个角点
        Debug.Log("Inner_lineNors::" + Inner_lineNors.Count);
        for (int i = 0; i < 4; ++i)
            Inner_lineNors.Add(Inner_lineNors[0]);//增加角点对应的法向量
        Inner_line.Add(lineNum + 0); Inner_line.Add(lineNum + 1);//增加外环顺序  顺
        Inner_line.Add(lineNum + 1); Inner_line.Add(lineNum + 2);
        Inner_line.Add(lineNum + 2); Inner_line.Add(lineNum + 3);
        Inner_line.Add(lineNum + 3); Inner_line.Add(lineNum + 0);
        //Inner_line.Add(lineNum + 3); Inner_line.Add(lineNum + 2);//增加外环顺序  逆
        //Inner_line.Add(lineNum + 2); Inner_line.Add(lineNum + 1);
        //Inner_line.Add(lineNum + 1); Inner_line.Add(lineNum + 0);
        //Inner_line.Add(lineNum + 0); Inner_line.Add(lineNum + 3);
        ITriangulator triangulator = new Triangulator(Inner_linePoints, Inner_line, Inner_lineNors[0]);
        //Debug.Log("333333");
        int[] newEdges, newTri, newTrangleEdges;
        triangulator.Fill(out newEdges, out newTri, out newTrangleEdges);

        //添加三角关系
        List<int> ts = new List<int>(newTri);
        ts.Reverse();//面的渲染方向反了  调整一下  修改1
        //showMesh(Inner_linePoints, Inner_lineNors, ts);//面已有
        //return;
        MeshMerge_A_2_B(ref in_ver, ref in_nor, ref in_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);
        //Debug.Log("=============");
        //Debug.Log("in_ver = " + in_ver.Count);
        //Debug.Log("in_nor = " + in_nor.Count);
        //Debug.Log("=============");
        //showMesh(in_ver, in_nor, in_tri);
        //return;
        //处理下一个面
        //plane = new Plane(trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[4])),
        //    trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[5])),
        //    trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[6])));
        //Plane plane1 = new Plane(CutPosition.CutPoints[0][0], CutPosition.CutPoints[0][1], CutPosition.CutPoints[0][2]);//投影到切割环平面
        Plane plane1 = new Plane(CutPosition.cutPlaneNormal[1], CutPosition.cutPoint[1]);
        for (int i = 0; i < Inner_linePoints.Count; ++i)
        {
            Inner_linePoints[i] = plane1.ClosestPointOnPlane(Inner_linePoints[i]);
        }
        //for(int i=0;i< Inner_linePoints1.Count; ++i)
        //{
        //    lineCirclesPointsL.Add(Inner_linePoints[i]);//获取第二个环
        //}
        ts.Reverse();//修改1
        MeshMerge_A_2_B(ref in_ver, ref in_nor, ref in_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);
        showMesh(in_ver, Inner_lineNors, in_tri, "测试！");
        //Debug.Log("=============");
        //Debug.Log("in_ver = " + in_ver.Count);
        //Debug.Log("in_nor = " + in_nor.Count);
        //Debug.Log("=============");
        for (int i = 0; i < 4; ++i)
        {
            allAhcorPoints.Add(plane1.ClosestPointOnPlane(allAhcorPoints[i]));//内 右
        }
        //showMesh(in_ver, in_nor, in_tri);
        //return;
        //添加内表面的三角关系 找到角点对应的索引
        int[] indexOfAnchorPoints = new int[8];
        int offset = in_ver.Count;
        for (int i = 0; i < 8; ++i)
        {
            indexOfAnchorPoints[i] = offset + i;
        }
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[0]))));
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[1]))));
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[2]))));
        in_ver.Add(plane.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[3]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[4]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[5]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[6]))));
        in_ver.Add(plane1.ClosestPointOnPlane(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[7]))));
        for(int i = 0; i < 8; ++i)
        {
            in_nor.Add(cutNorsOut2[0]);
        }
        //缝合内表面 2侧面
        in_tri.Add(indexOfAnchorPoints[7]);
        in_tri.Add(indexOfAnchorPoints[5]);
        in_tri.Add(indexOfAnchorPoints[3]);

        in_tri.Add(indexOfAnchorPoints[1]);
        in_tri.Add(indexOfAnchorPoints[7]);
        in_tri.Add(indexOfAnchorPoints[3]);

        in_tri.Add(indexOfAnchorPoints[2]);
        in_tri.Add(indexOfAnchorPoints[4]);
        in_tri.Add(indexOfAnchorPoints[0]);

        in_tri.Add(indexOfAnchorPoints[4]);
        in_tri.Add(indexOfAnchorPoints[6]);
        in_tri.Add(indexOfAnchorPoints[0]);
        //showMesh(in_ver, in_nor, in_tri);
        //return;
        //Debug.Log("=============");
        //Debug.Log("in_ver = " + in_ver.Count);
        //Debug.Log("in_nor = " + in_nor.Count);
        //Debug.Log("=============");

        //生成外表面
        //List<Vector3> out_ver = new List<Vector3>(outterFace.vertices);
        //List<Vector3> out_nor = new List<Vector3>(outterFace.normals);
        //List<int> out_tri = new List<int>(outterFace.triangles);
        Vector3 dir = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[5])) - trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[3]));
        dir.Normalize();
        Vector3 point = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[5])) + dir * thickness * 0.1f;
        plane = new Plane(Inner_lineNors[0], point);
        //List<Vector3> cutPointsOut1, cutNorsOut1;
        //List<int> cutLinesOut1;
        //WholeCut(point, Inner_lineNors[0], ref out_ver, ref out_nor, ref out_tri, out cutPointsOut1, out cutNorsOut1, out cutLinesOut1);
        anchorPoints = new Vector3[4];
        Vector3 L, D, UP;
        L = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[1])) - trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[7]));
        D = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[1])) - trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[0]));
        UP = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[5])) - trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[7]));
        L.Normalize(); D.Normalize(); UP.Normalize();
        //anchorPoints[0] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[2])) + L * thickness - D * thickness;//L -D  //左侧面平移
        //anchorPoints[1] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[3])) + L * thickness + D * thickness;//L D
        //anchorPoints[2] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[1])) + L * thickness - D * thickness;//L -D
        //anchorPoints[3] = trans.InverseTransformPoint(transform.GetChild(0).transform.TransformPoint(v[0])) + L * thickness + D * thickness;//L D

        anchorPoints[3] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[5])) - L * thickness + D * thickness;//-L -D  //右侧面平移
        anchorPoints[2] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[4])) - L * thickness - D * thickness;//-L -D
        anchorPoints[1] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[6])) - L * thickness - D * thickness;//-L -D
        anchorPoints[0] = trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[7])) - L * thickness + D * thickness;//-L D
        //allAhcorPoints.AddRange(anchorPoints);//外 左
        int cnt = cutPointsOut2.Count;
        List<int> lines = new List<int>();
        lines.Add(cnt + 0); lines.Add(cnt + 1);
        lines.Add(cnt + 1); lines.Add(cnt + 2);
        lines.Add(cnt + 2); lines.Add(cnt + 3);
        lines.Add(cnt + 3); lines.Add(cnt + 0);
        lines.Reverse();

        //for (int i = 0; i < Inner_linePoints.Count; ++i)
        //{
        //    Inner_linePoints[i] = plane1.ClosestPointOnPlane(Inner_linePoints[i]);
        //}
        ////Debug.Log("out_ver:::" + out_ver.Count);
        //ts.Reverse();
        //MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);//左侧靠内
        //左侧靠外
        cutLinesOut2.Reverse();
        cutLinesOut2.AddRange(lines);
        //角点位置修正到平面位置
        for (int i = 0; i < 4; ++i)
        {
            anchorPoints[i] = plane.ClosestPointOnPlane(anchorPoints[i]);
            cutNorsOut2.Add(cutNorsOut2[0]);//补充角点的法向量
        }
        allAhcorPoints.AddRange(anchorPoints);//外 左
        cutPointsOut2.AddRange(anchorPoints);
        triangulator = new Triangulator(cutPointsOut2, cutLinesOut2, Inner_lineNors[0]);
        triangulator.Fill(out newEdges, out newTri, out newTrangleEdges);
        List<int> ts1 = new List<int>(newTri);
        //ts1.Reverse();
        //MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref cutPointsOut2, ref Inner_lineNors, ref ts1);
        MeshMerge_A_2_B(ref in_ver, ref in_nor, ref in_tri, ref cutPointsOut2, ref cutNorsOut2, ref ts1);
        //Debug.Log("=============");
        //Debug.Log("in_ver = " + in_ver.Count);
        //Debug.Log("in_nor = " + in_nor.Count);
        //Debug.Log("cutPointsOut2 = " + cutPointsOut2.Count);
        //Debug.Log("cutNorsOut2 = " + cutNorsOut2.Count);
        //Debug.Log("=============");
        //MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref in_ver, ref in_nor, ref in_tri); 
        //showMesh(in_ver, in_nor, in_tri);
        //return;
        //showMesh(out_ver, out_nor, out_tri);
        //return;

        //外左缝合
        plane = new Plane(Inner_lineNors[0], trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[2])) + L * thickness);//外左
        Inner_linePoints = new List<Vector3>(CutPosition.CutPoints[1]);//获取内网格切割环
        for (int i = 0; i < Inner_linePoints.Count; ++i)
        {
            Inner_linePoints[i] = plane.ClosestPointOnPlane(Inner_linePoints[i]);//进行投影
        }
        lineCirclesPointsL.AddRange(Inner_linePoints);
        //Inner_linePoints.Reverse();
        //修改角点
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[2])) + L * thickness - D * thickness);
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[3])) + L * thickness + D * thickness);
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[1])) + L * thickness + D * thickness);
        Inner_linePoints.Add(trans.InverseTransformPoint(transform.GetChild(1).transform.TransformPoint(v[0])) + L * thickness - D * thickness);
        //ts.Reverse();
        //showMesh(Inner_linePoints, Inner_lineNors, ts);
        //return;
        //Debug.Log("Inner_linePoints1=" + Inner_linePoints1.Count);//内外环的顶点数量不同 不可直接缝合 应使用内环的
        //Debug.Log("cutPointsOut1=" + cutPointsOut1.Count);
        //ts.Reverse();
        MeshMerge_A_2_B(ref in_ver, ref in_nor, ref in_tri, ref Inner_linePoints, ref Inner_lineNors, ref ts);
        //Debug.Log("=============");
        //Debug.Log("in_ver = " + in_ver.Count);
        //Debug.Log("in_nor = " + in_nor.Count);
        //Debug.Log("=============");
        //showMesh(out_ver, out_nor, out_tri);
        //return;
        for (int i = 0; i < 4; ++i)
        {
            Vector3 p = plane.ClosestPointOnPlane(anchorPoints[i]);
            allAhcorPoints.Add(p);
            //lineCirclesPointsL.Add(p);
        }
        //MeshMerge_A_2_B(ref out_ver, ref out_nor, ref out_tri, ref in_ver, ref in_nor, ref in_tri);//合并内外表面
        //showMesh(out_ver, out_nor, out_tri);
        //return;
        //缝合内外表面的上层、下层
        {
            cnt = in_ver.Count;
            in_ver.AddRange(allAhcorPoints);
            for (int i = 0; i < 16; ++i)
            {
                in_nor.Add(cutNorsOut2[0]);
            }
            //上层
            in_tri.Add(cnt + 3);
            in_tri.Add(cnt + 9);
            in_tri.Add(cnt + 13);

            in_tri.Add(cnt + 7);
            in_tri.Add(cnt + 9);
            in_tri.Add(cnt + 3);

            in_tri.Add(cnt + 6);
            in_tri.Add(cnt + 9);
            in_tri.Add(cnt + 7);

            in_tri.Add(cnt + 8);
            in_tri.Add(cnt + 9);
            in_tri.Add(cnt + 6);

            in_tri.Add(cnt + 8);
            in_tri.Add(cnt + 6);
            in_tri.Add(cnt + 2);

            in_tri.Add(cnt + 12);
            in_tri.Add(cnt + 8);
            in_tri.Add(cnt + 2);

            in_tri.Add(cnt + 12);
            in_tri.Add(cnt + 2);
            in_tri.Add(cnt + 3);

            in_tri.Add(cnt + 12);
            in_tri.Add(cnt + 3);
            in_tri.Add(cnt + 13);

            //下
            in_tri.Add(cnt + 1);
            in_tri.Add(cnt + 11);
            in_tri.Add(cnt + 15);

            in_tri.Add(cnt + 5);
            in_tri.Add(cnt + 11);
            in_tri.Add(cnt + 1);

            in_tri.Add(cnt + 4);
            in_tri.Add(cnt + 11);
            in_tri.Add(cnt + 5);

            in_tri.Add(cnt + 4);
            in_tri.Add(cnt + 10);
            in_tri.Add(cnt + 11);

            in_tri.Add(cnt + 10);
            in_tri.Add(cnt + 4);
            in_tri.Add(cnt + 0);

            in_tri.Add(cnt + 14);
            in_tri.Add(cnt + 10);
            in_tri.Add(cnt + 0);

            in_tri.Add(cnt + 14);
            in_tri.Add(cnt + 0);
            in_tri.Add(cnt + 1);

            in_tri.Add(cnt + 14);
            in_tri.Add(cnt + 1);
            in_tri.Add(cnt + 15);
        }
        //侧面
        {
            in_tri.Add(cnt + 15);
            in_tri.Add(cnt + 8);
            in_tri.Add(cnt + 12);

            in_tri.Add(cnt + 15);
            in_tri.Add(cnt + 11);
            in_tri.Add(cnt + 8);

            in_tri.Add(cnt + 13);
            in_tri.Add(cnt + 9);
            in_tri.Add(cnt + 14);

            in_tri.Add(cnt + 9);
            in_tri.Add(cnt + 10);
            in_tri.Add(cnt + 14);
        }
        //夹层的两个环
        //最左边的环
        List<int> linetriL;
        //Debug.Log("=============");
        //Debug.Log("in_ver = " + in_ver.Count);
        //Debug.Log("in_nor = " + in_nor.Count);
        //Debug.Log("=============");
        connectCircleHull(lineCirclesPointsL, out linetriL);
        linetriL.Reverse();
        List<Vector3> nor = new List<Vector3>(Inner_lineNor2);
        nor.AddRange(Inner_lineNor2);
        MeshMerge_A_2_B(ref in_ver, ref in_nor, ref in_tri, ref lineCirclesPointsL, ref nor, ref linetriL);
        Debug.Log("=============");
        Debug.Log("in_ver = " + in_ver.Count);
        Debug.Log("in_nor = " + in_nor.Count);
        //Debug.Log("lineCirclesPointsL = " + lineCirclesPointsL.Count);
        //Debug.Log("nor = " + nor.Count);
        Debug.Log("=============");
        //showMesh(out_ver, out_nor, out_tri);
        //Debug.Log("lineCirclesPointsL:" + lineCirclesPointsL.Count);
        //Debug.Log("Inner_lineNors:" + Inner_lineNors.Count);
        //GizomsPoint = lineCirclesPointsL;
        //showMesh(lineCirclesPointsL, Inner_lineNor1, linetriL);
        //showMesh(Inner_linePoints, Inner_lineNors, ts);

        //outterFace.vertices = out_ver.ToArray();
        //outterFace.normals = out_nor.ToArray();
        //outterFace.triangles = out_tri.ToArray();        

        innerFace.vertices = in_ver.ToArray();
        innerFace.normals = in_nor.ToArray();
        innerFace.triangles = in_tri.ToArray();
        //showMesh(in_ver, in_nor, in_tri);
        //return;
    }
    void showMesh(Mesh mesh)
    {
		GameObject go = new GameObject("showMesh");
		go.AddComponent<MeshFilter>().mesh = mesh;
		go.AddComponent<MeshRenderer>();
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
        //Debug.Log("被切割物体的顶点数：" + vertices.Length);
        Vector3[] normals = nors.ToArray();

        bool[] vertexAbovePlane;
        vertexAbovePlane = new bool[vertices.Length];
        int[] oldToNewVertexMap = new int[vertices.Length];

        List<Vector3> newVertices1 = new List<Vector3>(vertices.Length);//为了避免数组多次动态增加容量，给最大值
        //Debug.Log("newVertices1:" + newVertices1.Count);
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
        //Debug.Log("被切割物体的三角面数：" + indices.Length);
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

    private void IsolateMesh(ref List<Vector3> vers, ref List<Vector3> nors, ref List<int> tris)
    {
        Debug.Log("isolate begin~~~");
        int startTime = System.Environment.TickCount;
        Vector3[] vertices = vers.ToArray();
        Vector3[] normals = nors.ToArray();
        int[] tri = tris.ToArray();
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

            //for (int i = 0; i < count; i++)//
            //{
            //    if (newTrianglesArrays[i].Count < tri.Length / 500) continue; //小于原网格1%的小碎片忽视掉（但可能有些小碎片是有用的）

            //}
            bool tag = newTrianglesArrays[0].Count > newTrianglesArrays[1].Count;//取小的
            vers = tag ? newVerticesArrays[1] : newVerticesArrays[0];
            nors = tag ? newNormalsArrays[1] : newNormalsArrays[0];
            tris = tag ? newTrianglesArrays[1] : newTrianglesArrays[0];
        }
        else
        {
            int endTime = System.Environment.TickCount;
            Debug.Log("分离耗时：" + (endTime - startTime));
        }
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
        mesh.triangles = tri.ToArray();
        mesh.normals = nor.ToArray();
        tempShow.GetComponent<MeshRenderer>().material = test;
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
    /// <summary>
    /// 将b的网格添加到a
    /// </summary>
    /// <param name="v_a"></param>
    /// <param name="n_a"></param>
    /// <param name="t_a"></param>
    /// <param name="v_b"></param>
    /// <param name="n_b"></param>
    /// <param name="t_b"></param>
    void MeshMerge_A_2_B(ref List<Vector3> v_a,ref List<Vector3> n_a,ref List<int> t_a, ref List<Vector3> v_b, ref List<Vector3> n_b, ref List<int> t_b)
    {
        int cnt = v_a.Count;
        v_a.AddRange(v_b);
        n_a.AddRange(n_b);
        for(int i = 0; i < t_b.Count / 3; ++i)
        {
            t_a.Add(cnt + t_b[3 * i + 0]);//添加三角形
            t_a.Add(cnt + t_b[3 * i + 1]);
            t_a.Add(cnt + t_b[3 * i + 2]);
        }
    }
    /// <summary>
    /// 初始化innerface
    /// </summary>
    /// <param name="vers"></param>
    /// <param name="nors"></param>
    /// <param name="tris"></param>
    void GetMesh(ref List<Vector3> vers, ref List<Vector3> nors, ref List<int> tris)
    {
        //Debug.Log("<---打印：---->" + vers.Count);
        Vector3 cutPoint = CutPosition.cutPoint[0];//第一个切面
        Vector3 cutNormal = CutPosition.cutPlaneNormal[0];
        //Debug.Log("cutPoint:" + CutPosition.cutPoint.Count);
        //Debug.Log("cutNormal:" + CutPosition.cutPlaneNormal.Count);
        LocalAreaCut(cutPoint, cutNormal, 0, ref vers, ref nors, ref tris, out Inner_linePoints1, out Inner_lineNor1, out Inner_line1);
        //Debug.Log("Inner_lineNor1::" + Inner_lineNor1.Count);
        //Debug.Log("----------------");
        //Debug.Log("Inner_linePoints1：" + Inner_linePoints1.Count);
        //Debug.Log("Inner_line1：" + Inner_line1.Count);
        //Debug.Log("Inner_lineNor1：" + Inner_lineNor1.Count);
        //Debug.Log("----------------");
        cutPoint = CutPosition.cutPoint[1];//第二个切面
        cutNormal = CutPosition.cutPlaneNormal[1];
        LocalAreaCut(cutPoint, cutNormal, 1, ref vers, ref nors, ref tris, out Inner_linePoints2, out Inner_lineNor2, out Inner_line2);
        Debug.Log("Inner_lineNor2::" + Inner_lineNor2.Count);
        //showMesh(vers, nors, tris, "第二次切割完成的网格");
        //WholeCut(cutPoint, cutNormal, ref vers, ref nors, ref tris, out Inner_linePoints2, out Inner_lineNor2, out Inner_line2);
        innerFace = new Mesh();
        innerFace.vertices = vers.ToArray();
        innerFace.normals = nors.ToArray();
        innerFace.triangles = tris.ToArray();
        Debug.Log("GetMesh END!");
    }
    /// <summary>
    /// 局部切割
    /// 切割平面的点、法向量、角点、被切割网格数据均为局部坐标，不需要转化
    /// </summary>
    /// <param name="localPoint"></param>
    /// <param name="localSplitPlaneNormal"></param>
    /// <param name="index">局部切割的四个角点对应的下标索引</param>
    /// <param name="vers"></param>
    /// <param name="nors"></param>
    /// <param name="tris"></param>
    /// <param name="linePoints"></param>
    /// <param name="lineNor"></param>
    /// <param name="line"></param>
    private void LocalAreaCut(Vector3 localPoint, Vector3 localSplitPlaneNormal, int index, ref List<Vector3> vers, ref List<Vector3> nors, ref List<int> tris, out List<Vector3> linePoints, out List<Vector3> lineNor, out List<int> line)
    {
        
        int startTime = System.Environment.TickCount;
        //根据本切割工具网格顶点形成切割面
        localSplitPlaneNormal.Normalize();//生成法向量
        //List<Vector3> cutPlanePoints = new List<Vector3>(4);
        //cutPlanePoints.Add(beCutThing.transform.InverseTransformPoint(CutPlanePosition[0]));
        //cutPlanePoints.Add(beCutThing.transform.InverseTransformPoint(CutPlanePosition[1]));
        //cutPlanePoints.Add(beCutThing.transform.InverseTransformPoint(CutPlanePosition[2]));
        //cutPlanePoints.Add(beCutThing.transform.InverseTransformPoint(CutPlanePosition[3]));
        //cutPlanePoints = CutPosition.cutAnchorPoints[index];//取切割角点
        List<Vector3> cutPlanePoints = new List<Vector3>(CutPosition.cutAnchorPoints[index]);
        //渲染角点
        //for (int i = 0; i < cutPlanePoints.Count; ++i)
        //{
        //    GizomsPoint.Add(CutPosition.cutObject[0].transform.TransformPoint(cutPlanePoints[i]));
        //}
        //渲染cutPoint、cutNor  经验证  位置正确
        //GizomsPoint.Add(CutPosition.cutObject[0].transform.TransformPoint(CutPosition.cutPoint[0]));
        //GizomsPoint.Add(CutPosition.cutObject[0].transform.TransformPoint(CutPosition.cutPoint[1]));
        //根据切割面，把顶点分成两部分

        Vector3[] vertices = vers.ToArray();
        Vector3[] normals = nors.ToArray();
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

        int[] indices = tris.ToArray();
        int triangleCount = indices.Length / 3;
        List<int> newTriangles1 = new List<int>(indices.Length);
        //List<int> newTriangles2 = new List<int>(indices.Length);
        List<Vector3> cutEdges1 = new List<Vector3>();
        List<Vector3> cutEdges2 = new List<Vector3>();
        //Debug.Log("index=" + index);
        //Debug.Log("CutPosition.cutAnchorPoints[" + index + "]=" + CutPosition.cutAnchorPoints[index].Count);
        Debug.Log("CutPosition.cutAnchorPoints[0]=" + CutPosition.cutAnchorPoints[0].Count);
        Debug.Log("CutPosition.cutAnchorPoints[1]=" + CutPosition.cutAnchorPoints[1].Count);
        Debug.Log("cutPlanePoints=" + cutPlanePoints.Count);
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
                        Debug.Log("不在四边形区域内！！");
                        //该点不在四边形内    
                        //（Temp)(cwIntersection点)-->OK 已解决
                        newTriangles1.Add(index0);//
                        newTriangles1.Add(index1);//
                        newTriangles1.Add(index2);//
                    }
                    else
                    {
                        Debug.Log("四边形区域内！！");
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
                        Debug.Log("不在四边形区域内！！");
                        newTriangles1.Add(index0);
                        newTriangles1.Add(index1);
                        newTriangles1.Add(index2);
                    }
                    else
                    {
                        //if (newVertices1[cw] != cwIntersection ||newVertices1[ccw]!=ccwIntersection)
                        //{
                        Debug.Log("四边形区域内！！");
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
        //Debug.Log("cutEdges1.Count: " + cutEdges1.Count);
        //Debug.Log("cutEdges2.Count: " + cutEdges2.Count);


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
        //取消切面缝合

        linePoints = points;
        lineNor = new List<Vector3>();
        for (int i = 0; i < points.Count; ++i)
            lineNor.Add(localSplitPlaneNormal);
        line = outline;
        int isolateStartTime = System.Environment.TickCount;
        vers = newVertices1;
        nors = newNormals1;
        tris = newTriangles1;
        IsolateMesh(ref vers, ref nors, ref tris);
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
    /// 缝合两个经过平移得到的环之间的表面
    /// 如下图：(缝合)
    /// --------------------
    /// --------------------
    /// </summary>
    /// <param name="circle">环的坐标点</param>
    /// <param name="tri">需要输出的面</param>
    private void connectCircleHull(List<Vector3> circle, out List<int> tri)
    {
        int cnt = circle.Count / 2;
        tri = new List<int>(circle.Count);
        for(int i = 0; i < cnt - 1; ++i)
        {
            tri.Add(i);
            tri.Add(i + 1);
            tri.Add(i + cnt);

            tri.Add(i+1);
            tri.Add(i + 1 + cnt);
            tri.Add(i + cnt);
        }
        tri.Add(cnt);
        tri.Add(0);
        tri.Add(2 * cnt - 1);

        tri.Add(0);
        tri.Add(cnt);
        tri.Add(2 * cnt - 1);
    }
    void OnDrawGizmos()
    {
        //if (GizomsPoint.Count == 0)
        //    return;
        Gizmos.color = Color.blue;
        for (int i = 0; i < GizomsPoint.Count; ++i)
        {
            //Gizmos.DrawSphere(GizomsPoint[i], 0.001f);
            //switch (i)
            //{
            //    case 0: Gizmos.color = Color.red; break;
            //    case 1: Gizmos.color = Color.blue; break;
            //    case 2: Gizmos.color = Color.yellow; break;
            //    case 3: Gizmos.color = Color.black; break;
            //}
            //Gizmos.DrawSphere(CutPosition.cutObject[0].transform.TransformPoint(GizomsPoint[i]), 0.3f);
            Gizmos.DrawSphere(CutPosition.cutObject[0].transform.TransformPoint(GizomsPoint[i]), 0.0005f);
        }
    }
}
