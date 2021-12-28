using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;
using Valve.VR;
using ImgSpc.Exporters;
using System.IO;

public class ChosenObjectMenuController : MonoBehaviour {
    public GameObject beChosenObject;
    public int beChosenObjectType;
    public GameObject SetScale;
    public GameObject SetScaleUI;
    public GameObject SetColorUI;
    public GameObject TagUI;
    public static int Export_num = 1;

    // Use this for initialization
    void Start()
    {
        Open();
    }


    // Update is called once per frame
    void Update()
    {
       
    }

    public void Open()
    {
        CloseAll();
        if (beChosenObjectType == 1)
        {
            SetScale.SetActive(false);
        }
        else if (beChosenObjectType == 2)
        {
            SetScale.SetActive(true);
        }
        //gh修改 有误
        //GameObject temp = GameObject.Find("Player").transform.GetChild(0).GetChild(2).gameObject;
        //GameObject temp = GameObject.FindWithTag("ChosenObject");
        //Debug.Log(temp.name);
        //Debug.Log("右手位置: " + temp.transform.TransformPoint(temp.transform.position));
        //transform.position = temp.transform.TransformPoint(temp.transform.position);//设置位置
        //transform.rotation = Quaternion.Euler(temp.transform.rotation.eulerAngles);//设置位置
        //transform.position = temp.transform.position;//设置位置
        //Debug.Log(transform.gameObject);
        //transform.position = new Vector3(temp.transform.position.x, temp.transform.position.y - 0.2f, temp.transform.position.z);
        //transform.position = (transform.position + temp.transform.position) / 2;
        //transform.rotation = temp.transform.rotation;
        //transform.rotation = Quaternion.Euler(temp.transform.TransformVector(temp.transform.rotation.eulerAngles));//设置位置
        //Debug.Log("菜单位置: " + transform.position);
    }

    public void SetScaleClick()
    {
        if (SetScaleUI.activeInHierarchy)
        {
            CloseAll();
        }
        else
        {
            CloseAll();
            SetScaleUI.SetActive(true);
            SetScaleUI.GetComponent<ScaleController>().Object = beChosenObject;
            SetScaleUI.GetComponent<ScaleController>().SetXYZ();
        }
        
    }
    public void SetColorClick()
    {
        if (SetColorUI.activeInHierarchy)
        {
            CloseAll();
        }
        else
        {
            CloseAll();
            SetColorUI.SetActive(true);
        }
    }
    public void CopyClick()
    {
        CloseAll();
        GameObject go = Instantiate(beChosenObject, transform.position + transform.forward * (float)0.1, Quaternion.identity);
        if(beChosenObject.tag== "ChosenObject" || beChosenObject.tag == "Objects")
        {
            go.tag = "Objects";
        }
        if (beChosenObject.tag == "ChosenTool" || beChosenObject.tag == "Tools")
        {
            go.tag = "Tools";
        }
    }
    public void ExportClick()
    {
        CloseAll();
        StlExporter();
    }
    public void DeleteClick()
    {
        CloseAll();
        Destroy(beChosenObject);

        gameObject.SetActive(false);
    }
    public void CloseAll()
    {
        SetScaleUI.SetActive(false);
        SetColorUI.SetActive(false);
    }

    public void ColorChange(string color)
    {
        //Material m = new Material(Shader.Find("Standard"));
        //m.color =color;
        //beChosenObject.GetComponent<Material>().color =new Color();
    }
    public void SetTag()
    {
        CloseAll();
        TagUI.SetActive(true);
        Debug.Log("打开设置标签界面");
    }
    public void SetTagObjects()
    {
        
        if(beChosenObject != null)
        {
            beChosenObject.tag = "ChosenObject";
            Debug.Log("改为ChosenObject");
        }
        else
        {
            Debug.Log("未找到物体");
        }
    }
    public void SetTagChoosenObject()
    {
        Debug.Log("进入");
        if (beChosenObject != null)
        {
            beChosenObject.tag = "Objects";
            Debug.Log("改为Objects");
        }
        else
        {
            Debug.Log("未找到物体");
        }
    }
    public void StlExporter()
    {
        ImgSpcExporter imgSpcExporter = new ImgSpcExporter();
        //string path = Application.dataPath + "/ExportFile/" + "第" + Export_num + "个导出的文件.stl";
        string path = Application.dataPath + "/ExportFile";
        if (Directory.Exists(path))
        {
            Directory.CreateDirectory(path + "ExportFile");
        }
        Debug.Log("path: " + path);
        path = path + "/" + "第" + Export_num++ + "个导出的文件.stl";
        Debug.Log("path: " + path);
        imgSpcExporter.SetFilename(path);//设置导出文件夹
        //执行导出操作
        GameObject game = GameObject.FindWithTag("ChosenObject");
        Debug.Log("game:" + (game == null ? "yes" : "no"));
        if (game != null)
        {
            imgSpcExporter.ObjectsToExport = new Object[1];
            imgSpcExporter.ObjectsToExport[0] = game;
            imgSpcExporter.Export();
            Debug.Log("已经导出！！");
        }
        else
        {
            Debug.Log("未找到物体");
        }
    }
    /// <summary>
    /// 将pre进行显示 分离后的子物体不显示
    /// </summary>
    public void RollBack()
    {
        RollBackHelper.pre.SetActive(true);
        for (int i = 0; i < RollBackHelper.CutThings.Length; i++)
        {
            //Debug.Log(i);
            if (RollBackHelper.CutThings[i] != null)
            {
                RollBackHelper.CutThings[i].SetActive(false);
            }
            else
                break;
        }
        //清除附着的标记
        //DestroyImmediate(RollBackHelper.pre.transform.GetChild(0));
        //DestroyImmediate(RollBackHelper.pre.transform.GetChild(1));
        for(int i=0;i<RollBackHelper.pre.transform.childCount;i++)
        {
            Debug.Log(i);
            //DestroyImmediate(RollBackHelper.pre.transform.GetChild(i));//清除对应子物体
            DestroyImmediate(RollBackHelper.pre.transform.GetChild(i).gameObject);//清除对应子物体
        }
    }

}
