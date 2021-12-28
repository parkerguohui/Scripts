using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
/// <summary>
/// 注意 本tag在旧版本中并未增加 需要在unity中增加该标签
/// </summary>
public class CheckLoadedObject : MonoBehaviour
{
    public Material material;

    // Use this for initialization
    void Start()
    {
        Debug.Log("check begins");
        GameObject go = GameObject.Find("LoadedObject");
        //测试
        
        //Material material = new Material("RedDefault");
        //Material material = new Material(Shader.Find("red"))
        //{
        //    color = Color.red
        //};
        Debug.Log("OK1~");
        //go.transform.GetChild(0).GetComponent<MeshRenderer>().materials.SetValue(material, 0);
        go.transform.GetChild(0).GetComponent<MeshRenderer>().material=material;
        Debug.Log("OK2~");
        
        //GameObject go = GameObject.FindGameObjectWithTag("LoadedObject");
        //Transform g = go.transform.GetChild(0);
        Transform g = go.transform.GetChild(0);
        //清除与父物体间的关系
        g.SetParent(null);
        //Debug.Log(g != null);
        while (g.GetComponent<MeshFilter>().mesh.bounds.size.x * g.transform.localScale.x > 1.5 || g.GetComponent<MeshFilter>().mesh.bounds.size.y * g.transform.localScale.y > 1.5 || g.GetComponent<MeshFilter>().mesh.bounds.size.z * g.transform.localScale.z > 1.5)
        {
            //物体尺寸超过2米,则缩小物体尺寸10倍
            Debug.Log("ok");
            g.transform.localScale = g.transform.localScale / 2;
        }
        Debug.Log("0");

        Debug.Log("再缩小一倍");
        g.transform.localScale = g.transform.localScale / 2;

        //while (go.GetComponent<MeshFilter>().mesh.bounds.size.x*go.transform.localScale.x > 2|| go.GetComponent<MeshFilter>().mesh.bounds.size.y * go.transform.localScale.y > 2|| go.GetComponent<MeshFilter>().mesh.bounds.size.z * go.transform.localScale.z > 2)
        //{
        //    //物体尺寸超过2米,则缩小物体尺寸10倍
        //    go.transform.localScale = go.transform.localScale / 10;
        //}
        ///<summary>结束后需要将物体标签改回Objects</summary>
        //go.tag = "Objects";
        ///<summary>设置为无父物体后 再添加组建"Choosable""Interactable"即可实现交互的基本条件</summary>
        GameObject defaultobject = GameObject.Find("defaultobject");
        if (defaultobject != null)
            Debug.Log(defaultobject);
        defaultobject.AddComponent<MeshCollider>();
        defaultobject.AddComponent<Interactable>();
        defaultobject.AddComponent<Choosable>();
        defaultobject.tag = "Objects";
        defaultobject.name = "Created in " + System.DateTime.Now.ToShortTimeString();
        defaultobject.transform.position = new Vector3(-0.5f, -0.5f, -0.5f);//设置位置
    }

    // Update is called once per frame
    void Update()
    {

    }
}
