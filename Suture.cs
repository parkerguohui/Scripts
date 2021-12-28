using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>简单切面缝合
///</summary>
public class Quick
{
    //快速排序（目标数组，数组的起始位置，数组的终止位置）
    public static void QuickSort( List<int> VerticesIntersection,  float[] lenth, int low, int high)
    {
        try
        {
            int keyValuePosition;   //记录关键值的下标

            
            if (low < high)
            {
                keyValuePosition = keyValuePositionFunction(  VerticesIntersection, lenth, low, high);  

                QuickSort( VerticesIntersection,  lenth, low, keyValuePosition - 1);    
                QuickSort( VerticesIntersection,  lenth, keyValuePosition + 1, high);   
            }
        }
        catch (Exception ex)
        { }
    }

   
    private static int keyValuePositionFunction( List<int> VerticesIntersection, float[] lenth, int low, int high)
    {
        int leftIndex = low;        
        int rightIndex = high;     

        float keyValue = lenth[low]; 
        float temp;
        int T ;

     
        while (leftIndex < rightIndex)
        {
            while (leftIndex < rightIndex && lenth[leftIndex] <= keyValue)  
            {
                leftIndex++;
            }
            while (leftIndex < rightIndex && lenth[rightIndex] > keyValue)  
            {
                rightIndex--;
            }
            if (leftIndex < rightIndex)  
            {
                //VLenth互换
                temp = lenth[leftIndex];
                lenth[leftIndex] = lenth[rightIndex];
                lenth[rightIndex] = temp;
                //VerticesIntersection互换
                T = VerticesIntersection[leftIndex];
                VerticesIntersection[leftIndex] = VerticesIntersection[rightIndex];
                VerticesIntersection[rightIndex] = T;
            }
        }

        //当左右两个动态下标相等时（即：左右下标指向同一个位置），此时便可以确定keyValue的准确位置
        temp = keyValue;
        T = VerticesIntersection[low];
        if (temp < lenth[rightIndex])   //当keyValue < 左右下标同时指向的值，将keyValue与rightIndex - 1指向的值交换，并返回rightIndex - 1
        {
            lenth[low] = lenth[rightIndex - 1];
            lenth[rightIndex - 1] = temp;
            //VerticesIntersection互换
            VerticesIntersection[low] = VerticesIntersection[rightIndex - 1];
            VerticesIntersection[rightIndex - 1] = T;
            return rightIndex - 1;
        }
        else //当keyValue >= 左右下标同时指向的值，将keyValue与rightIndex指向的值交换，并返回rightIndex
        {
            lenth[low] = lenth[rightIndex];
            lenth[rightIndex] = temp;
            //VerticesIntersection互换
            VerticesIntersection[low] = VerticesIntersection[rightIndex];
            VerticesIntersection[rightIndex] = T;
            return rightIndex;
        }
    }

}
public  class Suture : MonoBehaviour {
    ///<value>hh</value>
    ///
    //private GameObject O;
    public static void CutHull_Suture(ref GameObject O,List<int> VerticesIntersection, List<Vector3> Vertices)
    {
        Debug.Log(VerticesIntersection.Count);
        for (int i = 0; i < VerticesIntersection.Count; i++)
        {
            Debug.Log("Vertices[VerticesIntersection[i]]:" + Vertices[VerticesIntersection[i]]);
        }
        //Debug.Log("Vertices[VerticesIntersection[2]]:" + Vertices[VerticesIntersection[2]]);
        //Debug.Log("Vertices[VerticesIntersection[3]]:" + Vertices[VerticesIntersection[3]]);
        //顶点排序
        Debug.Log("VerticesIntersection:" + VerticesIntersection.Count);
        List<Vector3> SortVectors = new List<Vector3>(900);
        List<float> VLenth = new List<float>(VerticesIntersection.Count);//向量长度
        //获取初始值0 无用位置赋值0
        SortVectors.Add(new Vector3(0, 0, 0));
        VLenth.Add(0);
        VLenth.Add(0);
        //SortVertices[0] = new Vector3(0, 0, 0);
        for (int i = 1; i < VerticesIntersection.Count; i++)
        {
            if (i >= 2)
            {
                SortVectors.Add(Vertices[VerticesIntersection[i]] - Vertices[VerticesIntersection[0]]);
                VLenth.Add((SortVectors[i] + SortVectors[1]).sqrMagnitude);//向量长度
            }
            else
            {
                //Debug.Log("第1次");
                //Debug.Log("VerticesIntersection[1]:" + VerticesIntersection[1]);
                //Debug.Log("VerticesIntersection[0]:" + VerticesIntersection[0]);
                //Debug.Log("Vertices:" + Vertices.Count);
                //Debug.Log(Vertices[Vertices.Count - 1]);
                //Debug.Log(Vertices[VerticesIntersection[1]]);
                //Debug.Log(Vertices[VerticesIntersection[1]] - Vertices[VerticesIntersection[0]]);
                //Debug.Log(SortVectors.Count);

                SortVectors.Add(Vertices[VerticesIntersection[1]] - Vertices[VerticesIntersection[0]]);//获得向量
                //Debug.Log("成功");
            }


            //Debug.Log(i + "次完成");
        }

        SortVectors[VerticesIntersection.Count - 1] += SortVectors[1];
        float[] lenth = VLenth.ToArray();
        //for (int i = 10; i < 60; i++)
        //    Debug.Log("Vertices[VerticesIntersection[i]]:" + Vertices[VerticesIntersection[i]]);
        Debug.Log("快排开始");
        //int start = System.Environment.TickCount;
        Debug.Log("VerticesIntersection.Count - 1:" + (VerticesIntersection.Count - 1));
        Quick.QuickSort( VerticesIntersection,  lenth, 2, VerticesIntersection.Count - 1);
        Debug.Log("快排结束");
        //Debug.Log("快排耗时:"+(System.Environment.TickCount - start).ToString());
        Mesh mesh = O.GetComponent<MeshFilter>().mesh;
        
        Debug.Log("增加前mesh.triangles.Length:" + mesh.triangles.Length);
        //Vector3[] vertices = mesh.vertices;

        List<int> Triangles = new List<int>(mesh.triangles);
        //List<int> Triangles = new List<int>(Vertices.Count+VerticesIntersection.Count);
        //for(int i=0;i<mesh.triangles.Length;i++)
        //{
        //    Triangles.Add(mesh.triangles[i]);
        //}
        Debug.Log("前Triangles.Count:" + Triangles.Count);
        //Debug.Log("前Triangles.Count:" + Triangles[1]);

        //Vector3[] normals = mesh.normals;
        //缝合切面
        for (int i = 1; i < VerticesIntersection.Count - 1; i++)
        {
            if (Vertices[VerticesIntersection[i]] == Vertices[VerticesIntersection[i] - 1])
                continue;//相同节点跳过
            Triangles.Add(VerticesIntersection[0]);
            Triangles.Add(VerticesIntersection[i]);
            Triangles.Add(VerticesIntersection[i + 1]);
            Debug.Log("ok");
        }
        Debug.Log("后Triangles.Count:" + Triangles.Count);
        mesh.triangles = Triangles.ToArray();
        //O.GetComponent<MeshFilter>().mesh.Clear();//清空原有mesh
        O.GetComponent<MeshFilter>().mesh = mesh;
        Debug.Log("增加后mesh.triangles.Length:"+mesh.triangles.Length);
        //int n = O.GetComponent<MeshFilter>().mesh.triangles.Length;
        //for (int i = 0; i < Triangles.Count; i += 3)//添加切面三角
        //{
        //    O.GetComponent<MeshFilter>().mesh.triangles[n + i - 1] = Triangles[i];
        //    O.GetComponent<MeshFilter>().mesh.triangles[n + i] = Triangles[i + 1];
        //    O.GetComponent<MeshFilter>().mesh.triangles[n + i + 1] = Triangles[i + 2];
        //    Debug.Log("okk");
        //}
        /*
         * Mesh的triangles数组不能被动态增加 因此无法增加元素
         */
        Debug.Log("添加完成");
    }
    
    




}
