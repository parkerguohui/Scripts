using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// 用于将Unity中的GameObject输出为.obj文件进行保存
/// </summary>
public class OBJWrite : MonoBehaviour {
    public static void SaveMesh(GameObject beSaveThing)
    {
        Mesh mesh = beSaveThing.GetComponent<MeshFilter>().mesh;
        string name = beSaveThing.name;
        CreateFile(mesh.vertices, mesh.triangles, name);
    }
    public static void CreateFile(Vector3[] Vertices, int[] Triangles,string name)
    {
        string path = @"D:\\BeSavedThing.obj";
        //创建StreamWriter 类的实例
        StreamWriter streamWriter = new StreamWriter(path);
        //向文件中写入数据
        Debug.Log("开始");
        streamWriter.WriteLine("#"+name);
        for (int i=0;i<Vertices.Length;i++)
        {
            streamWriter.WriteLine("v "+Vertices[i].x + " " + Vertices[i].y + " " + Vertices[i].z);
        }
        for(int i = 0; i < Triangles.Length ; i+=3)
        {
            streamWriter.WriteLine("f " + Triangles[i] + " " + Triangles[i + 1] + " " + Triangles[i + 2]);
        }
    }
}
