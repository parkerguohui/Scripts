using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows.Forms;
using System.IO;
using System;

/// <summary>
/// 用于显示打开文件夹窗口 并选择文件的类
/// </summary>
public class OBJLoader : MonoBehaviour
{

    string defaultPath = "";


    void Start()
    {
        //defaultPath = UnityEngine.Application.dataPath + "/GameAssets/";
        //defaultPath = "D:" + "/GameAssets/";
    }
    /// <summary>
    /// 选择文件夹
    /// </summary>
    private void SelectFolder()
    {

        DirectoryInfo mydir = new DirectoryInfo(defaultPath);
        if (!mydir.Exists)
        {
            MessageBox.Show("请先创建资源文件夹");
            return;
        }

        try
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择要打包的资源文件夹";
            fbd.ShowNewFolderButton = false;
            fbd.RootFolder = Environment.SpecialFolder.MyComputer;//设置默认打开路径
            fbd.SelectedPath = defaultPath;  //默认打开路径下的详细路径

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                defaultPath = fbd.SelectedPath;
                //selectDir.text = fbd.SelectedPath;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("打开错误：" + e.Message);
            return;
        }
    }
    /// <summary>
    /// 选择文件
    /// </summary>
    public void SelectFile()
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "obj文件(*.obj)|*.obj";
        ofd.Title = "选择待手术的对象";
        ofd.InitialDirectory = "file://" + UnityEngine.Application.dataPath;//默认打开路径 
        
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            Debug.Log(ofd.FileName);
            // 读出文本文件的所有行
            string[] lines = File.ReadAllLines(ofd.FileName);
            GameObject newone = OBJCreate(lines);
            ///<summary>需要先修改标签 再调用组件</summary>
            newone.tag = "LoadedObject";
            newone.AddComponent<MeshCollider>();
            MeshRenderer meshRenderer= newone.AddComponent<MeshRenderer>();
            ///<summary>需要控制导入物体的大小
            ///增加该组件即可
            ///</summary>
            //CheckLoadedObject check = new CheckLoadedObject();
            ///<summary>添加组件后即可调整大小 调整结束后需要修改标签</summary>
            newone.AddComponent<CheckLoadedObject>();//用于控制导入物体的大小的组件
            //newone.tag = "Objects";
            //meshRenderer.material = 
            //newone.GetComponent<Transform>().transform = new Transform(0, 0, 0);
        }
        //Debug.Log("已经打开");
    }
    /// <summary>
    /// 根据传入文件生成对应gameobject
    /// </summary>
    /// <returns></returns>
    static GameObject OBJCreate(string[] lines)
    {
        int start = System.Environment.TickCount;

        GameObject o = new GameObject();
        //var meshCollider = o.AddComponent<MeshCollider>();
        //var meshFilter = o.AddComponent<MeshFilter>();
        
        o.AddComponent<MeshFilter>();
        o.GetComponent<MeshFilter>().mesh.Clear();
        var mesh = new Mesh();

        List<Vector3> vertexList = new List<Vector3>();
        List<int> TrianglesList = new List<int>();
        //foreach (string line in lines)
        //{
        //    if (line[0] == 'v')
        //        break;
        //}
        for (int i = 0; i < lines.Length; i++)
        {
            var currentLine = lines[i];

            if (currentLine[0]=='#' || currentLine.Length == 0)
            {
                Debug.Log("#");
                continue;
            }

            if (currentLine[0] == 'v')
            {
                Debug.Log("V");
                var splitInfo = currentLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                vertexList.Add(new Vector3() { x = float.Parse(splitInfo[1]), y = float.Parse(splitInfo[2]), z = float.Parse(splitInfo[3]) });
            }
            //else if (currentLine.Contains("vt "))
            //{
            //    var splitInfo = currentLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //    vertexTextureList.Add(new Vector3() { x = splitInfo.Length > 1 ? float.Parse(splitInfo[1]) : 0, y = splitInfo.Length > 2 ? float.Parse(splitInfo[2]) : 0, z = splitInfo.Length > 3 ? float.Parse(splitInfo[3]) : 0 });
            //}
            //else if (currentLine.Contains("vn "))
            //{
            //    var splitInfo = currentLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //    vertexNormalList.Add(new Vector3() { x = float.Parse(splitInfo[1]), y = float.Parse(splitInfo[2]), z = float.Parse(splitInfo[3]) });
            //}
            else if (currentLine[0] == 'f')
            {
                Debug.Log("f");
                int star_split = System.Environment.TickCount;
                var splitInfo = currentLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                Debug.Log("单个Split花费的时间："+(System.Environment.TickCount - star_split));

                int[] triangle = new int[] { int.Parse(splitInfo[1]) - 1, int.Parse(splitInfo[2]) - 1, int.Parse(splitInfo[3]) - 1 };
                int n = vertexList.Count;

                //if (triangle[0] > n - 1 || triangle[1] > n - 1 || triangle[2] > n - 1)
                //{
                //    Debug.Log("越界");
                //    Debug.Log(triangle[0]+ " " + triangle[1] + " " + triangle[2]);
                //    continue;
                //}
                int start_Tria = System.Environment.TickCount;
                TrianglesList.Add(triangle[0]);
                TrianglesList.Add(triangle[1]);
                TrianglesList.Add(triangle[2]);
                Debug.Log("单次增加三角耗时:" + (System.Environment.TickCount - start_Tria));
            }
        }
        Debug.Log("vertexList.Count:" + vertexList.Count);
        Debug.Log("TrianglesList.Count:" + TrianglesList.Count);
        Debug.Log("赋值");
        Debug.Log("1");
        mesh.vertices = vertexList.ToArray();
        Debug.Log("2");

        mesh.triangles = TrianglesList.ToArray();
        Debug.Log("3");
        o.GetComponent<MeshFilter>().mesh = mesh;
        //Debug.Log("4");
        //o.GetComponent<MeshFilter>().mesh.triangles = TrianglesList.ToArray();
        //Debug.Log("5");
        //o.GetComponent<MeshFilter>().mesh.vertices = vertexList.ToArray();
        int start_Collider = System.Environment.TickCount;
        o.AddComponent<MeshCollider>();
        Debug.Log("collider耗时:" + (System.Environment.TickCount - start_Collider));
        Debug.Log("创建完毕");
        Debug.Log("花费时间" + (System.Environment.TickCount - start));
        return o;
    }

}
