using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;
public class LocalCutPlaneTool : MonoBehaviour
{
    private GameObject beCutThing;

    private List<Vector3> CutPlanePositon = new List<Vector3>(10);//预设大小为10的链表
    
    static bool cut_State = false;//用于判别切割状态
    // Use this for initialization
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        //记录起始、终止位置
        if(SteamVR_Actions.default_Teleport.GetStateDown(SteamVR_Input_Sources.Any) &&  SteamVR_Actions.default_GrabPinch.GetStateDown(SteamVR_Input_Sources.Any))
        {
            CutPlanePositon.Add(transform.position);//   起始位置 || 终止位置
        }


        if (Input.GetKeyDown(KeyCode.C))//读取键盘 键入C时 执行
        //if (Input.GetMouseButtonDown(1) || SteamVR_Actions.default_Teleport.GetStateDown(SteamVR_Input_Sources.Any))
        {

            if (gameObject.tag == "ChosenTool")
            {
                beCutThing = GameObject.FindGameObjectWithTag("ChosenObject");
                Debug.Log("ver=" + beCutThing.GetComponent<MeshFilter>().mesh.vertexCount);
                Debug.Log("nor=" + beCutThing.GetComponent<MeshFilter>().mesh.normals.Length);
                if (beCutThing != null)
                {
                    Debug.Log("cut begin~~~");
                    Debug.Log("顶点数：" + beCutThing.GetComponent<MeshFilter>().mesh.vertices.Length);
                    Debug.Log("三角面数：" + beCutThing.GetComponent<MeshFilter>().mesh.triangles.Length);
                    Cut();
                }
                else
                {
                    Debug.Log("there is nothing to cut!");
                }

            }
        }
    }
    
    private void Cut()
    {
        

        int startTime = System.Environment.TickCount;
        //根据本切割工具网格顶点形成切割面
        Vector3[] v = transform.GetComponent<MeshFilter>().mesh.vertices;
        Plane cutPlane = new Plane(transform.TransformPoint(v[0]), transform.TransformPoint(v[1]), transform.TransformPoint(v[v.Length - 1]));
        //Debug.Log("平面向量"+cutPlane.normal);
        Vector3[] cutPlanePoints = new Vector3[4];
        cutPlanePoints[0] = transform.TransformPoint(v[10]);//切割平面4个顶点  转化为世界坐标
        cutPlanePoints[1] = transform.TransformPoint(v[0]);
        cutPlanePoints[2] = transform.TransformPoint(v[110]);
        cutPlanePoints[3] = transform.TransformPoint(v[120]);

        //List<Vector3> CutAnchorPoints = new List<Vector3>();
        //for (int i = 0; i < 4; ++i)
        //{
        //    CutAnchorPoints.Add(transform.TransformPoint(cutPlanePoints[i]));//转化为世界坐标
        //}
        //CutPosition.CutAnchorPoints.Add(CutAnchorPoints);

        //把切割面转化为被切割物体的当地坐标 (cutPlane.normal * -cutPlane.distance)表示？
        Vector3 localPoint = beCutThing.transform.InverseTransformPoint(cutPlane.normal * -cutPlane.distance);
        //Debug.Log(localPoint);
        Vector3 localSplitPlaneNormal = beCutThing.transform.InverseTransformDirection(cutPlane.normal);
        localSplitPlaneNormal.Normalize();//生成法向量
        cutPlanePoints[0] = beCutThing.transform.InverseTransformPoint(cutPlanePoints[0]);//打印一下跟上面是否一样(temp)
        cutPlanePoints[1] = beCutThing.transform.InverseTransformPoint(cutPlanePoints[1]);
        cutPlanePoints[2] = beCutThing.transform.InverseTransformPoint(cutPlanePoints[2]);
        cutPlanePoints[3] = beCutThing.transform.InverseTransformPoint(cutPlanePoints[3]);


        List<Vector3> CutAnchorPoints = new List<Vector3>(cutPlanePoints);//存储切割角点到CutPosition
        CutPosition.cutAnchorPoints.Add(CutAnchorPoints);

        //将切割面角点进行记录
        //CutPosition.cutAnchorPoints.Add(new List<Vector3>(cutPlanePoints));//存储局部坐标

        //Debug.Log("======打印第一次切割角点(局部坐标)======");
        //Debug.Log("cutPlanePoints[0]:" + cutPlanePoints[0]);
        //Debug.Log("cutPlanePoints[1]:" + cutPlanePoints[1]);
        //Debug.Log("cutPlanePoints[2]:" + cutPlanePoints[2]);
        //Debug.Log("cutPlanePoints[3]:" + cutPlanePoints[3]);
        //Debug.Log("======打印第一次完成======");

        //Debug.Log("===========");
        //Debug.Log("cutPlanePoints[0]" + cutPlanePoints[0]);
        //Debug.Log("cutPlanePoints[1]" + cutPlanePoints[1]);
        //Debug.Log("cutPlanePoints[2]" + cutPlanePoints[2]);
        //Debug.Log("cutPlanePoints[3]" + cutPlanePoints[3]);
        //Debug.Log("===========");
        //根据切割面，把顶点分成两部分

        //用于测试
        //Debug.Log("=====判断最后的切割直线直线是否在平面cutPlane上======");
        //Vector3 vector3 = cutPlanePoints[cutPlanePoints.Length - 1] - cutPlanePoints[cutPlanePoints.Length - 2];
        //bool res = Vector3.Dot(vector3, cutPlane.normal) == 0 ? true : false;
        //if (!res)
        //    Debug.Log("Vector3.Dot(vector3, cutPlane.normal): " + Vector3.Dot(vector3, cutPlane.normal));
        //Debug.Log("结果： " + res);
        //Debug.Log("=====================================================");

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
            vertexAbovePlane[i] = abovePlane;
            //if (abovePlane)
            //{
            //    // Assign vertex to 1
            //    oldToNewVertexMap[i] = newVertices1.Count;
            //newVertices1.Add(vertex);
            //newNormals1.Add(normals[i]);
            //}
            //else
            //{
            //    // Assign vertex to 2
            //    oldToNewVertexMap[i] = newVertices2.Count;
            //    newVertices2.Add(vertex);
            //    newNormals2.Add(normals[i]);
            //}
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

        //是否添加标记的判断
        int Mark_tag = 0;
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
                newTriangles1.Add(index0);
                newTriangles1.Add(index1);
                newTriangles1.Add(index2);
            }
            else if (!above0 && !above1 && !above2)
            {
                newTriangles1.Add(index0);
                newTriangles1.Add(index1);
                newTriangles1.Add(index2);
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
                    {
                        //Debug.Log("========打印出需要分割但不符合条件的结果的值=============");
                        //Debug.Log("Vector3.Dot(v0, v1)" + Vector3.Dot(v0, v1));
                        //Debug.Log("Vector3.Dot(v0, v2)" + Vector3.Dot(v0, v2));
                        //Debug.Log("Vector3.Dot(v0, v3)" + Vector3.Dot(v0, v3));
                        //Debug.Log("=========================================================");
                        //该点不在四边形内    
                        //Debug.Log("不在四边形区域内！！");
                        //（Temp)(cwIntersection点)-->OK 已解决
                        newTriangles1.Add(index0);//
                        newTriangles1.Add(index1);//
                        newTriangles1.Add(index2);//
                    }
                    else
                    {
                        //Debug.Log("========打印出需要分割并且符合条件的结果的值=============");
                        //Debug.Log("Vector3.Dot(v0, v1)" + Vector3.Dot(v0, v1));
                        //Debug.Log("Vector3.Dot(v0, v2)" + Vector3.Dot(v0, v2));
                        //Debug.Log("Vector3.Dot(v0, v3)" + Vector3.Dot(v0, v3));
                        //Debug.Log("=========================================================");
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

                        Mark_tag++;
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
                        //Debug.Log("========打印出需要分割并且符合条件的结果的值=============");
                        //Debug.Log("Vector3.Dot(v0, v1)" + Vector3.Dot(v0, v1));
                        //Debug.Log("Vector3.Dot(v0, v2)" + Vector3.Dot(v0, v2));
                        //Debug.Log("Vector3.Dot(v0, v3)" + Vector3.Dot(v0, v3));
                        //Debug.Log("=========================================================");
                        //if (newVertices1[cw] != cwIntersection ||newVertices1[ccw]!=ccwIntersection)
                        //{
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
                        Mark_tag++;
                    }

                }

            }
        }
        if(Mark_tag!=0)
        {
            GameObject temp_Cutplane = GameObject.Instantiate(gameObject);
            temp_Cutplane.tag = "temp_cutplane_mark";//设置临时标记
            temp_Cutplane.GetComponent<MeshRenderer>().material.color = Color.red;
            temp_Cutplane.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
            temp_Cutplane.transform.SetParent(GameObject.FindGameObjectWithTag("ChosenObject").transform);
            //DestroyImmediate(temp_Cutplane.GetComponent<Choosable>());
            //DestroyImmediate(temp_Cutplane.GetComponent<Interactable>());
            //DestroyImmediate(temp_Cutplane.GetComponent<Choosable>());
            DestroyImmediate(temp_Cutplane.GetComponent<MeshCollider>());
            DestroyImmediate(temp_Cutplane.transform.GetChild(0).GetComponent<MeshCollider>());
            DestroyImmediate(temp_Cutplane.GetComponent<LocalCutPlaneTool>());
        }
        /*
         * 获取被切割下的网格数据
         */
        //GetTempMeshSaver(newVertices1, newTriangles1, newNormals1);

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
                //Debug.Log("第" + j + "个连通区域startpointcount=" + startpointcount + " endpointcount=" + endpointcount);
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
        //Debug.Log("第二个切面连通区域个数" + count);
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
                //Debug.Log("第" + j + "个连通区域startpointcount=" + startpointcount + " endpointcount=" + endpointcount);
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
        CutPosition.cutPoint.Add(cutPosition);//局部坐标系
        //CutPosition.cutPoint.Add(beCutThing.transform.TransformPoint(cutPosition));//世界坐标系
        CutPosition.cutPlaneNormal.Add(localSplitPlaneNormal);//局部坐标系
        //CutPosition.cutPlaneNormal.Add(beCutThing.transform.TransformVector(localSplitPlaneNormal));//世界坐标系
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

        //切割边数量大于1

        //if (false)
        if (points.Count > 0)             //调用triangulator
        {
            //points加入cutposition 用于生成导板使用
            CutPosition.EdgesOfCutting.Enqueue(points);
            Debug.Log("当前CutPosition.EdgesOfCutting中存在" + CutPosition.EdgesOfCutting.Count + "个元素");
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




            for (int i = 0; i < points.Count; i++)
            {
                newNormals1.Add(normalA);
                //newNormals2.Add(normalB);


            }


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


        }





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
        //if(false)
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
        int trianglationOverTime = System.Environment.TickCount;
        

        beCutThing.GetComponent<MeshFilter>().mesh.Clear();
        beCutThing.GetComponent<MeshFilter>().mesh.vertices = newVertices1.ToArray();
        beCutThing.GetComponent<MeshFilter>().mesh.normals = newNormals1.ToArray();
        beCutThing.GetComponent<MeshFilter>().mesh.triangles = newTriangles1.ToArray();
         

        
        int isolateStartTime = System.Environment.TickCount;
        Isolate.IsolateMesh(beCutThing);
        //Isolate.IsolateMesh(part2);
        //part1.tag = "Objects";
        //part2.tag = "Objects";
        int overTime = System.Environment.TickCount;
        //Debug.Log("切割总耗时：" + (overTime - startTime) + "      三角面分割耗时：" + (cutOverTime - startTime) + "            缝补耗时：" + (trianglationOverTime - cutOverTime) + "           分离耗时：" + (overTime - trianglationOverTime));

    }
    protected void SplitTriangletop1(Vector3 pointOnPlane, Vector3 planeNormal, Vector3[] vertices, Vector3[] normals, int top, int cw, int ccw, out Vector3 cwIntersection, out Vector3 ccwIntersection, out Vector3 cwNormal, out Vector3 ccwNormal)
    {

        Vector3 v0 = vertices[top];
        Vector3 v1 = vertices[cw];
        Vector3 v2 = vertices[ccw];

        // Intersect the top-cw edge with the plane
        float cwDenominator = Vector3.Dot(v1 - v0, planeNormal);
        float cwScalar = Vector3.Dot(pointOnPlane - v0, planeNormal) / cwDenominator;

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


    private void FindFromHead(List<Vector3> CutEdge, ref List<bool> visit, ref int head, int tail, ref List<Vector3> points, ref List<int> outline, ref int visitTime)
    {
        int n = CutEdge.Count / 2;
        List<Vector3> points1 = new List<Vector3>(n);
        List<int> outline1 = new List<int>(n * 2);
        int temp = points.Count;

        //if (visit[i] == true)
        //    continue;
        for (int j = 0; j < n; j++)
        {
            if (visit[j] == false && CutEdge[2 * j + 1] == CutEdge[2 * head])
            {
                visit[j] = true;
                head = j;
                outline1.Add(points1.Count + n);
                outline1.Add(points1.Count + 1 + n);
                points1.Add(CutEdge[j * 2 + 1]);
                points1.Add(CutEdge[j * 2]);
                visitTime++;
            }
        }
        points1.Reverse();//翻转
        points1.AddRange(points);
        points = points1;

        outline.AddRange(outline1);


    }
    private void FindFromTail(List<Vector3> CutEdge, ref List<bool> visit, int head,  ref int tail, ref List<Vector3> points, ref List<int> outline, ref int visitTime)
    {
        int n = CutEdge.Count / 2;
        List<Vector3> points1 = new List<Vector3>(n);
        List<int> outline1 = new List<int>(n * 2);
        int temp = points.Count;
        //if (visit[i] == true)
        //        continue;

        for (int j = 0; j < n; j++)
        {

            if (visit[j] == false && CutEdge[2 * tail + 1] == CutEdge[2 * j])
            {
                
                visit[j] = true;
                tail = j;
                outline1.Add(points1.Count + n);
                outline1.Add(points1.Count + 1 + n);
                points1.Add(CutEdge[j * 2]);
                points1.Add(CutEdge[j * 2 + 1]);
                visitTime++;
                Debug.Log("1");
            }
        }
        //Debug.Log("1");
        points.AddRange(points1);
        outline.AddRange(outline1);


    }
    private void GetTempMeshSaver(List<Vector3> vers, List<int> tris, List<Vector3> nors)
    {
        GameObject go = new GameObject("temp");
        Mesh mesh = new Mesh();
        mesh.vertices = vers.ToArray();
        mesh.normals = nors.ToArray();
        mesh.triangles = tris.ToArray();
        go.AddComponent<MeshFilter>().mesh = mesh;
        IsolateMesh(go);
    }
    private static void IsolateMesh(GameObject beIsolatedThing)
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
            Debug.Log("分离耗时：" + (endTime - startTime));
            //记录最小的网格标号 用于赋值给cutposition的TempMesh 仅限于腿骨切割
            int IndexOfMinMesh = 0;
            for (int i = 0; i < count; i++)//
            {
                if (newTrianglesArrays[i].Count < newTrianglesArrays[IndexOfMinMesh].Count)
                {
                    IndexOfMinMesh = i;
                }
                if (newTrianglesArrays[i].Count < tri.Length / 500) continue; //小于原网格1%的小碎片忽视掉（但可能有些小碎片是有用的）
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
                //part.GetComponent<MeshRenderer>().material = beIsolatedThing.GetComponent<MeshRenderer>().material;
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
        Debug.Log("CutPosition.TempMeshSaver" + (CutPosition.TempMeshSaver == null ? "为空" : "不为空"));
    }
}
