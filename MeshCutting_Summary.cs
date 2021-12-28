using ImgSpc.Exporters;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// 汇总的处理网格切割等操作的类
/// </summary>
public class MeshCutting_Summary : MonoBehaviour {
    public Material test = null;
    /// <summary>
    /// 应用于圆柱切割
    /// 无切面缝合
    /// </summary>
    /// <param name="vers"></param>
    /// <param name="tris"></param>
    /// <param name="nors"></param>
    /// <param name="tf"></param>
    /// <param name="cutEdge_Output">用于输出到外部 后期缝合使用</param>
    /// <param name="CutPlaneEdgePoints">WPosition</param>
    public static void LocalCutting(ref List<Vector3> vers, ref List<int> tris, ref List<Vector3> nors, Transform tf, Vector3[] CutPlaneEdgePoints ,ref List<Vector3> cutEdge_Output)
    {
        int startTime = System.Environment.TickCount;
        //根据本切割工具网格顶点形成切割面
        Plane cutPlane = new Plane(CutPlaneEdgePoints[0], CutPlaneEdgePoints[1], CutPlaneEdgePoints[2]);


        //把切割面转化为被切割物体的当地坐标 (cutPlane.normal * -cutPlane.distance)表示？
        Vector3 localPoint = tf.InverseTransformPoint(cutPlane.normal * -cutPlane.distance);
        //Debug.Log(localPoint);
        Vector3 localSplitPlaneNormal = tf.InverseTransformDirection(cutPlane.normal);
        localSplitPlaneNormal.Normalize();//生成法向量
        List<Vector3> cutPlanePoints = new List<Vector3>(4);
        cutPlanePoints.Add(tf.InverseTransformPoint(CutPlaneEdgePoints[0]));
        cutPlanePoints.Add(tf.InverseTransformPoint(CutPlaneEdgePoints[1]));
        cutPlanePoints.Add(tf.InverseTransformPoint(CutPlaneEdgePoints[2]));
        cutPlanePoints.Add(tf.InverseTransformPoint(CutPlaneEdgePoints[3]));



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
                    //判断两个交叉点
                    Vector3 v0 = Vector3.Cross(cutPlanePoints[0] - cutPlanePoints[3], cwIntersection - cutPlanePoints[3]);
                    Vector3 v1 = Vector3.Cross(cutPlanePoints[1] - cutPlanePoints[0], cwIntersection - cutPlanePoints[0]);
                    Vector3 v2 = Vector3.Cross(cutPlanePoints[2] - cutPlanePoints[1], cwIntersection - cutPlanePoints[1]);
                    Vector3 v3 = Vector3.Cross(cutPlanePoints[3] - cutPlanePoints[2], cwIntersection - cutPlanePoints[2]);


                    Vector3 v10 = Vector3.Cross(cutPlanePoints[0] - cutPlanePoints[3], ccwIntersection - cutPlanePoints[3]);
                    Vector3 v11 = Vector3.Cross(cutPlanePoints[1] - cutPlanePoints[0], ccwIntersection - cutPlanePoints[0]);
                    Vector3 v12 = Vector3.Cross(cutPlanePoints[2] - cutPlanePoints[1], ccwIntersection - cutPlanePoints[1]);
                    Vector3 v13 = Vector3.Cross(cutPlanePoints[3] - cutPlanePoints[2], ccwIntersection - cutPlanePoints[2]);
                    if ((Vector3.Dot(v0, v1) < 0 || Vector3.Dot(v0, v2) < 0 || Vector3.Dot(v0, v3) < 0) 
                        && (Vector3.Dot(v10, v11) < 0 || Vector3.Dot(v10, v12) < 0 || Vector3.Dot(v10, v13) < 0))//强化切割条件 一个交点在其中时不做切割
                    //if (Vector3.Dot(v0, v1) < Min_Judger || Vector3.Dot(v0, v2) < Min_Judger || Vector3.Dot(v0, v3) < Min_Judger)
                    {
                        //Debug.Log("不在四边形区域内！！");
                        //该点不在四边形内    
                        //（Temp)(cwIntersection点)-->OK 已解决
                        newTriangles1.Add(index0);//
                        newTriangles1.Add(index1);//
                        newTriangles1.Add(index2);//
                    }
                    else
                    {
                        //Debug.Log("四边形区域内,进行切割！！");
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
                    //判断两个交叉点
                    Vector3 v0 = Vector3.Cross(cutPlanePoints[0] - cutPlanePoints[3], cwIntersection - cutPlanePoints[3]);
                    Vector3 v1 = Vector3.Cross(cutPlanePoints[1] - cutPlanePoints[0], cwIntersection - cutPlanePoints[0]);
                    Vector3 v2 = Vector3.Cross(cutPlanePoints[2] - cutPlanePoints[1], cwIntersection - cutPlanePoints[1]);
                    Vector3 v3 = Vector3.Cross(cutPlanePoints[3] - cutPlanePoints[2], cwIntersection - cutPlanePoints[2]);

                    Vector3 v10 = Vector3.Cross(cutPlanePoints[0] - cutPlanePoints[3], ccwIntersection - cutPlanePoints[3]);
                    Vector3 v11 = Vector3.Cross(cutPlanePoints[1] - cutPlanePoints[0], ccwIntersection - cutPlanePoints[0]);
                    Vector3 v12 = Vector3.Cross(cutPlanePoints[2] - cutPlanePoints[1], ccwIntersection - cutPlanePoints[1]);
                    Vector3 v13 = Vector3.Cross(cutPlanePoints[3] - cutPlanePoints[2], ccwIntersection - cutPlanePoints[2]);
                    if ((Vector3.Dot(v0, v1) < 0 || Vector3.Dot(v0, v2) < 0 || Vector3.Dot(v0, v3) < 0)
                        && (Vector3.Dot(v10, v11) < 0 || Vector3.Dot(v10, v12) < 0 || Vector3.Dot(v10, v13) < 0))
                    //if (Vector3.Dot(v0, v1) < 0 || Vector3.Dot(v0, v2) < 0 || Vector3.Dot(v0, v3) < 0)//即：在平面外不切割
                    //if (Vector3.Dot(v0, v1) < Min_Judger || Vector3.Dot(v0, v2) < Min_Judger || Vector3.Dot(v0, v3) < Min_Judger)//即：在平面外不切割
                    {
                        
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
                        //Debug.Log("四边形区域内,进行切割！！");
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

        //将cutEdges1输出到cutEdge_Output中
        //注意:需要转化为世界坐标？？？先试试不转化
        cutEdge_Output.AddRange(cutEdges1);

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
        //Debug.Log("第一个切面连通区域个数" + count);
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


        }


        {//用于添加cutEdge_Output
            int n = cutEdge_Output.Count;
            if (cutEdge_Output.Count > 0)
            {
                cutEdge_Output.AddRange(points);
                for (int i=0;i<n; ++i)
                {
                    
                }
            }
                
        }

        //取消切面缝合
        /*
        
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




            for (int i = 0; i < points.Count; i++)
            {
                newNormals1.Add(normalA);
                //newNormals2.Add(normalB);

                //临时记录
                //temp_Nor_List.Add(normalA);

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
        */  //结束

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

        }

        /*
         * 
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
        */ //结束




        int trianglationOverTime = System.Environment.TickCount;



        int isolateStartTime = System.Environment.TickCount;
        Mesh mesh = new Mesh();
        //mesh.vertices = vers.ToArray();
        //mesh.normals = nors.ToArray();
        //mesh.triangles = tris.ToArray();
        vers = newVertices1;
        nors = newNormals1;
        tris = newTriangles1;
        
        
        int overTime = System.Environment.TickCount;
        //Debug.Log("切割总耗时：" + (overTime - startTime) + "      三角面分割耗时：" + (cutOverTime - startTime) + "            缝补耗时：" + (trianglationOverTime - cutOverTime) + "           分离耗时：" + (overTime - trianglationOverTime));
        
    }
    /// <summary>
    /// 应用于圆柱切割
    /// 无切面缝合
    /// </summary>
    /// <param name="vers"></param>
    /// <param name="tris"></param>
    /// <param name="nors"></param>
    /// <param name="tf"></param>
    /// <param name="cutEdge_Output">用于输出到外部 后期缝合使用</param>
    /// <param name="CutPlaneEdgePoints">WPosition</param>
    public static void LocalCutting_Triangularchip(ref List<Vector3> vers, ref List<int> tris, ref List<Vector3> nors, Transform tf, Vector3[] CutPlaneEdgePoints ,ref List<Vector3> cutEdge_Output)
    {
        int startTime = System.Environment.TickCount;
        //根据本切割工具网格顶点形成切割面
        Plane cutPlane = new Plane(CutPlaneEdgePoints[0], CutPlaneEdgePoints[1], CutPlaneEdgePoints[2]);


        //把切割面转化为被切割物体的当地坐标 (cutPlane.normal * -cutPlane.distance)表示？
        Vector3 localPoint = tf.InverseTransformPoint(cutPlane.normal * -cutPlane.distance);
        //Debug.Log(localPoint);
        Vector3 localSplitPlaneNormal = tf.InverseTransformDirection(cutPlane.normal);
        localSplitPlaneNormal.Normalize();//生成法向量
        List<Vector3> cutPlanePoints = new List<Vector3>(4);
        cutPlanePoints.Add(tf.InverseTransformPoint(CutPlaneEdgePoints[0]));
        cutPlanePoints.Add(tf.InverseTransformPoint(CutPlaneEdgePoints[1]));
        cutPlanePoints.Add(tf.InverseTransformPoint(CutPlaneEdgePoints[2]));



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
                Debug.Log("不同向 需要切割");
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
                    //判断两个交叉点
                    Vector3 v0 = Vector3.Cross(cutPlanePoints[0] - cutPlanePoints[2], cwIntersection - cutPlanePoints[2]);
                    Vector3 v1 = Vector3.Cross(cutPlanePoints[1] - cutPlanePoints[0], cwIntersection - cutPlanePoints[0]);
                    Vector3 v2 = Vector3.Cross(cutPlanePoints[2] - cutPlanePoints[1], cwIntersection - cutPlanePoints[1]); 


                    Vector3 v10 = Vector3.Cross(cutPlanePoints[0] - cutPlanePoints[2], ccwIntersection - cutPlanePoints[2]);
                    Vector3 v11 = Vector3.Cross(cutPlanePoints[1] - cutPlanePoints[0], ccwIntersection - cutPlanePoints[0]);
                    Vector3 v12 = Vector3.Cross(cutPlanePoints[2] - cutPlanePoints[1], ccwIntersection - cutPlanePoints[1]);
                    if ((Vector3.Dot(v0, v1) < 0 || Vector3.Dot(v0, v2) < 0) 
                        && (Vector3.Dot(v10, v11) < 0 || Vector3.Dot(v10, v12) < 0 ))//强化切割条件 一个交点在其中时不做切割
                    //if (Vector3.Dot(v0, v1) < Min_Judger || Vector3.Dot(v0, v2) < Min_Judger || Vector3.Dot(v0, v3) < Min_Judger)
                    {
                        Debug.Log("不在四边形区域内！！");
                        //该点不在四边形内    
                        //（Temp)(cwIntersection点)-->OK 已解决
                        newTriangles1.Add(index0);//
                        newTriangles1.Add(index1);//
                        newTriangles1.Add(index2);//
                    }
                    else
                    {
                        Debug.Log("四边形区域内,进行切割！！");
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
                    //判断两个交叉点
                    Vector3 v0 = Vector3.Cross(cutPlanePoints[0] - cutPlanePoints[2], cwIntersection - cutPlanePoints[2]);
                    Vector3 v1 = Vector3.Cross(cutPlanePoints[1] - cutPlanePoints[0], cwIntersection - cutPlanePoints[0]);
                    Vector3 v2 = Vector3.Cross(cutPlanePoints[2] - cutPlanePoints[1], cwIntersection - cutPlanePoints[1]);


                    Vector3 v10 = Vector3.Cross(cutPlanePoints[0] - cutPlanePoints[2], ccwIntersection - cutPlanePoints[2]);
                    Vector3 v11 = Vector3.Cross(cutPlanePoints[1] - cutPlanePoints[0], ccwIntersection - cutPlanePoints[0]);
                    Vector3 v12 = Vector3.Cross(cutPlanePoints[2] - cutPlanePoints[1], ccwIntersection - cutPlanePoints[1]);
                    if ((Vector3.Dot(v0, v1) < 0 || Vector3.Dot(v0, v2) < 0 )
                        && (Vector3.Dot(v10, v11) < 0 || Vector3.Dot(v10, v12) < 0 ))
                    //if (Vector3.Dot(v0, v1) < 0 || Vector3.Dot(v0, v2) < 0 || Vector3.Dot(v0, v3) < 0)//即：在平面外不切割
                    //if (Vector3.Dot(v0, v1) < Min_Judger || Vector3.Dot(v0, v2) < Min_Judger || Vector3.Dot(v0, v3) < Min_Judger)//即：在平面外不切割
                    {
                        
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
                        Debug.Log("四边形区域内,进行切割！！");
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

        //将cutEdges1输出到cutEdge_Output中
        //注意:需要转化为世界坐标？？？先试试不转化
        cutEdge_Output.AddRange(cutEdges1);

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
        //Debug.Log("第一个切面连通区域个数" + count);
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


        }


        {//用于添加cutEdge_Output
            int n = cutEdge_Output.Count;
            if (cutEdge_Output.Count > 0)
            {
                cutEdge_Output.AddRange(points);
                for (int i=0;i<n; ++i)
                {
                    
                }
            }
                
        }

        //取消切面缝合
        /*
        
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




            for (int i = 0; i < points.Count; i++)
            {
                newNormals1.Add(normalA);
                //newNormals2.Add(normalB);

                //临时记录
                //temp_Nor_List.Add(normalA);

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
        */  //结束

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

        }

        /*
         * 
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
        */ //结束




        int trianglationOverTime = System.Environment.TickCount;



        int isolateStartTime = System.Environment.TickCount;
        Mesh mesh = new Mesh();
        //mesh.vertices = vers.ToArray();
        //mesh.normals = nors.ToArray();
        //mesh.triangles = tris.ToArray();
        vers = newVertices1;
        nors = newNormals1;
        tris = newTriangles1;
        
        
        int overTime = System.Environment.TickCount;
        Debug.Log("切割总耗时：" + (overTime - startTime) + "      三角面分割耗时：" + (cutOverTime - startTime) + "            缝补耗时：" + (trianglationOverTime - cutOverTime) + "           分离耗时：" + (overTime - trianglationOverTime));
        
    }

    protected static void SplitTriangletop1(Vector3 pointOnPlane, Vector3 planeNormal, Vector3[] vertices, Vector3[] normals, int top, int cw, int ccw, out Vector3 cwIntersection, out Vector3 ccwIntersection, out Vector3 cwNormal, out Vector3 ccwNormal)
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
    protected static void SplitTriangletop0(Vector3 pointOnPlane, Vector3 planeNormal, Vector3[] vertices, Vector3[] normals, int top, int cw, int ccw, out Vector3 cwIntersection, out Vector3 ccwIntersection, out Vector3 cwNormal, out Vector3 ccwNormal)
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

    private static Vector3 GetDifferentVector3(Vector3 v)//在float有效数字最后一位加1（使切割产生的重复点不重复）
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
    private static Vector3 GetDifferentVector3one(Vector3 v1, Vector3 v2)//（使切割产生的重复点不重复）
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
    
    /// <param name="vers"></param>
    /// <param name="nors"></param>
    /// <param name="tris"></param>
    /// <param name="tf">被处理网格对应的物体TF</param>
    public static void IsolateMesh(ref List<Vector3> vers, ref List<Vector3> nors, ref List<int> tris, Transform tf)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vers.ToArray();
        mesh.normals = nors.ToArray();
        mesh.triangles = tris.ToArray();
        IsolateMesh(ref mesh, tf);
        vers = new List<Vector3>(mesh.vertices);
        nors = new List<Vector3>(mesh.normals);
        tris = new List<int>(mesh.triangles);
    }
    public static void IsolateMesh(ref Mesh mesh,Transform tf)
    {
        Debug.Log("isolate begin~~~");
        int startTime = System.Environment.TickCount;
        //Mesh mesh = beIsolatedThing.GetComponent<MeshFilter>().mesh;
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
            Debug.Log("count>=2!!!!");
            ///*
            ///<summary>
            ///先清除切片标记
            ///</summary>
            //GameObject[] cutplanemark = GameObject.FindGameObjectsWithTag("temp_cutplane_mark");
            //for (int i = 0; i < cutplanemark.Length; i++)
            //{
            //    DestroyImmediate(cutplanemark[i]);//删除标记
            //}
            //*/
            List<int>[] arrays = union.getResult();
            int[] oldToNewVertexMap = new int[vertices.Length];
            int[] partNumber = new int[vertices.Length];

            List<Vector3>[] newVerticesArrays = new List<Vector3>[count];
            List<Vector3>[] newNormalsArrays = new List<Vector3>[count];
            List<int>[] newTrianglesArrays = new List<int>[count];

            for (int i = 0; i < count; i++) //初始化新顶点数组
            {
                newVerticesArrays[i] = new List<Vector3 >(vertices.Length / 2);
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
            

            ////清理所有被选中物体 因为可能存在复制问题 导致标签重复 2020.08.08
            //GameObject[] ChosenObjects = GameObject.FindGameObjectsWithTag("ChosenObject");
            //for (int i = 0; i < ChosenObjects.Length; i++)
            //    ChosenObjects[i].tag = "Objects";

            //GameObject temp = null;
            for (int i = 0; i < count; i++)//
            {
                //if (newTrianglesArrays[i].Count < tri.Length / 500) continue; //小于原网格1%的小碎片忽视掉（但可能有些小碎片是有用的）
                num++;
                //GameObject part = new GameObject(beIsolatedThing.name + i.ToString());
                ///<summary>需要删除原物体碰撞器 再重新生成</summary>
                /*
                 * 
                GameObject part = new GameObject();

                ////初始化n
                //n = part.GetComponent<MeshFilter>().mesh.vertices.Length;

                //Destroy(part.GetComponent<MeshCollider>());
                //part.AddComponent<MeshCollider>();
                num++;
                part.name = tf.gameObject.name + "[" + i.ToString() + "]";
                //part.AddComponent<MeshFilter>();
                //part.AddComponent<MeshRenderer>();

                part.GetComponent<MeshFilter>().mesh.Clear();
                part.GetComponent<MeshFilter>().mesh.vertices = newVerticesArrays[i].ToArray();
                part.GetComponent<MeshFilter>().mesh.normals = newNormalsArrays[i].ToArray();
                part.GetComponent<MeshFilter>().mesh.triangles = newTrianglesArrays[i].ToArray();
                part.GetComponent<MeshRenderer>().material = tf.gameObject.GetComponent<MeshRenderer>().material;
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
                */



                //if (num == 1)
                //{
                //    GameObject gg = GameObject.FindGameObjectWithTag("ChosenObject");
                //    if (gg != null)
                //    {
                //        part.tag = "Objects";
                //    }
                //    part.tag = "ChosenObject";
                //}
                //else
                //{
                //    //part.tag = "Objects";
                //}

                //part.AddComponent<MeshCollider>();
                //part.GetComponent<MeshCollider>().convex = true;
                //part.GetComponent<MeshCollider>().cookingOptions = MeshColliderCookin`gOptions.InflateConvexMesh;
            }
            //记录数据
            //if (num > 1)
            //{
            //    CutPosition.DoubleHolderToolHelper = GameObject.Instantiate(temp);
            //    CutPosition.DoubleHolderToolHelper.name = "DoubleHolderToolHelper";



            //    CutPosition.DoubleHolderToolHelper.GetComponent<MeshRenderer>().material = Transparent;
            //    DestroyImmediate(CutPosition.DoubleHolderToolHelper.GetComponent<MeshCollider>());
            //    DestroyImmediate(CutPosition.DoubleHolderToolHelper.GetComponent<Choosable>());
            //    DestroyImmediate(CutPosition.DoubleHolderToolHelper.GetComponent<Interactable>());
            //}

            //tf.gameObject.SetActive(false);

            int maxIndex = 0;
            int maxCount = 0;
            for(int i=0;i<count;++i)
            {
                if (newVerticesArrays[i].Count > maxCount)
                {
                    maxCount = newVerticesArrays[i].Count;
                    maxIndex = i;
                }
                Debug.Log("最大的网格索引'maxIndex'为:  " + maxIndex);
            }
            Debug.Log("=======");
            Debug.Log("顶点" + mesh.vertices.Length);
            Debug.Log("法向量" + mesh.normals.Length);
            Debug.Log("=======");
            mesh.vertices = new List<Vector3>(newVerticesArrays[maxIndex]).ToArray();
            mesh.normals = new List<Vector3>(newNormalsArrays[maxIndex]).ToArray();
            mesh.triangles = new List<int>(newTrianglesArrays[maxIndex]).ToArray();
        }
        else
        {
            int endTime = System.Environment.TickCount;
            Debug.Log("分离耗时：" + (endTime - startTime));
        }
    }
    public static void IsolateMesh1(GameObject beIsolatedThing)
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
                //if (newTrianglesArrays[i].Count < tri.Length / 500) continue; //小于原网格1%的小碎片忽视掉（但可能有些小碎片是有用的）
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
    /// <summary>
    /// 网格分离 处理时 加入对(切割导致的)网格碎片的整合操作
    /// </summary>
    /// <param name="beIsolatedThing"></param>
    public static void IsolateMesh(GameObject beIsolatedThing)//修改版
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
        for (int i = 0; i < vertices.Length; i++) //连通重复点 建立两者连接
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

        int count = union.getCount();
        Debug.Log("连通分量为: " + count);

        if (count >= 2)              //分离
        {
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
            //Debug.Log("@@@@@1");
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
            //Debug.Log("@@@@@2");

            for (int i = 0; i < tri.Length; i = i + 3)//映射 旧三角面与新三角面
            {
                int index = partNumber[tri[i]];

                newTrianglesArrays[index].Add(oldToNewVertexMap[tri[i]]);
                newTrianglesArrays[index].Add(oldToNewVertexMap[tri[i + 1]]);
                newTrianglesArrays[index].Add(oldToNewVertexMap[tri[i + 2]]);
            }
            //Debug.Log("@@@@@3");
            
            /*
             * 设置一个数组 存放所有物体的引用 
             * 解决小块三角网格与大网格分离的情况
             */

            //List<Vector3> add_vers = new List<Vector3>();
            //List<Vector3> add_nors = new List<Vector3>();
            //List<int> add_tris = new List<int>();
            //Debug.Log("####");
            int firstInd = -1, secondInd = -1, thirdInd = -1;
            //Node[] arr = new Node[count];
            //for(int i = 0; i < count; ++i)
            //{
            //    arr[i] = new Node(newVerticesArrays[i].Count, i);//顶点数_下标索引  对应关系
            //}
            //heap.HeapSortFunction(ref arr);//执行堆排序

            //showMesh(new List<Vector3>(newVerticesArrays[arr[count - 1].index]), 
            //    new List<Vector3>(newNormalsArrays[arr[count - 1].index]),
            //    newTrianglesArrays[arr[count - 1].index]);
            //return;
            //firstInd = arr[count - 1].index;
            //secondInd = arr[count - 2].index;
            //thirdInd = arr[count - 3].index;
            List<Vector3> add_vers = new List<Vector3>(newVerticesArrays[0]);
            List<Vector3> add_nors = new List<Vector3>(newNormalsArrays[0]);
            List<int> add_tris = new List<int>(newTrianglesArrays[0]);
            for(int i = 3; i < count; ++i)
            {
                MeshMerge_A_2_B(ref add_vers, ref add_nors, ref add_tris, newVerticesArrays[i], newNormalsArrays[i], newTrianglesArrays[i]);
            }
            Debug.Log("ver大小为:" + add_vers.Count);
            Debug.Log("nor大小为:" + add_nors.Count);
            Debug.Log("tri大小为:" + add_tris.Count);
            //MeshMerge_A_2_B(ref add_vers, ref add_nors, ref add_tris, 
            //    newVerticesArrays[firstInd], newNormalsArrays[firstInd], newTrianglesArrays[firstInd]);
            //for (int i = 0; i < count - 3; ++i)
            //{
            //    MeshMerge_A_2_B(ref add_vers, ref add_nors, ref add_tris, 
            //        newVerticesArrays[arr[i].index], newNormalsArrays[arr[i].index], newTrianglesArrays[arr[i].index]);
            //}
            

            GameObject result = Instantiate(beIsolatedThing, null);
            result.name = "result";
            Mesh mesh1 = result.GetComponent<MeshFilter>().mesh;
            mesh1.Clear();
            mesh1.vertices = add_vers.ToArray();
            mesh1.normals = add_nors.ToArray();
            mesh1.triangles = add_tris.ToArray();
            Destroy(result.GetComponent<MeshCollider>());
            result.AddComponent<MeshCollider>();
            beIsolatedThing.SetActive(false);
            Debug.Log("~~~~设置隐藏~~~");
            int endTime = System.Environment.TickCount;
            Debug.Log("分离耗时1：" + (endTime - startTime));
        }
        else
        {
            int endTime = System.Environment.TickCount;
            Debug.Log("分离耗时2：" + (endTime - startTime));
        }
        //beIsolatedThing.SetActive(false);
        //Debug.Log("~~~~设置隐藏~~~");
    }
    public static void showMesh(List<Vector3> ver, List<Vector3> nor, List<int> tri)
    {
        string name = "tempShow";
        showMesh(ver, nor, tri, name);
    }
    public static void showMesh(List<Vector3> ver, List<Vector3> nor, List<int> tri, string name)
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
        //tempShow.GetComponent<MeshRenderer>().material = test;
    }

    /// <summary>
    /// 合并网格
    /// </summary>
    /// <param name="v_a"></param>
    /// <param name="n_a"></param>
    /// <param name="t_a"></param>
    /// <param name="v_b"></param>
    /// <param name="n_b"></param>
    /// <param name="t_b"></param>
    public static void MeshMerge_A_2_B(ref List<Vector3> v_a, ref List<Vector3> n_a,ref List<int> t_a, List<Vector3> v_b,  List<Vector3> n_b,  List<int> t_b)
    {
        int cnt = v_a.Count;
        v_a.AddRange(v_b);
        n_a.AddRange(n_b);
        for (int i = 0; i < t_b.Count / 3; ++i)
        {
            t_a.Add(cnt + t_b[3 * i + 0]);//添加三角形
            t_a.Add(cnt + t_b[3 * i + 1]);
            t_a.Add(cnt + t_b[3 * i + 2]);
        }
    }
    /// <summary>
    /// 复制网格并缝合单面圆柱
    /// </summary>
    public static void CopyAndStitch(GameObject Go)
    {
        Mesh mesh = Go.GetComponent<MeshFilter>().mesh;
        List<Vector3> vers = new List<Vector3>(mesh.vertices);
        List<Vector3> nors = new List<Vector3>(mesh.normals);
        List<int> tris = new List<int>(mesh.triangles);

        List<List<int>> circles = null;
        

    }
    public static void StlExporter(GameObject ObjectToBeEXP)
    {
        ImgSpcExporter imgSpcExporter = new ImgSpcExporter();
        //string path = Application.dataPath + "/ExportFile/" + "第" + Export_num + "个导出的文件.stl";
        System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
        string path = "";
        dialog.Description = "请选择导出文件路径";   // 设置窗体的标题
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)   // 窗体打开成功
        {
            path = dialog.SelectedPath;  // 获取文件的路径
                                         //MessageBox.Show("已选择文件夹:" + path, "选择文件夹提示", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }
        path = path + "\\";
        if (Directory.Exists(path))
        {
            Directory.CreateDirectory(path + "ExportFile");
        }
        Debug.Log("path: " + path);
        path = path + "/" + ObjectToBeEXP + ".stl";
        Debug.Log("path: " + path);
        imgSpcExporter.SetFilename(path);//设置导出文件夹
        //执行导出操作
        Debug.Log("ObjectToBeEXP:" + (ObjectToBeEXP == null ? "yes" : "no"));
        if (ObjectToBeEXP != null)
        {
            imgSpcExporter.ObjectsToExport = new Object[1];
            imgSpcExporter.ObjectsToExport[0] = ObjectToBeEXP;
            imgSpcExporter.Export();
            Debug.Log("已经导出！！");
        }
        else
        {
            Debug.Log("未找到物体");
        }
    }

    /*
     * 作用:
     * 将传入的顶点坐标进行分离和去重
     * 
     */ 
    public static void DistinctAndCircle(List<Vector3> cutEdge,out List<List<Vector3>> loops)
    //public static void DistinctAndCircle(List<Vector3> cutEdge,ref List<List<Vector3>> loops)
    {
        loops = new List<List<Vector3>>();
        int edgeCount = cutEdge.Count / 2;
        List<Vector3>  loop = new List<Vector3>(edgeCount);
        int start = 0;
        for (int current = 0; current < edgeCount; current++)
        {
            int next = current + 1;
            // Find the next edge
            int nearest = start;
            float nearestDistance = (cutEdge[current * 2 + 1] - cutEdge[start * 2 + 0]).sqrMagnitude;

            for (int other = next; other < edgeCount; other++)
            {
                float distance = (cutEdge[current * 2 + 1] - cutEdge[other * 2 + 0]).sqrMagnitude;

                if (distance < nearestDistance)
                {
                    nearest = other;
                    nearestDistance = distance;
                }
            }

            // Is the current edge the last edge in this edge loop?
            if (nearest == start && current > start)
            {
                int pointStart = loop.Count;
                int pointCounter = pointStart;

                // Add this edge loop to the triangulation lists
                for (int edge = start; edge < current; edge++)
                {
                    loop.Add(cutEdge[edge * 2 + 0]);
                    //outline.Add(pointCounter++);
                    //outline.Add(pointCounter);
                }

                loop.Add(cutEdge[current * 2 + 0]);
                //outline.Add(pointCounter);
                //outline.Add(pointStart);


                // Start a new edge loop
                loop = new List<Vector3>();
                loops.Add(loop);
                start = next;
            }
            else if (next < edgeCount)
            {
                // Move the nearest edge so that it follows the current edge
                Vector3 n0 = cutEdge[next * 2 + 0];
                Vector3 n1 = cutEdge[next * 2 + 1];

                cutEdge[next * 2 + 0] = cutEdge[nearest * 2 + 0];
                cutEdge[next * 2 + 1] = cutEdge[nearest * 2 + 1];

                cutEdge[nearest * 2 + 0] = n0;
                cutEdge[nearest * 2 + 1] = n1;
            }

        }
    }

    public static void LineMark( Transform tf, Vector3[] vertices)
    {
        LineRenderer line = new LineRenderer();
        GameObject line2 = new GameObject("line");
        //Debug.Log("===========uouououo222=========");
        //Debug.Log(line2);
        //Debug.Log("===========uouououo222=========");
        //Debug.Log("line2" + line2);
        line2.tag = "temp_cutplane_mark";
        //line2.transform.SetParent(tf);
        line = line2.AddComponent<LineRenderer>();
        //line.material = material;
        //line[1].material = new Material(Shader.Find("snapTurnArrow"));
        line2.GetComponent<LineRenderer>().useWorldSpace = false;
        //line2.transform.localScale.Set(1, 1, 1);//设置其大小，避免出现过大情况 就不会附着在父物体上
        line2.transform.localScale = new Vector3(1f, 1f, 1f);
        line.positionCount = vertices.Length + 1;
        line.startWidth = 0.0003f;
        line.endWidth = 0.0003f;
        for (int i = 0; i < vertices.Length; i++)
            line.SetPosition(i, tf.TransformPoint(vertices[i]));//设置渲染顶点
        line.SetPosition(vertices.Length, tf.InverseTransformPoint(vertices[0]));
        Debug.Log("创建line结束");
    }

    /// <summary>
    /// 判断四个点是否在一个平面上
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    /// <returns></returns>
    public static bool juge(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        bool tag = false;
        Plane plane = new Plane(a, b, c);
        Vector3 temp_point = plane.ClosestPointOnPlane(d);
        //d = new Vector3(d.x, d.y, d.z + 0.001f);
        if (d == temp_point) return true;
        else return tag;
    }
}
public struct Node
{
    public int vertices;
    public int index;
    public Node(int v,int ind)
    {
        vertices = v;
        index = ind;
    }
    public static bool operator >(Node lhs, Node rhs)
    {
        return lhs.vertices > rhs.vertices;
    }
    public static bool operator <(Node lhs, Node rhs)
    {
        return lhs.vertices < rhs.vertices;
    }
}
/// <summary>
/// 堆排序
/// </summary>
public class heap
{
    //堆排序算法（传递待排数组名，即：数组的地址。故形参数组的各种操作反应到实参数组上）
    public static void HeapSortFunction(ref Node[] array)
    {
        try
        {
            BuildMaxHeap(array);    //创建大顶推（初始状态看做：整体无序）
            for (int i = array.Length - 1; i > 0; i--)
            {
                Swap(ref array[0], ref array[i]); //将堆顶元素依次与无序区的最后一位交换（使堆顶元素进入有序区）
                MaxHeapify(array, 0, i); //重新将无序区调整为大顶堆
            }
        }
        catch (System.Exception ex)
        { }
    }

    ///<summary>
    /// 创建大顶推（根节点大于左右子节点）
    ///</summary>
    ///<param name="array">待排数组</param>
    private static void BuildMaxHeap(Node[] array)
    {
        try
        {
            //根据大顶堆的性质可知：数组的前半段的元素为根节点，其余元素都为叶节点
            for (int i = array.Length / 2 - 1; i >= 0; i--) //从最底层的最后一个根节点开始进行大顶推的调整
            {
                MaxHeapify(array, i, array.Length); //调整大顶堆
            }
        }
        catch (System.Exception ex)
        { }
    }

    ///<summary>
    /// 大顶推的调整过程
    ///</summary>
    ///<param name="array">待调整的数组</param>
    ///<param name="currentIndex">待调整元素在数组中的位置（即：根节点）</param>
    ///<param name="heapSize">堆中所有元素的个数</param>
    private static void MaxHeapify(Node[] array, int currentIndex, int heapSize)
    {
        try
        {
            int left = 2 * currentIndex + 1;    //左子节点在数组中的位置
            int right = 2 * currentIndex + 2;   //右子节点在数组中的位置
            int large = currentIndex;   //记录此根节点、左子节点、右子节点 三者中最大值的位置

            if (left < heapSize && array[left] > array[large])  //与左子节点进行比较
            {
                large = left;
            }
            if (right < heapSize && array[right] > array[large])    //与右子节点进行比较
            {
                large = right;
            }
            if (currentIndex != large)  //如果 currentIndex != large 则表明 large 发生变化（即：左右子节点中有大于根节点的情况）
            {
                Swap(ref array[currentIndex], ref array[large]);    //将左右节点中的大者与根节点进行交换（即：实现局部大顶堆）
                MaxHeapify(array, large, heapSize); //以上次调整动作的large位置（为此次调整的根节点位置），进行递归调整
            }
        }
        catch (System.Exception ex)
        { }
    }

    ///<summary>
    /// 交换函数
    ///</summary>
    ///<param name="a">元素a</param>
    ///<param name="b">元素b</param>W
    private static void Swap(ref Node a, ref Node b)
    {
        Node temp = new Node();
        temp = a;
        a = b;
        b = temp;
    }
}
