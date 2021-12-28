using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;
using System;
public class cut_plane_tool_test : MonoBehaviour
{
    private GameObject beCutThing;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))//读取键盘 键入C时 执行
        //if (Input.GetMouseButtonDown(1) || SteamVR_Actions.default_Teleport.GetStateDown(SteamVR_Input_Sources.Any))
        {

            if (gameObject.tag == "ChosenTool")
            {
                beCutThing = GameObject.FindGameObjectWithTag("ChosenObject");
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
    }

    private void Cut()
    {
        //==================
        int startTime = System.Environment.TickCount;
        //根据本切割工具网格顶点形成切割面
        Vector3[] v = transform.GetComponent<MeshFilter>().mesh.vertices;
        Plane cutPlane = new Plane(transform.TransformPoint(v[0]), transform.TransformPoint(v[1]), transform.TransformPoint(v[v.Length - 1]));
        Debug.Log(cutPlane.normal);

        //把切割面转化为被切割物体的当地坐标
        Vector3 localPoint = beCutThing.transform.InverseTransformPoint(cutPlane.normal * -cutPlane.distance);
        //Debug.Log(localPoint);
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

        GameObject part1 = Instantiate(beCutThing, null);//(T)用于复制物体？  省去增加组件步骤  
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
        Isolate.IsolateMesh(part1);
        Isolate.IsolateMesh(part2);

        part1.tag = "Objects";
        part2.tag = "Objects";

        int overTime = System.Environment.TickCount;
        Debug.Log("切割总耗时：" + (overTime - startTime) + "      三角面分割耗时：" + (cutOverTime - startTime) + "            缝补耗时：" + (trianglationOverTime - cutOverTime) + "           分离耗时：" + (overTime - trianglationOverTime));

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

}
