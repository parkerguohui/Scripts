using ImgSpc.Exporters;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// 用于网格处理的工具类
/// by 郭辉 12.15
/// </summary>
public class AssistionMeshTools : MonoBehaviour
{
    private GameObject go;
    Mesh mesh;
    Vector3[] vertices;
    public Material material;
    public AssistionMeshTools(GameObject g,Material mt)//不可被修改 及无法将修改传递回去 避免蛇皮操作
    {
        if(g!=null)
        {
            go = g;
            mesh = go.GetComponent<MeshFilter>().mesh;
            vertices = mesh.vertices;

        }
        if(mt!=null)
            material = mt;
    }
    public void ReverseMesh(ref Mesh mesh)  
    {
        List<int> tri = new List<int>(mesh.triangles);
        tri.Reverse();
        mesh.triangles = tri.ToArray();
    }
    /// <summary>
    /// 三角面组边缘提取
    /// </summary>
    /// <param name="triangles"></param>
    /// <param name="edges"></param>
    /// <param name="invalidFlag"></param>
    /// <returns></returns>
    public List<Vector2Int> TrianglesEdgeAnalysis(int[] triangles)
    {
        int start = System.Environment.TickCount;
        int[,] edges = new int[triangles.Length, 2];
        for (int i = 0; i < triangles.Length; i += 3)
        {
            for (int j = 0; j < 3; j++)//??
            {
                for (int k = 0; k < 2; k++)
                {
                    int index = (j + k) % 3;
                    edges[i + j, k] = triangles[i + index];//存储边
                }
            }
        }
        bool[] invalidFlag = new bool[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            for (int j = i + 1; j < triangles.Length; j++)
            {
                if ((edges[i, 0] == edges[j, 0] && edges[i, 1] == edges[j, 1]) || (edges[i, 0] == edges[j, 1] && edges[i, 1] == edges[j, 0]))//同一条边
                {
                    invalidFlag[i] = true;//重复边标记
                    invalidFlag[j] = true;
                }
            }
        } 
        List<Vector2Int> edgeLines = new List<Vector2Int>();
        for (int i = 0; i < triangles.Length; i++)
        {
            if (!invalidFlag[i])
            {
                edgeLines.Add(new Vector2Int(edges[i, 0], edges[i, 1]));//edge保存索引
            }
        }
        if (edgeLines.Count == 0)
        {
            Debug.Log("Calculate wrong, there is not any valid line");
        }
        Debug.Log("耗时: " + (System.Environment.TickCount - start));
        return edgeLines;
    }
    /// <summary>
    /// 边缘排序与分离
    /// 可操作多个轮廓
    /// </summary>
    /// <param name="edgeLines">两个顶点的索引组成的一条边 [a_Index,b_Index]</param>
    /// <param name="vertices"></param>
    /// <returns></returns>
    public List<List<Vector3>> SpliteLines(List<Vector2Int> edgeLines, Vector3[] vertices)
    {
        List<List<Vector3>> result = new List<List<Vector3>>();
        List<int> edgeIndex = new List<int>();
        int startIndex = edgeLines[0].x;
        edgeIndex.Add(edgeLines[0].x);
        int removeIndex = 0;
        int currentIndex = edgeLines[0].y;

        while (true)
        {
            edgeLines.RemoveAt(removeIndex);
            edgeIndex.Add(currentIndex);

            bool findNew = false;
            for (int i = 0; i < edgeLines.Count && !findNew; i++)
            {
                if (currentIndex == edgeLines[i].x)
                {
                    currentIndex = edgeLines[i].y;//当前边下一个结点
                    removeIndex = i;
                    findNew = true;
                }
                else if (currentIndex == edgeLines[i].y)
                {
                    currentIndex = edgeLines[i].x;
                    removeIndex = i;
                    findNew = true;
                }
            }

            if (findNew && currentIndex == startIndex)
            {
                Debug.Log("Complete Closed curve");
                edgeLines.RemoveAt(removeIndex);
                List<Vector3> singleVertices = new List<Vector3>();
                for (int i = 0; i < edgeIndex.Count; i++)
                    singleVertices.Add(vertices[edgeIndex[i]]);
                result.Add(singleVertices);

                if (edgeLines.Count > 0)
                {
                    edgeIndex = new List<int>();
                    startIndex = edgeLines[0].x;
                    edgeIndex.Add(edgeLines[0].x);
                    removeIndex = 0;
                    currentIndex = edgeLines[0].y;
                }
                else
                {
                    break;
                }
            }
            else if (!findNew)
            {
                Debug.Log("Complete curve, but not closed");
                List<Vector3> singleVertices = new List<Vector3>();
                for (int i = 0; i < edgeIndex.Count; i++)
                    singleVertices.Add(vertices[edgeIndex[i]]);
                result.Add(singleVertices);

                if (edgeLines.Count > 0)
                {
                    edgeIndex = new List<int>();
                    startIndex = edgeLines[0].x;
                    edgeIndex.Add(edgeLines[0].x);
                    removeIndex = 0;
                    currentIndex = edgeLines[0].y;
                }
                else
                {
                    break;
                }
            }
        }

        return result;
    }
    /// <summary>
    /// 边缘排序与分离
    /// 可操作多个轮廓
    /// </summary>
    /// <param name="edgeLines">两个顶点的索引组成的一条边 [a_Index,b_Index]</param>
    /// <returns>返回索引数组</returns>
    public List<List<int>> SpliteLines_Multi(List<Vector2Int> edgeLines)
    {
        List<List<int>>  edgeIndexes = new List<List<int>>();//初始化 存储所有的环的索引
        List<int> edgeIndex = new List<int>();
        int startIndex = edgeLines[0].x;
        edgeIndex.Add(edgeLines[0].x);
        int removeIndex = 0;
        int currentIndex = edgeLines[0].y;

        while (true)
        {
            edgeLines.RemoveAt(removeIndex);
            edgeIndex.Add(currentIndex);

            bool findNew = false;
            for (int i = 0; i < edgeLines.Count && !findNew; i++)
            {
                if (currentIndex == edgeLines[i].x)
                {
                    currentIndex = edgeLines[i].y;//当前边下一个结点
                    removeIndex = i;
                    findNew = true;
                }
                else if (currentIndex == edgeLines[i].y)
                {
                    currentIndex = edgeLines[i].x;
                    removeIndex = i;
                    findNew = true;
                }
            }

            if (findNew && currentIndex == startIndex)//形成闭环
            {
                Debug.Log("Complete Closed curve");
                edgeLines.RemoveAt(removeIndex);
                edgeIndexes.Add(edgeIndex);//将环的索引加入
                if (edgeLines.Count > 0)
                {
                    edgeIndex = new List<int>();
                    startIndex = edgeLines[0].x;
                    edgeIndex.Add(edgeLines[0].x);
                    removeIndex = 0;
                    currentIndex = edgeLines[0].y;
                }
                else
                {
                    break;
                }
            }
            else if (!findNew)
            {
                Debug.Log("Complete curve, but not closed");
                
                

                if (edgeLines.Count > 0)
                {
                    edgeIndex = new List<int>();
                    startIndex = edgeLines[0].x;
                    edgeIndex.Add(edgeLines[0].x);
                    removeIndex = 0;
                    currentIndex = edgeLines[0].y;
                }
                else
                {
                    break;
                }
            }
        }

        return edgeIndexes;
    }
    /// <summary>
    /// 边缘排序与分离
    /// 仅操作一个轮廓
    /// </summary>
    /// <param name="edgeLines">两个顶点的索引组成的一条边 [a_Index,b_Index]</param>
    /// <param name="vertices"></param>
    /// <returns></returns>
    public List<List<Vector3>> SpliteLines_single(List<Vector2Int> edgeLines, Vector3[] vertices,out List<int> edgeIndex)
    {
        List<List<Vector3>> result = new List<List<Vector3>>();

        //List<int> edgeIndex = new List<int>();
        edgeIndex = new List<int>();
        int startIndex = edgeLines[0].x;
        edgeIndex.Add(edgeLines[0].x);
        int removeIndex = 0;
        int currentIndex = edgeLines[0].y;

        while (true)
        {
            edgeLines.RemoveAt(removeIndex);
            edgeIndex.Add(currentIndex);

            bool findNew = false;//查找下一条边的判断标志
            for (int i = 0; i < edgeLines.Count && !findNew; i++)//大循环
            {
                if (currentIndex == edgeLines[i].x)
                {
                    currentIndex = edgeLines[i].y;//当前边下一个结点
                    removeIndex = i;
                    findNew = true;
                }

                else if (currentIndex == edgeLines[i].y)
                {
                    currentIndex = edgeLines[i].x;
                    removeIndex = i;
                    findNew = true;
                }
            }

            if (findNew && currentIndex == startIndex)//找到环的起始位置
            {
                Debug.Log("Complete Closed curve");
                edgeLines.RemoveAt(removeIndex);
                List<Vector3> singleVertices = new List<Vector3>();
                for (int i = 0; i < edgeIndex.Count; i++)
                    singleVertices.Add(vertices[edgeIndex[i]]);
                //调用渲染
                //LineMark(edgeIndex, go.transform);

                result.Add(singleVertices);

                //跳出，后面的数据不再需要
                break;
                if (edgeLines.Count > 0)
                {
                    edgeIndex = new List<int>();
                    startIndex = edgeLines[0].x;
                    edgeIndex.Add(edgeLines[0].x);
                    removeIndex = 0;
                    currentIndex = edgeLines[0].y;
                }
                else
                {
                    break;
                }
            }
            else if (!findNew)
            {
                Debug.Log("Complete curve, but not closed");
                List<Vector3> singleVertices = new List<Vector3>();
                for (int i = 0; i < edgeIndex.Count; i++)
                    singleVertices.Add(vertices[edgeIndex[i]]);
                result.Add(singleVertices);
                //跳出，后面的数据不再需要
                break;
                if (edgeLines.Count > 0)
                {
                    edgeIndex = new List<int>();
                    startIndex = edgeLines[0].x;
                    edgeIndex.Add(edgeLines[0].x);
                    removeIndex = 0;
                    currentIndex = edgeLines[0].y;
                }
                else
                {
                    break;
                }
            }
        }

        return result;
    }  
    /// <summary>
    /// 用于标记
    /// </summary>
    /// <param name="points"></param>
    /// <param name="transform"></param>
    public void LineMark(List<int> points_Index, Transform transform)
    {
        LineRenderer line = new LineRenderer();
        GameObject line2 = new GameObject("line");
        Debug.Log("===========uouououo222=========");
        Debug.Log(line2);
        Debug.Log("===========uouououo222=========");
        Debug.Log("line2" + line2);
        line2.tag = "temp_cutplane_mark";
        line2.transform.SetParent(transform);
        line = line2.AddComponent<LineRenderer>();
        line.material = material;
        //line[1].material = new Material(Shader.Find("snapTurnArrow"));
        line2.GetComponent<LineRenderer>().useWorldSpace = false;
        //line2.transform.localScale.Set(1, 1, 1);//设置其大小，避免出现过大情况 就不会附着在父物体上
        line2.transform.localScale = new Vector3(1f, 1f, 1f);
        line.positionCount = points_Index.Count + 1;
        line.startWidth = 0.0003f;
        line.endWidth = 0.0003f;
        for (int i = 0; i < points_Index.Count; i++)
            line.SetPosition(i, go.transform.TransformPoint(vertices[points_Index[i]]));//设置渲染顶点
        line.SetPosition(points_Index.Count, go.transform.InverseTransformPoint(vertices[points_Index[0]]));

    }

    public void CopyMesh(Mesh mesh,Transform tf,out List<Vector3> vers,out List<int> tris,out List<Vector3> nors)
    {
        vers = new List<Vector3>(mesh.vertices.Length);
        Vector3 nor = (mesh.normals[0] + mesh.normals[mesh.normals.Length / 2] + mesh.normals[mesh.normals.Length - 1]) / 3;//平均值  
        nor.Normalize();
        float thickness = 0.006f;
        for(int i=0;i<mesh.vertices.Length;++i)
        {
            vers.Add(mesh.vertices[i] + thickness * nor);//局部
        }
        tris = new List<int>(mesh.triangles);
        tris.Reverse();//翻转三角面
        nors = new List<Vector3>(mesh.normals.Length);
        for (int i = 0; i < mesh.normals.Length; ++i)
            nors.Add(mesh.normals[i]);
        //showMesh(vers, nors, tris,"copyMesh",tf);
    }

    /// <summary>
    /// 显示网格
    /// </summary>
    /// <param name="ver"></param>
    /// <param name="nor"></param>
    /// <param name="tri"></param>
    public void showMesh(List<Vector3> ver, List<Vector3> nor, List<int> tri)
    {
        string name = "tempShow";
        showMesh(ver, nor, tri, name,transform);
    }
    public void showMesh(List<Vector3> ver, List<Vector3> nor, List<int> tri, string name,Transform tf)
    {
        GameObject tempShow = new GameObject(name);
        tempShow.AddComponent<MeshRenderer>();
        tempShow.AddComponent<MeshCollider>();
        tempShow.AddComponent<Choosable>();
        tempShow.AddComponent<Interactable>();
        Mesh mesh = tempShow.AddComponent<MeshFilter>().mesh;
        mesh.vertices = ver.ToArray();
        mesh.normals = nor.ToArray();
        mesh.triangles = tri.ToArray();
        tempShow.transform.position = tf.position;
        tempShow.transform.rotation = tf.rotation;
    }
    /// <summary>
    /// 将三维模型导出为点云数据 Ply格式 
    /// </summary>
    /// <param name="vers">点数组</param>
    /// <param name="nors">法向量数组</param>
    public void WriteFile(List<Vector3> vers, List<Vector3> nors)
    {
        string path = @"D:\\res.ply";
        //创建StreamWriter 类的实例
        StreamWriter streamWriter = new StreamWriter(path);
        streamWriter.WriteLine("ply");
        streamWriter.WriteLine("format ascii 1.0");
        streamWriter.WriteLine("comment VCGLIB generated");
        streamWriter.WriteLine("element vertex " + vers.Count);
        streamWriter.WriteLine("property float x");
        streamWriter.WriteLine("property float y");
        streamWriter.WriteLine("property float z");
        streamWriter.WriteLine("property float nx");
        streamWriter.WriteLine("property float ny");
        streamWriter.WriteLine("property float nz");
        streamWriter.WriteLine("property list uchar int vertex_indices");
        streamWriter.WriteLine("end_header");
        //向文件中写入数据
        //Debug.Log("1");
        //streamWriter.WriteLine("物体1的坐标点");
        for (int i = 0; i < vers.Count; i++)
        {
            streamWriter.Write(vers[i].x + " " + vers[i].y + " " + vers[i].z + " ");
            streamWriter.WriteLine(nors[i].x + " " + nors[i].y + " " + nors[i].z);
        }
        streamWriter.Flush();
        streamWriter.Close();


    }

    /// <summary>
    /// 将复制之后的面与之前的面进行缝合
    /// </summary>
    /// <param name="mesh">待修改的物体Mesh网格</param>
    /// <param name="ver1"></param>
    /// <param name="nor1"></param>
    /// <param name="tri1"></param>
    /// <param name="ver2"></param>
    /// <param name="nor2"></param>
    /// <param name="tri2"></param>
    /// <param name="edgeIndex">轮廓索引</param>
    public void Suture_TwoFaces(ref Mesh mesh, List<Vector3> ver1, List<Vector3> nor1, List<int> tri1, List<Vector3> ver2, List<Vector3> nor2, List<int> tri2, List<int> edgeIndex)
    {
        //if(edgeIndex==null)
        //{
        //    edgeIndex = new List<int>(ver1.Count);
        //    for (int i = 0; i < ver1.Count; ++i)
        //        edgeIndex.Add(i);
        //}
        //直接从tri1上进行修改 不再添加新变量
        for (int i = 0; i < edgeIndex.Count - 1; ++i)//缝合
        {
            tri1.Add(edgeIndex[i] + ver1.Count);
            tri1.Add(edgeIndex[i + 1]);
            tri1.Add(edgeIndex[i]);

            tri1.Add(edgeIndex[i] + ver1.Count);
            tri1.Add(edgeIndex[i + 1] + ver1.Count);
            tri1.Add(edgeIndex[i + 1]);
        }
        //最后两个三角形
       {
            tri1.Add(edgeIndex[edgeIndex.Count - 1] + ver1.Count);
            tri1.Add(edgeIndex[0]);
            tri1.Add(edgeIndex[edgeIndex.Count - 1]);

            tri1.Add(edgeIndex[0]);
            tri1.Add(edgeIndex[0] + ver1.Count);
            tri1.Add(edgeIndex[edgeIndex.Count - 1] + ver1.Count);
       }

        for (int i = 0; i < tri2.Count; i++)//合并
        {
            tri1.Add(ver1.Count + tri2[i]);
            //tri.Add(ver.Count + tris[i + 1]);
            //tri.Add(ver.Count + tris[i + 2]);
        }
        ver1.AddRange(ver2);
        nor1.AddRange(nor2);

        //赋值给mesh
        mesh.vertices = ver1.ToArray();
        mesh.triangles = tri1.ToArray();
        mesh.normals = nor1.ToArray();
    }

    /// <summary>
    /// STL文件导出
    /// </summary>
    public void StlExporter(GameObject ObjectToBeEXP)
    {
        ImgSpcExporter imgSpcExporter = new ImgSpcExporter();
        //string path = Application.dataPath + "/ExportFile/" + "第" + Export_num + "个导出的文件.stl";
        System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
        string path="";
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
        //Debug.Log("path: " + path);
        path = path + "/" + ObjectToBeEXP +".stl";
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
}
