using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows.Forms;
using System.IO;
using System;
using Valve.VR.InteractionSystem;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
///<summary>要给物体添加插件里的脚本组件 需要引用该插件的命名空间</summary>
/// <summary>
/// 自动添加需要的脚本到物体上
/// </summary>
/// <summary>这个类暂时搁置 有更好的解决办法了
/// </summary>
[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(Choosable))]
public class Scanf
{
    Hashtable typePatterns;
    public Scanf()//构造函数
    {
        //仅需匹配int型和single型的数据
        typePatterns = new Hashtable();
        typePatterns.Add("Int32", @"-[0-9]+|[0-9]+");
        typePatterns.Add("Double",
         @"-?([0-9]{1,4}[.]?[0-9]{0,5})");//Single=float 单精度浮点型
    }
    private object ReturnValue(string typeName, string sValue)
    {
        object o = null;
        switch (typeName)
        {
            case "Int32":
                o = UInt32.Parse(sValue);
                break;
                
            case "Double":
                o = Double.Parse(sValue);
                break;
        }
        return o;
    }
    private string ReturnPattern(string typeName)
    {
        string innerPattern = "";
        switch (typeName)
        {
            case "Int32":
                innerPattern = (String)typePatterns["Int32"];
                break;

            case "Double":
                innerPattern = (String)typePatterns["Double"];
                break;
        }
        return innerPattern;
    }
    /// <summary>
    /// 用于扫描String 根据对应的正则表达式 返回扫描到的值
    /// </summary>
    public MatchCollection Scan(string OBJtext,int tag)
    {
        //object[] targets = null;
        MatchCollection matches;
        try
        {
            if(tag==1)
            {
                matches = Regex.Matches(OBJtext, (string)typePatterns["Double"]);
                //Debug.Log("共扫描到："+matches.Count);
                //for (int i = 0; i < matches.Count; i++)
                //    Debug.Log(matches[i]);
                //if (matches.Count > 3)
                //    Debug.Log(matches.Count);

                //for(int i=0;i<3;i++)
                //{

                //}
            }
            
            else
            {
                matches = Regex.Matches(OBJtext, (string)typePatterns["Int32"]);
                //Debug.Log("共扫描到:" + matches.Count);
                //for(int i=0;i<3;i++)
                //{

                //}
                //for (int i = 0; i < matches.Count; i++)
                //    Debug.Log(matches[i]);
                //if (matches.Count > 3)
                //    Debug.Log(matches.Count);
            }
                
        }
        catch (Exception ex)
        {
            throw new ScanExeption("Scan exception", ex);
        }
        return matches;
    }


}
class ScanExeption : Exception
{
    public ScanExeption() : base()
    {
    }

    public ScanExeption(string message) : base(message)
    {
    }

    public ScanExeption(string message, Exception inner) : base(message, inner)
    {
    }

    public ScanExeption(SerializationInfo info,
      StreamingContext context) : base(info, context)
    {
    }
}

//[AddComponentMenu("")]
/// <summary>
/// 用于显示打开文件夹窗口 并选择文件的类
/// 使用C++中的fscanf()来代替split()函数
/// 并改进读取的方式 一起改善读取文件的效率问题
/// </summary>
public class OBJLoader_optimized : MonoBehaviour
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
            int start_ReadAllLines = System.Environment.TickCount;
            string[] lines = File.ReadAllLines(ofd.FileName);
            Debug.Log("读取文件耗时:" + (System.Environment.TickCount-start_ReadAllLines));
            //string[] lines = File.ReadAllLines(ofd.InitialDirectory);//现在的InitialDirectory不适合使用
            GameObject o = OBJCreate(lines);
            o.GetComponent<Transform>().position =new Vector3(0.5f,-0.5f,-0.5f);
            if (o.AddComponent<Interactable>() != null)
                Debug.Log("Interactable has loaded");
            if (o.AddComponent<Choosable>() != null)
                Debug.Log("Choosable has loaded");
            //o.AddComponent<Interactable>();
            //o.AddComponent<Choosable>();
            //newone.GetComponent<Transform>().transform = new Transform(0, 0, 0);
        }
        //Debug.Log("已经打开");
    }
    /// <summary>
    /// 根据传入文件生成对应gameobject
    /// 注意某些OBJ文件中节点坐标是以double存放
    /// </summary>
    static GameObject OBJCreate(string[] lines)
    {
        Debug.Log("OBJCreate begin~");
        GameObject o = new GameObject();
        //var meshCollider = o.AddComponent<MeshCollider>();
        //var meshFilter = o.AddComponent<MeshFilter>();
        o.AddComponent<MeshCollider>();
        o.AddComponent<MeshFilter>();
        o.GetComponent<MeshFilter>().mesh.Clear();
        o.AddComponent<MeshRenderer>();
        
        List<Vector3> vertexList = new List<Vector3>();
        List<int> TrianglesList = new List<int>();
        Scanf s = new Scanf();
        int start_Regex = System.Environment.TickCount;
        for (int i = 0; i < lines.Length; i++)
        {
            var currentLine = lines[i];
            MatchCollection matches;
            //if (currentLine.Contains("#") || currentLine.Length == 0)
            if (currentLine[0]=='#' || currentLine.Length == 0)
            {
                Debug.Log("#");
                continue;
            }

            if (currentLine[0] == 'v')
            {
                Debug.Log("V");
                matches = s.Scan(currentLine, 1);
                vertexList.Add(new Vector3((float)Double.Parse(matches[0].Value), (float)Double.Parse(matches[1].Value), (float)Double.Parse(matches[2].Value)));
                Debug.Log(vertexList[vertexList.Count - 1].x +" "+ vertexList[vertexList.Count - 1].y + " " + vertexList[vertexList.Count - 1].z);
                //vertexList.Add(new Vector3() { x = float.Parse(splitInfo[1]), y = float.Parse(splitInfo[2]), z = float.Parse(splitInfo[3]) });
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
                matches = s.Scan(currentLine, 0);
                for (int j = 0; j < 3; j++)
                {
                    int temp = (Int32.Parse(matches[j].Value) - 1);
                    Debug.Log("三角索引:"+temp);
                    if (temp > vertexList.Count)
                        Debug.Log("越界节点:" + temp);
                    TrianglesList.Add(temp);
                }
                
                
            }
        }
        Debug.Log("匹配耗时:" + (System.Environment.TickCount - start_Regex));
        Debug.Log("vertexList.Count:" + vertexList.Count);
        Debug.Log("TrianglesList.Count:" + TrianglesList.Count);
        Debug.Log("赋值");
        Debug.Log("1");
        //mesh.vertices = vertexList.ToArray();
        Debug.Log("2");
        //mesh.triangles = TrianglesList.ToArray();
        Debug.Log("3");
        //o.GetComponent<MeshFilter>().mesh = mesh;
        Debug.Log("4");
        o.GetComponent<MeshFilter>().mesh.vertices = vertexList.ToArray();
        Debug.Log("5");
        o.GetComponent<MeshFilter>().mesh.triangles = TrianglesList.ToArray();
        Debug.Log("正则匹配耗时:" + (System.Environment.TickCount - start_Regex));
        Debug.Log("创建完毕");
        return o;
    }

}

