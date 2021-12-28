using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
/// <summary>
/// 切割函数库
/// 2021.10.16
/// </summary>
public class cuttingTools : MonoBehaviour {
    /// <summary>
    /// 用平面切割的方式 移除两侧的缝合面
    /// </summary>
    /// <param name="beCutThing"></param>
    public static void removeCutHull(Mesh cutMesh)
    {
        return;
        Plane cutPlane;
        //一切
        Vector3 cutPoint = CutPosition.cutPoint[CutPosition.cutPoint.Count - 1];
        Vector3 cutHullnormal = CutPosition.cutPlaneNormal[CutPosition.cutPlaneNormal.Count - 1];
        cutPlane = new Plane(cutHullnormal, cutPoint);
        //WholeCut(beCutThing, cutPlane);
        //二切
        cutPoint = CutPosition.cutPoint[CutPosition.cutPoint.Count - 2];
        cutHullnormal = CutPosition.cutPlaneNormal[CutPosition.cutPlaneNormal.Count - 2];
        cutPlane = new Plane(cutHullnormal, cutPoint);
        //WholeCut(beCutThing, cutPlane);
    }
    /// <summary>
    /// 不缝合
    /// </summary>
    /// <param name="cutMesh"></param>
    /// <param name="transform"></param>
    /// <param name="cutPlane"></param>
    private void WholeCut(Mesh cutMesh, Transform transform, Plane cutPlane)
    {
        //==================
        int startTime = System.Environment.TickCount;

        //把切割面转化为被切割物体的当地坐标
        Vector3 localPoint = transform.InverseTransformPoint(cutPlane.normal * -cutPlane.distance);
        Debug.Log(localPoint);
        Vector3 localSplitPlaneNormal = transform.InverseTransformDirection(cutPlane.normal);
        localSplitPlaneNormal.Normalize();

        //根据切割面，把顶点分成两部分
        Vector3[] vertices = cutMesh.vertices;
        Debug.Log("被切割物体的顶点数：" + vertices.Length);
        Vector3[] normals = cutMesh.normals;

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

        int[] indices = cutMesh.triangles;
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
