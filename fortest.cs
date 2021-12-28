using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fortest : MonoBehaviour {
    //test
    Mesh mesh;
    GameObject haha;
    int n;
    int yoyo_temp = 0;
    // Use this for initialization
    void Start () {
        //test
        mesh = transform.gameObject.GetComponent<MeshFilter>().mesh;
        Debug.Log("=================------>" + mesh.vertices.Length);
        //测试手柄位置
        //beCutThing = GameObject.FindWithTag("ChosenObject");
        //haha = GameObject.Find("haha");
        //Debug.Log(haha);
        //Debug.Log("local::"+transform.InverseTransformPoint(haha.transform.position));
        //Debug.Log("world::"+transform.TransformPoint(haha.transform.position));

        //mesh = gameObject.GetComponent<MeshFilter>().mesh;//获取mesh网格

        //n = mesh.vertexCount;
        //Debug.Log("mesh.vertexCount:  " + mesh.vertexCount);
        //int j = 0;
        //for (int i = 1; i < n; i++)
        //{
        //    if (mesh.vertices[i] != mesh.vertices[0])
        //        ++j;
        //}
        //Debug.Log(j);
        ////已经验证scalpel的mesh网格不一致
        //for (int i = 0; i < n; i++)
        //    Debug.Log("mesh网格" + i + "  :  " + transform.TransformPoint(mesh.vertices[i]));
    }
	
	// Update is called once per frame
	void Update () {
        
        //if (yoyo_temp < n)
        //{
        //    if (Input.GetKeyDown(KeyCode.C))
        //    {
        //        Debug.Log("====" + yoyo_temp + "====");
        //        ++yoyo_temp;
        //        Debug.Log("transform.InverseTransformPoint(mesh.vertices[yoyo_temp]:  " 
        //            + transform.InverseTransformPoint(mesh.vertices[yoyo_temp]));
        //        //haha.transform.position = transform.InverseTransformPoint(mesh.vertices[yoyo_temp]);
        //        Debug.Log("========");
        //    }
            
        //    //Debug.Log("========");
        //}
    }
}
