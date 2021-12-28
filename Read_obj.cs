using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Networking;
using System;
/// <summary>
/// 用于读取obj文件到项目中直接使用
/// </summary>
public class Read_obj : MonoBehaviour
{
    void Start()
    {
        Debug.Log("文件选择开始");
        //OBJLoader oBJLoader = new OBJLoader();
        //换成新的 进行测试
        //调用开始
        OBJLoader_optimized oBJLoader = new OBJLoader_optimized();
        oBJLoader.SelectFile();
        //结束
        Debug.Log("结束");
    }
}




