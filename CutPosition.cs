using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutPosition {
    /// <summary>
    /// 被切割的原物体，key是该物体局部坐标下的切割面
    /// </summary>
    public static List<GameObject> cutObject = new List<GameObject>();  //被切割的原物体，key是该物体局部坐标下的切割面
    /// <summary>
    /// 切割面的中心点局部坐标
    /// </summary>
    public static List<Vector3> cutPoint = new List<Vector3>();//切割面的中心点局部坐标
    /// <summary>
    /// 切割面的局部坐标
    /// </summary>
    public static List<Vector3> cutPlaneNormal = new List<Vector3>();//切割面的局部法向量
    /// <summary>
    /// 用于辅助生成双层导板
    /// </summary>
    //public static List<GameObject> DoubleHolderToolHelper = new List<GameObject>(5);
    public static GameObject DoubleHolderToolHelper = new GameObject("DoubleHolderToolHelper");
    //public static bool tag = false;
    /// <summary>
    /// 存储切割过程中的产生的切割环
    /// </summary>
    public static Queue<List<Vector3>> EdgesOfCutting = new Queue<List<Vector3>>();
    /// <summary>
    /// 存储网格分离过程中的临时网格数据
    /// </summary>
    public static Mesh TempMeshSaver;
    /// <summary>
    /// 记录局部切割的四个角点  局部坐标
    /// </summary>
    public static List<List<Vector3>> cutAnchorPoints = new List<List<Vector3>>();
    /// <summary>
    /// 临时变量 后期删除
    /// </summary>
    public static Mesh mesh;
    /// <summary>
    /// 临时变量 切割环点
    /// </summary>
    public static List<List<Vector3>> CutPoints = new List<List<Vector3>>();
    /// <summary>
    /// 临时变量 切割环点连接关系
    /// </summary>
    public static List<List<int>> CutLine = new List<List<int>>();
    /// <summary>
    /// 临时变量 切割环点法向量
    /// </summary>
    public static List<List<Vector3>> CutNors = new List<List<Vector3>>();
    ///// <summary>
    ///// 切割角点 按照世界坐标存储
    ///// </summary>
    //public static List<List<Vector3>> CutAnchorPoints = new List<List<Vector3>>();

}
