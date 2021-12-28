using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class test : MonoBehaviour
{
    public GameObject holder;
    public GameObject holder2;
    List<Vector3> ver = new List<Vector3>();
    List<Vector3> nor = new List<Vector3>();
    List<int> tri = new List<int>();
    List<int> outline = new List<int>();
    // Use this for initialization
    void Start()
    {
        Transform LHT = holder.transform;
        Transform LHT2 = holder.transform;
        Vector3 dir = LHT.TransformDirection(holder.GetComponent<MeshFilter>().mesh.vertices[2] - holder.GetComponent<MeshFilter>().mesh.vertices[3]);
        ver.Add(LHT.TransformPoint(holder.GetComponent<MeshFilter>().mesh.vertices[4]));//加上四个角点
        ver.Add(LHT.TransformPoint(holder.GetComponent<MeshFilter>().mesh.vertices[5]));
        ver.Add(LHT.TransformPoint(holder.GetComponent<MeshFilter>().mesh.vertices[7]));
        ver.Add(LHT.TransformPoint(holder.GetComponent<MeshFilter>().mesh.vertices[6]));



        //ver.Add(LHT2.TransformPoint(holder2.GetComponent<MeshFilter>().mesh.vertices[4]));//加上四个角点
        //ver.Add(LHT2.TransformPoint(holder2.GetComponent<MeshFilter>().mesh.vertices[5]));
        //ver.Add(LHT2.TransformPoint(holder2.GetComponent<MeshFilter>().mesh.vertices[7]));
        //ver.Add(LHT2.TransformPoint(holder2.GetComponent<MeshFilter>().mesh.vertices[6]));
        Vector3 up = LHT.TransformPoint(ver[0]) - LHT.TransformPoint(ver[3]);
        Vector3 left = LHT.TransformPoint(ver[0]) - LHT.TransformPoint(ver[1]);
        ver.Add(LHT2.TransformPoint(ver[0]) + up * 0.000001f + left * 0.000001f);
        ver.Add(LHT2.TransformPoint(ver[1]) + up * 0.000001f - left * 0.000001f);
        ver.Add(LHT2.TransformPoint(ver[2]) - up * 0.000001f - left * 0.000001f);
        ver.Add(LHT2.TransformPoint(ver[3]) - up * 0.000001f + left * 0.000001f);
        //ver.Reverse();//没有要求顺时针/逆时针顺序
        //for (int i = 0; i < 4; i++)
        //    Debug.Log("第" + i + "个点: " + LHT.TransformPoint(ver[i]));
        //for (int i = 4; i < ver.Count; i++)
        //    Debug.Log("第" + i + "个点: " + LHT2.TransformPoint(ver[i]));
        for (int i = 0; i < ver.Count; i++)
            Debug.Log("第" + i + "个点: " + ver[i]);
        nor.Add(dir);
        nor.Add(dir);
        nor.Add(dir);
        nor.Add(dir);
        nor.Add(dir);
        nor.Add(dir);
        nor.Add(dir);
        nor.Add(dir);
        //for (int i = 0; i < 4; i++)
        //{
        //    outline.Add(i);
        //    outline.Add((i + 1) % 4);
        //}
        ////outline.Add(3);
        ////outline.Add(4);
        //for (int i = 4; i < ver.Count; i++)
        //{
        //    outline.Add(i);
        //    outline.Add((i + 1) % ver.Count);
        //}
        //outline.Add(0);
        //outline.Add(1);
        //outline.Add(1);
        //outline.Add(2);
        //outline.Add(2);
        //outline.Add(3);
        //outline.Add(3);
        //outline.Add(0);

        //outline.Add(5);
        //outline.Add(4);
        //outline.Add(4);
        //outline.Add(7);
        //outline.Add(7);
        //outline.Add(6);
        //outline.Add(6);
        //outline.Add(5);
        List<int> new_tri = new List<int>();
        for (int i=0;i<4;i++)
        {
            if(i==3)
            {
                tri.Add(3);
                tri.Add(4);
                tri.Add(7);

                tri.Add(3);
                tri.Add(0);
                tri.Add(4);
                continue;
            }
            tri.Add(i);
            tri.Add(i + 1 + 4);
            tri.Add(i + 4);

            tri.Add(i);
            tri.Add(i + 1);
            tri.Add(i + 4 + 1);

            //tri.Add()
            //tri.Add()
            //tri.Add()
        }

        //int[] newEdges, newTriangles, newTriangleEdges;
        //ITriangulator triangulator = new Triangulator(ver.ToArray(), outline, dir);
        //triangulator.Fill(out newEdges, out newTriangles, out newTriangleEdges);
        //List<int> new_tri = new List<int>(newTriangles);
        //Debug.Log("三角数:" + (newTriangles.Length / 3));
        showMesh(ver.ToArray(), nor.ToArray(), tri.ToArray());
    }

    // Update is called once per frame
    void Update()
    {

    }
    void showMesh(Vector3[] ver, Vector3[] nor, int[] tri)
    {
        GameObject tempShow = new GameObject("tempShow");
        tempShow.AddComponent<MeshRenderer>();
        tempShow.AddComponent<MeshCollider>();
        tempShow.AddComponent<Choosable>();
        tempShow.AddComponent<Interactable>();
        Mesh mesh = tempShow.AddComponent<MeshFilter>().mesh;
        mesh.vertices = ver;
        mesh.normals = nor;
        mesh.triangles = tri;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for(int i=0;i<4;i++)
        {
            Gizmos.DrawSphere(ver[i], 0.0008f);
        }
        Gizmos.color = Color.blue;
        //for (int i = 4; i < 8; i++)
        //{
        //    Gizmos.DrawSphere(ver[i], 0.0008f);
        //}
        Gizmos.DrawSphere(ver[4], 0.0008f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(ver[5], 0.0008f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(ver[6], 0.0008f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(ver[7], 0.0008f);
    }
}
