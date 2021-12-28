using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour {
    public GameObject ToolUI;
    public GameObject FileUI;
    public GameObject EditUI;
    public GameObject SetUI;
    public GameObject HelpUI;

    public GameObject cutPlane;
    public GameObject PartiallyCut;
    public GameObject holder;
    public GameObject scalpel;
    public GameObject doubleHolder;
    public GameObject halfHolder;

    // Use this for initialization
    void Start () {
        ToolClick();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ToolClick()
    {
        CloseAll();
        ToolUI.SetActive(true);
    }
    public void FileClick()
    {
        CloseAll();
        FileUI.SetActive(true);
    }
    public void EditClick()
    {
        CloseAll();
        EditUI.SetActive(true);
    }
    public void SetClick()
    {
        CloseAll();
        SetUI.SetActive(true);
    }
    public void HelpClick()
    {
        CloseAll();
        HelpUI.SetActive(true);
    }
    private void CloseAll()
    {
        ToolUI.SetActive(false);
        FileUI.SetActive(false);
        EditUI.SetActive(false);
        SetUI.SetActive(false);
        HelpUI.SetActive(false);
    }


    public void Tool_Scalpel_Create_Click()
    {
        Debug.Log("生成手术刀工具");
        GameObject go = Instantiate(scalpel, transform.position + transform.forward * (float)0.1, Quaternion.identity);
        go.tag = "Tools";
    }
    public void Tool_Scalpel_Delete_Click()
    {
        Debug.Log("删除手术刀工具");
        Destroy(GameObject.Find("Scalpel(Clone)"));
    }

    public void Tool_PartiallyCut_Create_Click()
    {
        Debug.Log("生成局部切割工具");
        GameObject go = Instantiate(PartiallyCut, transform.position + transform.forward * (float)0.1, Quaternion.identity);
        go.tag = "Tools";
    }
    public void Tool_PartiallyCut_Delete_Click()
    {
        Debug.Log("删除局部切割工具");
        Destroy(GameObject.Find("LocalCutPlaneTool(Clone)"));
    }

    public void Tool_CutPlane_Create_Click()
    {
        Debug.Log("生成平面切割工具");
        GameObject go = Instantiate(cutPlane, transform.position + transform.forward * (float)0.1, Quaternion.identity);
        go.tag = "Tools";
    }
    public void Tool_CutPlane_Delete_Click()
    {
        Debug.Log("删除平面切割工具");
        Destroy(GameObject.Find("CutPlaneTool(Clone)"));
    }

    public void Tool_Holder_Create_Click()
    {
        Debug.Log("生成导板工具");
        GameObject go = Instantiate(holder,transform.position+transform.forward*(float)0.1,Quaternion.identity);
        go.tag = "Tools";
    }
    public void Tool_Holder_Delete_Click()
    {
        Debug.Log("删除导板工具");
        Destroy(GameObject.Find("HolderTool(Clone)"));
    }

    public void Tool_DoubleHolder_Create_Click()
    {
        Debug.Log("生成双导板工具");
        GameObject go = Instantiate(doubleHolder, transform.position + transform.forward * (float)0.1, Quaternion.identity);
        go.tag = "Tools";
    }
    public void Tool_DoubleHolder_Delete_Click()
    {
        Debug.Log("删除双导板工具");
        Destroy(GameObject.Find("DoubleHolderTool(Clone)"));
    }

    public void Tool_HalfHolder_Create_Click()
    {
        Debug.Log("生成半导板工具");
        GameObject go = Instantiate(halfHolder, transform.position + transform.forward * (float)0.1, Quaternion.identity);
        go.tag = "Tools";
    }
    public void Tool_HalfHolder_Delete_Click()
    {
        Debug.Log("删除半导板工具");
        Destroy(GameObject.Find("HalfHolderTool(Clone)"));
    }
    
}
