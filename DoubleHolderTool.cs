using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;
public class DoubleHolderTool : MonoBehaviour
{

    public float modelThickness;   //模型厚度
    public float distance;         //表面网格提取范围
    public Material material;
    public Transform osteotome1;
    public Transform osteotome2;

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




    //---------------------------------------------------------调试用
    ArrayList a;
    ArrayList b;
    int n;
    Mesh Mesh;
    //---------------------------------------------------------
    // Use this for initialization
    void Start()
    {

        //Vector3[] vertices = transform.GetComponent<MeshFilter>().mesh.vertices;
        //Vector3[] normals = transform.GetComponent<MeshFilter>().mesh.normals;
        ////for (int i = 0; i < vertices.Length; i++)
        ////{
        ////    Debug.Log(vertices[i]);
        ////}
        ////Debug.Log("------------------------------------------------------------");
        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    Debug.Log(transform.TransformPoint(vertices[i]));//受位置和缩放影响
        //}
        //for (int i = 0; i < normals.Length; i++)
        //{
        //    Debug.Log(normals[i]);

        //    Debug.Log(transform.TransformVector(normals[i]));//受缩放影响
        //    Debug.Log("----");
        //}


    }

    //VR

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("d") || SteamVR_Actions.default_Teleport.GetStateDown(SteamVR_Input_Sources.Any))
        {
            if (gameObject.transform.parent.tag == "ChosenTool")
            {
                beCutThing = GameObject.FindGameObjectWithTag("ChosenObject");
                if (beCutThing != null)
                {
                    cut = true;

                    GameObject shell;
                    ArrayList triDown, triUpAndOther; //顺便分别记录膜上下表面的三角面，为下一步作准备
                    GenerateShell(out shell, out triDown, out triUpAndOther);

                    ArrayList triangle1, triangle2;
                    Cut(osteotome1, shell, triDown, triUpAndOther, out triangle1, out triangle2);

                    Cut(osteotome2, shell, triangle1, triangle2, out triangle1, out triangle2);
                }
                else
                {
                    Debug.Log("there is nothing be cut!");
                }
            }



        }

        //---------------------------------------------------------调试用
        if (a != null)
        {
            //Debug.Log("cut points count:"+a.Count);
            for (int i = 0; i < a.Count - 1; i++)
            {
                Debug.DrawLine(Mesh.vertices[(int)a[i]], Mesh.vertices[(int)a[i + 1]], Color.red);
                //Debug.DrawRay(Mesh.vertices[(int)a[i]],8 * Vector3.Normalize(Mesh.vertices[(int)a[i+1]] - Mesh.vertices[(int)a[i]]));
            }
            //Debug.DrawRay(Mesh.vertices[(int)a[0]],10*( Mesh.vertices[(int)a[1]] - Mesh.vertices[(int)a[0]]));
        }
        if (b != null)
        {
            for (int i = 0; i < b.Count - 1; i++)
            {
                Debug.DrawLine(Mesh.vertices[(int)b[i]], Mesh.vertices[(int)b[i + 1]], Color.blue);
            }
        }
        //---------------------------------------------------------调试用
    }

    private void Cut(Transform osteotome, GameObject shell, ArrayList triDown, ArrayList triUpAndOther, out ArrayList triangle1, out ArrayList triangle2)
    {

        //1.首先简单判断切割板宽度是否小于被切割的模型
        //



        //2.确定切面（世界坐标） 
        Vector3[] vertices = osteotome.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] normals = osteotome.GetComponent<MeshFilter>().mesh.normals;
        Plane splitPlaneLeftDown = new Plane(osteotome.TransformPoint(vertices[0]), osteotome.TransformPoint(vertices[2]), osteotome.TransformPoint(vertices[1]));
        //Debug.Log("splitPlaneLeftDown: " + splitPlaneLeftDown);
        Plane splitPlaneRightDown = new Plane(-Vector3.Normalize(osteotome.TransformVector(normals[0])), osteotome.TransformPoint(vertices[6]));
        Plane splitPlaneLeftUp = new Plane(Vector3.Normalize(osteotome.TransformVector(normals[0])), osteotome.TransformPoint(vertices[0]) + modelThickness * Vector3.Normalize(osteotome.TransformVector(normals[0])));
        //Debug.Log("splitPlaneLeftUp:" + splitPlaneLeftUp);
        Plane splitPlaneRightUp = new Plane(-Vector3.Normalize(osteotome.TransformVector(normals[0])), osteotome.TransformPoint(vertices[6]) - modelThickness * Vector3.Normalize(osteotome.TransformVector(normals[0])));
        Plane splitPlaneMiddle = new Plane(Vector3.Normalize(osteotome.TransformVector(normals[0])), (osteotome.TransformPoint(vertices[0]) + osteotome.TransformPoint(vertices[6])) / 2);
        abovePlaneOfOsteotome = new Plane(beCutThing.transform.InverseTransformDirection(Vector3.Normalize(osteotome.TransformVector(normals[4]))), beCutThing.transform.InverseTransformPoint(osteotome.TransformPoint(vertices[4])));  //计算出相对被切割物体的坐标的平面

        underPlaneOfOsteotome = new Plane(beCutThing.transform.InverseTransformDirection(Vector3.Normalize(osteotome.TransformVector(normals[4]))), beCutThing.transform.InverseTransformPoint(osteotome.TransformPoint(vertices[7])));//计算出相对被切割物体的坐标的平面
        Vector3 fb = Vector3.Cross(Vector3.Normalize(osteotome.TransformVector(normals[0])), Vector3.Normalize(osteotome.TransformVector(normals[4])));
        forwardPlaneOfOsteotome = new Plane(-beCutThing.transform.InverseTransformDirection(fb), beCutThing.transform.InverseTransformPoint(osteotome.TransformPoint(vertices[0]))); //计算出相对被切割物体的坐标的平面
        backPlaneOfOsteotome = new Plane(beCutThing.transform.InverseTransformDirection(fb), beCutThing.transform.InverseTransformPoint(osteotome.TransformPoint(vertices[1])));//计算出相对被切割物体的坐标的平面
        outsideForwardPlaneOfOsteotome = new Plane(forwardPlaneOfOsteotome.normal, forwardPlaneOfOsteotome.ClosestPointOnPlane(Vector3.zero) + modelThickness / beCutThing.transform.lossyScale.x * Vector3.Normalize(forwardPlaneOfOsteotome.normal));
        outsideBackPlaneOfOsteotome = new Plane(backPlaneOfOsteotome.normal, backPlaneOfOsteotome.ClosestPointOnPlane(Vector3.zero) + modelThickness / beCutThing.transform.lossyScale.x * Vector3.Normalize(backPlaneOfOsteotome.normal));
        cutVector = Vector3.Normalize(osteotome.TransformVector(vertices[0]) - osteotome.TransformVector(vertices[2]));
        //Debug.Log("cutVector:" + cutVector);

        ////3.根据切面提取被切割物体的一定范围表面，并构造成膜
        //GameObject shell;
        //ArrayList triDown, triUpAndOther; //顺便分别记录膜上下表面的三角面，为下一步作准备
        //GenerateShell( out shell, out triDown, out triUpAndOther);

        //4.分别对膜的上层和下层表面左右进行切割，以空出生成导板的位置
        Mesh mesh = shell.GetComponent<MeshFilter>().mesh;
        mesh.triangles = (int[])triDown.ToArray(typeof(int));         //替换mesh的三角面为膜下层表面
        inner = true;//针对上下层表面，MeshChange方法有部分不同的处理，根据inner值来执行对应代码
        ArrayList angularPoints1 = MeshChange(splitPlaneLeftDown, splitPlaneRightDown, mesh);                                                   //MeshChange   用两个面切膜的下层表面
        triangle1 = new ArrayList(mesh.triangles);//把经过MeshChange处理过的下层表面三角面保存到triangle1
        mesh.triangles = (int[])triUpAndOther.ToArray(typeof(int));   //替换mesh的三角面为膜上层表面和缝合面
        inner = false;
        ArrayList angularPoints2 = MeshChange(splitPlaneLeftUp, splitPlaneRightUp, mesh);                                                       //MeshChange   用两个面分别切膜的上层表面
        triangle2 = new ArrayList(mesh.triangles);//把经过MeshChange处理过的上层表面三角面保存到triangle2
        ArrayList newTriangles = new ArrayList();
        newTriangles.AddRange(triangle1);

        //triangle1.AddRange(triangle2);
        //mesh.triangles = (int[])triangle1.ToArray(typeof(int));


        //5.对上下层的导板表面进行三角面缝合
        ArrayList angularPointsOfDownLeft = (ArrayList)angularPoints1[0];
        ArrayList angularPointsOfDownRight = (ArrayList)angularPoints1[1];
        ArrayList angularPointsOfUpLeft = (ArrayList)angularPoints2[0];
        ArrayList angularPointsOfUpRight = (ArrayList)angularPoints2[1];

        newTriangles.Add((int)angularPointsOfDownLeft[0]);
        newTriangles.Add((int)angularPointsOfUpLeft[0]);
        newTriangles.Add((int)angularPointsOfDownLeft[1]);
        newTriangles.Add((int)angularPointsOfUpLeft[0]);
        newTriangles.Add((int)angularPointsOfDownLeft[0]);
        newTriangles.Add((int)angularPointsOfUpLeft[1]);

        newTriangles.Add((int)angularPointsOfDownLeft[3]);
        newTriangles.Add((int)angularPointsOfDownLeft[2]);
        newTriangles.Add((int)angularPointsOfUpLeft[3]);
        newTriangles.Add((int)angularPointsOfDownLeft[3]);
        newTriangles.Add((int)angularPointsOfUpLeft[3]);
        newTriangles.Add((int)angularPointsOfUpLeft[2]);

        //////
        newTriangles.Add((int)angularPointsOfDownRight[0]);
        newTriangles.Add((int)angularPointsOfUpRight[0]);
        newTriangles.Add((int)angularPointsOfDownRight[1]);
        newTriangles.Add((int)angularPointsOfUpRight[0]);
        newTriangles.Add((int)angularPointsOfDownRight[0]);
        newTriangles.Add((int)angularPointsOfUpRight[1]);

        newTriangles.Add((int)angularPointsOfDownRight[3]);
        newTriangles.Add((int)angularPointsOfDownRight[2]);
        newTriangles.Add((int)angularPointsOfUpRight[3]);
        newTriangles.Add((int)angularPointsOfDownRight[3]);
        newTriangles.Add((int)angularPointsOfUpRight[3]);
        newTriangles.Add((int)angularPointsOfUpRight[2]);
        /////
        newTriangles.Add((int)angularPointsOfDownLeft[1]);
        newTriangles.Add((int)angularPointsOfUpLeft[0]);
        newTriangles.Add((int)angularPointsOfUpRight[1]);
        newTriangles.Add((int)angularPointsOfDownLeft[1]);
        newTriangles.Add((int)angularPointsOfUpRight[1]);
        newTriangles.Add((int)angularPointsOfDownRight[0]);

        newTriangles.Add((int)angularPointsOfDownLeft[0]);
        newTriangles.Add((int)angularPointsOfDownRight[1]);
        newTriangles.Add((int)angularPointsOfUpLeft[1]);
        newTriangles.Add((int)angularPointsOfUpLeft[1]);
        newTriangles.Add((int)angularPointsOfDownRight[1]);
        newTriangles.Add((int)angularPointsOfUpRight[0]);
        /////
        newTriangles.Add((int)angularPointsOfUpLeft[3]);
        newTriangles.Add((int)angularPointsOfDownLeft[2]);
        newTriangles.Add((int)angularPointsOfDownRight[3]);
        newTriangles.Add((int)angularPointsOfUpLeft[3]);
        newTriangles.Add((int)angularPointsOfDownRight[3]);
        newTriangles.Add((int)angularPointsOfUpRight[2]);

        newTriangles.Add((int)angularPointsOfUpRight[3]);
        newTriangles.Add((int)angularPointsOfDownRight[2]);
        newTriangles.Add((int)angularPointsOfDownLeft[3]);
        newTriangles.Add((int)angularPointsOfUpRight[3]);
        newTriangles.Add((int)angularPointsOfDownLeft[3]);
        newTriangles.Add((int)angularPointsOfUpLeft[2]);

        triangle1 = new ArrayList(newTriangles);
        newTriangles.AddRange(triangle2);
        mesh.triangles = (int[])newTriangles.ToArray(typeof(int));


    }

    private void GenerateShell(out GameObject shell, out ArrayList triDown, out ArrayList triUp)
    {
        Debug.Log("ExtractMesh");
        triDown = new ArrayList();
        triUp = new ArrayList();
        shell = new GameObject("DoubleHolder");
        shell.AddComponent<MeshFilter>();
        shell.AddComponent<MeshRenderer>();
        //MeshFilter mf = shell.GetComponent<MeshFilter>();
        Mesh newMesh = new Mesh(); //为防止变量赋值时直接引用原物体网格，导致修改变量会改变原物体网格，先new个mesh把原物体网格复制过来再赋值给变量
        newMesh.vertices = beCutThing.GetComponent<MeshFilter>().mesh.vertices;
        newMesh.triangles = beCutThing.GetComponent<MeshFilter>().mesh.triangles;
        newMesh.normals = beCutThing.GetComponent<MeshFilter>().mesh.normals;
        Vector3[] vertices = newMesh.vertices;
        Vector3[] normals = newMesh.normals;
        Vector3[] verticesInWorldSpace = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            verticesInWorldSpace[i] = beCutThing.transform.TransformPoint(vertices[i]);   //把顶点用世界坐标表示,再去提取distance范围的顶点(由于不同物体的scale不一样,这样做distance的值才方便设置)
        }
        int[] triangles = newMesh.triangles;
        ArrayList ver = new ArrayList();
        ArrayList nor = new ArrayList();
        ArrayList tri = new ArrayList();
        Vector3 theVertex = Vector3.zero;

        int[] oldToNewMap = new int[vertices.Length];        //新旧网格顶点映射
        bool[] inRange = new bool[vertices.Length];          //相应顶点是否在范围内
        ArrayList cutPointIndex = new ArrayList();

        //Vector3 point = beCutThing.transform.InverseTransformPoint(transform.position) ;
        Vector3 point = (transform.position + osteotome2.transform.position) / 2;
        for (int i = 0; i < vertices.Length; i++)            //怎样确定提取的范围？？？？？
        {
            //float distanceVerToPlane = Mathf.Abs(splitPlaneMiddle.GetDistanceToPoint(vertices[i])) ;  //顶点到切面的距离

            float distanceVerToPoint = Mathf.Sqrt((verticesInWorldSpace[i] - point).sqrMagnitude);
            if (distanceVerToPoint > distance)
            {                               //范围外的顶点
                inRange[i] = false;
                oldToNewMap[i] = -1;

            }
            else
            {
                inRange[i] = true;
                oldToNewMap[i] = ver.Count;
                ver.Add(vertices[i]);
                nor.Add(normals[i]);
            }

        }                             //顶点确定完毕

        int triangelCount = triangles.Length / 3;
        for (int i = 0; i < triangelCount; i++)
        {
            int index0 = triangles[i * 3 + 0];
            int index1 = triangles[i * 3 + 1];
            int index2 = triangles[i * 3 + 2];

            bool in0 = inRange[index0];
            bool in1 = inRange[index1];
            bool in2 = inRange[index2];

            if (in0 && in1 && in2)
            {
                tri.Add(oldToNewMap[index0]);      // 三角面添加到三角数组
                tri.Add(oldToNewMap[index1]);
                tri.Add(oldToNewMap[index2]);
            }
            else if (!in0 && !in1 && !in2)         //不在范围内则忽略
            { }
            else                                   //对于有顶点在范围内和外的三角面  对应情况处理
            {
                int top, ls, rs;

                if (in1 == in2 && in0 != in1)
                {
                    top = index0;
                    ls = index2;
                    rs = index1;
                }
                else if (in2 == in0 && in1 != in2)
                {
                    top = index1;
                    ls = index0;
                    rs = index2;
                }
                else
                {
                    top = index2;
                    ls = index1;
                    rs = index0;
                }
                Vector3 leftCutPoint = new Vector3();
                Vector3 rightCutPoint = new Vector3();
                Vector3 OT = new Vector3();
                Vector3 l = new Vector3();
                Vector3 lWorld = new Vector3();


                if (inRange[top])
                {
                    //添加一个三角面
                    float x;
                    OT = verticesInWorldSpace[top] - point;
                    l = vertices[ls] - vertices[top];
                    lWorld = verticesInWorldSpace[ls] - verticesInWorldSpace[top];
                    x = CaculateX(OT, lWorld);
                    leftCutPoint = vertices[top] + l * x;
                    l = vertices[rs] - vertices[top];
                    lWorld = verticesInWorldSpace[rs] - verticesInWorldSpace[top];
                    x = CaculateX(OT, lWorld);
                    rightCutPoint = vertices[top] + l * x;

                    tri.Add(oldToNewMap[top]);
                    tri.Add(ver.Count);
                    cutPointIndex.Add(ver.Count);      //右
                    ver.Add(rightCutPoint);
                    tri.Add(ver.Count);
                    cutPointIndex.Add(ver.Count);      //左
                    ver.Add(leftCutPoint);
                    nor.Add((normals[rs] + normals[top]) / 2);
                    nor.Add((normals[ls] + normals[top]) / 2);


                }
                else
                {
                    //添加二个三角面
                    float x;
                    OT = verticesInWorldSpace[ls] - point;
                    l = vertices[top] - vertices[ls];
                    lWorld = verticesInWorldSpace[top] - verticesInWorldSpace[ls];
                    x = CaculateX(OT, lWorld);
                    leftCutPoint = vertices[ls] + l * x;
                    OT = verticesInWorldSpace[rs] - point;
                    l = vertices[top] - vertices[rs];
                    lWorld = verticesInWorldSpace[top] - verticesInWorldSpace[rs];
                    x = CaculateX(OT, lWorld);
                    rightCutPoint = vertices[rs] + l * x;

                    tri.Add(oldToNewMap[rs]);
                    tri.Add(oldToNewMap[ls]);
                    tri.Add(ver.Count);
                    tri.Add(ver.Count);

                    ver.Add(rightCutPoint);
                    cutPointIndex.Add(ver.Count);        //左
                    cutPointIndex.Add(ver.Count - 1);        //右
                    tri.Add(oldToNewMap[ls]);
                    tri.Add(ver.Count);
                    ver.Add(leftCutPoint);
                    nor.Add((normals[rs] + normals[top]) / 2);
                    nor.Add((normals[ls] + normals[top]) / 2);


                }



            }
        }    //一面提取完毕

        normals = (Vector3[])nor.ToArray(typeof(Vector3));
        vertices = (Vector3[])ver.ToArray(typeof(Vector3));
        triangles = (int[])tri.ToArray(typeof(int));
        //modelThickness = 0.003f;
        int n = ver.Count;
        for (int i = 0; i < n; i++)
        {
            ver.Add((Vector3)ver[i] + modelThickness / beCutThing.transform.lossyScale.x * normals[i]);
            nor.Add((Vector3)nor[i]);
        }
        //--------------------------------------------------------------------------------------------------
        //为了避免尖锐边角顶点分离，但由于复杂度高可能会无法在大模型上运行
        ArrayList repetivePoint = new ArrayList();
        bool repeated = false;
        for (int i = 0; i < vertices.Length; i++)  //寻找重复点,放入数组
        {
            repetivePoint.Add(i);
            repeated = false;
            for (int j = i + 1; j < vertices.Length; j++)
            {
                if (vertices[i] == vertices[j])
                {
                    repetivePoint.Add(j);
                    if (!repeated) { repeated = true; }
                }
            }
            if (repeated)
            {
                repetivePoint.RemoveAt(repetivePoint.Count - 1);
            }

        }
        ArrayList oneSetPoint = new ArrayList();
        Vector3 v = new Vector3(0, 0, 0);
        while (repetivePoint.Count > 0)             //修正重复点的位置
        {
            oneSetPoint.Add(0);
            v = normals[(int)repetivePoint[0]];
            for (int i = 1; i < repetivePoint.Count; i++)
            {
                if (repetivePoint[0] == repetivePoint[i]) { repetivePoint.RemoveAt(i); continue; }
                if (vertices[(int)repetivePoint[0]] == vertices[(int)repetivePoint[i]])
                {
                    oneSetPoint.Add(i);
                    v = v + normals[(int)repetivePoint[i]];
                }
            }
            v = modelThickness / beCutThing.transform.lossyScale.x * v / oneSetPoint.Count * (float)1.3 + vertices[(int)repetivePoint[0]];

            for (int j = oneSetPoint.Count - 1; j >= 0; j--)
            {
                ver[(int)repetivePoint[(int)oneSetPoint[j]] + vertices.Length] = v;    //修改顶点
                repetivePoint.RemoveAt((int)oneSetPoint[j]);         //修改完的点从数组里去掉
            }
            v = Vector3.zero;                                        //为下一次循环初始化
            oneSetPoint.RemoveRange(0, oneSetPoint.Count);
        }
        //---------------------------------------------------------------------------------------------------

        tri.Reverse();       //下层翻转
        triDown.AddRange(tri);
        int m = tri.Count;
        for (int i = 0; i < m; i++)   //加上层三角面
        {
            tri.Add(triangles[i] + n);
            triUp.Add(triangles[i] + n);
        }
        m = cutPointIndex.Count / 2;
        for (int i = 0; i < m; i++)        // 缝合
        {
            tri.Add(cutPointIndex[i * 2]);
            tri.Add(cutPointIndex[i * 2 + 1]);
            tri.Add((int)cutPointIndex[i * 2 + 1] + n);

            tri.Add((int)cutPointIndex[i * 2 + 1] + n);
            tri.Add((int)cutPointIndex[i * 2] + n);
            tri.Add(cutPointIndex[i * 2]);

            triUp.Add(cutPointIndex[i * 2]);
            triUp.Add(cutPointIndex[i * 2 + 1]);
            triUp.Add((int)cutPointIndex[i * 2 + 1] + n);

            triUp.Add((int)cutPointIndex[i * 2 + 1] + n);
            triUp.Add((int)cutPointIndex[i * 2] + n);
            triUp.Add(cutPointIndex[i * 2]);
        }


        normals = (Vector3[])nor.ToArray(typeof(Vector3));
        vertices = (Vector3[])ver.ToArray(typeof(Vector3));
        triangles = (int[])tri.ToArray(typeof(int));
        shell.GetComponent<MeshFilter>().mesh.vertices = vertices;
        shell.GetComponent<MeshFilter>().mesh.normals = normals;
        shell.GetComponent<MeshFilter>().mesh.triangles = triangles;
        shell.GetComponent<MeshRenderer>().material = material;
        shell.transform.localPosition = beCutThing.transform.position;
        shell.transform.rotation = beCutThing.transform.rotation;
        shell.transform.localScale = beCutThing.transform.localScale;
        shell.AddComponent<Interactable>();
        shell.AddComponent<Choosable>();
        shell.AddComponent<MeshCollider>();
        shell.GetComponent<MeshCollider>().convex = true;
        shell.GetComponent<MeshCollider>().cookingOptions = MeshColliderCookingOptions.InflateConvexMesh;
        //shell.AddComponent<Interactable>();
        //shell.AddComponent<Throwable>();
        //shell.transform.parent = transform;

    }

    protected ArrayList MeshChange(Plane splitPlaneLeft, Plane splitPlaneRight, Mesh theMesh)
    {
        ArrayList cutPoints1 = new ArrayList();
        ArrayList cutPoints2 = new ArrayList();
        ArrayList oldTriangle = new ArrayList(theMesh.triangles);

        cutPoints1 = GetCutPoints(splitPlaneLeft, theMesh);   //切取mesh左端，记录下切割点

        ArrayList triangle1 = new ArrayList(theMesh.triangles);
        theMesh.triangles = (int[])oldTriangle.ToArray(typeof(int));

        cutPoints2 = GetCutPoints(splitPlaneRight, theMesh);   //切取mesh右端，记录下切割点

        ArrayList newTriangle = new ArrayList(theMesh.triangles);
        for (int i = 0; i < triangle1.Count; i++)     //把前面切取的两端放到一起
        {
            newTriangle.Add((int)triangle1[i]);
        }
        theMesh.triangles = (int[])newTriangle.ToArray(typeof(int));

        ArrayList cutPointLeftUp = (ArrayList)cutPoints1[0];
        ArrayList cutPointLeftDown = (ArrayList)cutPoints1[1];
        ArrayList cutPointRightUp = (ArrayList)cutPoints2[0];
        ArrayList cutPointRightDown = (ArrayList)cutPoints2[1];

        //Debug.Log("cutPointLeftUp.Count:" + cutPointLeftUp.Count);
        Mesh = theMesh;
        a = cutPointLeftUp;
        b = cutPointLeftDown;

        ArrayList angularPoints = new ArrayList();
        ArrayList angularPointsOfLeft = Mend(theMesh, cutPointLeftUp, cutPointLeftDown, "left");
        ArrayList angularPointsOfRight = Mend(theMesh, cutPointRightUp, cutPointRightDown, "right");
        angularPoints.Add(angularPointsOfLeft);
        angularPoints.Add(angularPointsOfRight);


        //左右两面的缝合，angularPointsOfLeft和angularPointsOfRight分别记录了面的4个角点
        newTriangle = new ArrayList(theMesh.triangles);

        newTriangle.Add((int)angularPointsOfLeft[1]);
        newTriangle.Add((int)angularPointsOfRight[0]);
        newTriangle.Add((int)angularPointsOfLeft[2]);

        newTriangle.Add((int)angularPointsOfLeft[2]);
        newTriangle.Add((int)angularPointsOfRight[0]);
        newTriangle.Add((int)angularPointsOfRight[3]);

        newTriangle.Add((int)angularPointsOfRight[1]);
        newTriangle.Add((int)angularPointsOfLeft[0]);
        newTriangle.Add((int)angularPointsOfRight[2]);

        newTriangle.Add((int)angularPointsOfRight[2]);
        newTriangle.Add((int)angularPointsOfLeft[0]);
        newTriangle.Add((int)angularPointsOfLeft[3]);
        theMesh.triangles = (int[])newTriangle.ToArray(typeof(int));

        return angularPoints;



    }

    protected ArrayList GetCutPoints(Plane splitPlane, Mesh theMesh)   //把splitPlane法向量那一侧的 theMesh的三角面 截取下来，顺便记录下截取面的切割点
    {
        ArrayList cutPoints = new ArrayList();
        ArrayList cutPointsIndex = new ArrayList();
        ArrayList newTriangles = new ArrayList();
        ArrayList newVertices = new ArrayList(theMesh.vertices);
        ArrayList newnormal = new ArrayList(theMesh.normals);
        bool[] vertexAbovePlane; //记录在splitPlane那一侧的顶点

        Vector3 localPoint = new Vector3();
        Vector3 localSplitPlaneNormal = new Vector3();
        Vector3 localCutVector = new Vector3();
        localPoint = beCutThing.transform.InverseTransformPoint(splitPlane.normal * -splitPlane.distance); //把各种世界坐标转化为当地坐标
        //Debug.Log(localPoint);
        localSplitPlaneNormal = beCutThing.transform.InverseTransformDirection(splitPlane.normal);
        localCutVector = beCutThing.transform.InverseTransformDirection(cutVector);
        Debug.Log("localCutVector:" + localCutVector);
        localSplitPlaneNormal.Normalize();
        localCutVector.Normalize();

        Vector3 thwartwiseNormal = Vector3.Cross(localCutVector, localSplitPlaneNormal);  //用这个向量来找 切割点 中最靠左和最靠右的
        if (!inner)
        {
            thwartwiseNormal = -thwartwiseNormal;
        }
        Debug.Log("thwartwiseNormal:" + thwartwiseNormal);




        Vector3[] vertices = theMesh.vertices;
        vertexAbovePlane = new bool[vertices.Length];
        //oldToNewVertexMap = new int[vertices.Length];
        //Debug.Log("vertices.Length=" + vertices.Length);
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];

            bool abovePlane = Vector3.Dot(vertex - localPoint, localSplitPlaneNormal) >= 0.0f;

            vertexAbovePlane[i] = abovePlane;
            //Debug.Log("vertexAbovePlane:" + vertexAbovePlane[i]);

        }



        int[] indices = theMesh.triangles;
        int triangleCount = indices.Length / 3;
        n = 0;
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
                newTriangles.Add(index0);
                newTriangles.Add(index1);
                newTriangles.Add(index2);
            }
            else if (!above0 && !above1 && !above2)
            {

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

                Vector3 cwIntersection, ccwIntersection, cutNormal0, cutNormal1;

                if (vertexAbovePlane[top])
                {
                    SplitTriangle(localPoint, localSplitPlaneNormal, theMesh, top, cw, ccw, out cwIntersection, out ccwIntersection, out cutNormal0, out cutNormal1); //0右1左
                    newTriangles.Add(top);
                    newTriangles.Add(newVertices.Count);
                    newTriangles.Add(newVertices.Count + 1);

                    cutPointsIndex.Add(newVertices.Count);
                    cutPointsIndex.Add(newVertices.Count + 1);

                    newVertices.Add(cwIntersection);
                    newVertices.Add(ccwIntersection);
                    newnormal.Add(cutNormal0);
                    newnormal.Add(cutNormal1);

                    n++;

                }
                else
                {
                    SplitTriangle(localPoint, localSplitPlaneNormal, theMesh, top, cw, ccw, out cwIntersection, out ccwIntersection, out cutNormal1, out cutNormal0); //0左1右
                    newTriangles.Add(newVertices.Count + 1);//cwIntersection
                    newTriangles.Add(cw);
                    newTriangles.Add(ccw);

                    newTriangles.Add(ccw);
                    newTriangles.Add(newVertices.Count);//ccwIntersection
                    newTriangles.Add(newVertices.Count + 1);// cwIntersection

                    cutPointsIndex.Add(newVertices.Count);
                    cutPointsIndex.Add(newVertices.Count + 1);

                    newVertices.Add(ccwIntersection);
                    newVertices.Add(cwIntersection);
                    newnormal.Add(cutNormal0);
                    newnormal.Add(cutNormal1);

                    n++;
                }

            }
        }
        Debug.Log("n=" + n);
        Debug.Log("cutPointsIndex.Count:" + cutPointsIndex.Count);//可能存在完全相同的三角面，导致切割点有重复的
        //for (int i = 1; i < cutPointsIndex.Count; i = i + 2)      //消除起始点相同的切割点对  XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        //{
        //    for (int j = i + 2; j < cutPointsIndex.Count; j = j + 2)
        //    {
        //        if ((Vector3)newVertices[(int)cutPointsIndex[i]] == (Vector3)newVertices[(int)cutPointsIndex[j]])
        //        {
        //            Debug.Log("i:" + (Vector3)newVertices[(int)cutPointsIndex[i-1]] + "----" + (Vector3)newVertices[(int)cutPointsIndex[i ]]);
        //            Debug.Log("j:" + (Vector3)newVertices[(int)cutPointsIndex[j-1]] + "----" + (Vector3)newVertices[(int)cutPointsIndex[j ]]);

        //            //cutPointsIndex.RemoveRange(j, 2);
        //            //j = j - 2;
        //        }
        //    }
        //}
        //Debug.Log("after clear repetive cut points cutPointsIndex.Count:" + cutPointsIndex.Count);
        theMesh.vertices = (Vector3[])newVertices.ToArray(typeof(Vector3));
        theMesh.normals = (Vector3[])newnormal.ToArray(typeof(Vector3));
        theMesh.triangles = (int[])newTriangles.ToArray(typeof(int));

        ArrayList cutPointUp = new ArrayList();                                       // 这里确定切点中最靠两边的两个点
        ArrayList cutPointDown = new ArrayList();
        float right = Vector3.Dot((Vector3)newVertices[(int)cutPointsIndex[0]] - localPoint, thwartwiseNormal);
        float left = right;
        if (cutPointsIndex.Count > 0)
        {
            int theRightPointIndex = (int)cutPointsIndex[0];
            int theLeftPointIndex = (int)cutPointsIndex[0];
            for (int i = 0; i < cutPointsIndex.Count; i = i + 2)
            {
                float a = Vector3.Dot((Vector3)newVertices[(int)cutPointsIndex[i]] - localPoint, thwartwiseNormal);//值越小越靠右
                if (a < right)
                {
                    right = a;
                    theRightPointIndex = (int)cutPointsIndex[i];
                }
                if (a > left)
                {
                    left = a;
                    theLeftPointIndex = (int)cutPointsIndex[i];
                }
            }
            cutPointUp.Add(theLeftPointIndex);
            cutPointDown.Add(theRightPointIndex);
            //Mesh = theMesh;
            //a = cutPointsIndex;
            //a = new ArrayList();
            //a.Add(theLeftPointIndex);
            //a.Add(theRightPointIndex);
            //Debug.Log("theLeftPointIndex:" + theLeftPointIndex);
            //Debug.Log("theRightPointIndex:" + theRightPointIndex);



            bool overUp = false;
            bool overDown = false;
            for (int n = 0; n < cutPointsIndex.Count; n++)
            {                                                 //这里分好上下两波切割点
                for (int i = 0; i < cutPointsIndex.Count; i = i + 2)
                {

                    if (!overUp && (Vector3)newVertices[(int)cutPointsIndex[i]] == (Vector3)newVertices[(int)cutPointUp[cutPointUp.Count - 1] + 1])
                    {
                        if (Vector3.Dot((Vector3)newVertices[(int)cutPointsIndex[i]] - localPoint, thwartwiseNormal) == right)
                        {
                            overUp = true;
                            cutPointUp.Add(cutPointsIndex[i]);
                            Debug.Log("get upcutpoints over");
                        }
                        else
                        {
                            cutPointUp.Add(cutPointsIndex[i]);

                        }

                    }

                    if (!overDown && (Vector3)newVertices[(int)cutPointsIndex[i]] == (Vector3)newVertices[(int)cutPointDown[cutPointDown.Count - 1] + 1])
                    {
                        if (Vector3.Dot((Vector3)newVertices[(int)cutPointsIndex[i]] - localPoint, thwartwiseNormal) == left)
                        {
                            overDown = true;
                            cutPointDown.Add(cutPointsIndex[i]);
                            Debug.Log("get downcutpoints over");
                        }
                        else
                        {
                            cutPointDown.Add(cutPointsIndex[i]);

                        }
                    }
                }
                if (overUp && overDown)
                {
                    break;
                }
            }
            //a = cutPointUp;
            //b = cutPointDown;
            cutPoints.Add(cutPointUp);
            cutPoints.Add(cutPointDown);
        }
        else { Debug.Log("There is no cut point !"); }



        return cutPoints;

    }

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
        vertices.Add(leftPlane.ClosestPointOnPlane((Vector3)vertices[(int)cutPointUp[0]]));
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

    protected void SplitTriangle(Vector3 pointOnPlane, Vector3 planeNormal, Mesh theMesh, int top, int cw, int ccw, out Vector3 cwIntersection, out Vector3 ccwIntersection, out Vector3 cwNormal, out Vector3 ccwNormal)
    {
        Vector3[] vertices = theMesh.vertices;
        Vector3[] normals = theMesh.normals;
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
        cwNormalx.z = n0.z + (n2.z - n0.z) * ccwScalar;

        ccwNormalx.Normalize();
        cwNormal = cwNormalx;
        ccwNormal = ccwNormalx;


        cwIntersection = cwVertex;
        ccwIntersection = ccwVertex;
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
}
