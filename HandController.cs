using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;

public class HandController : MonoBehaviour {

    public SteamVR_Action_Boolean trackPad;
    public SteamVR_Action_Boolean grip;
    public SteamVR_Action_Boolean trigger;
    public SteamVR_Action_Boolean menu;
    private SteamVR_Behaviour_Pose trackedObj;


    public GameObject menuUI;
    public GameObject ChosenObjectMenuUI;
    private void Awake()
    {
        trackedObj = GetComponent<SteamVR_Behaviour_Pose>();
        menuUI.SetActive(false);
        ChosenObjectMenuUI.SetActive(false);
        //SteamVR_Actions.SetOpacity
    }

    private void Update()
    {
        //if (trackPad.GetStateDown(trackedObj.inputSource)){
        //    Debug.Log("trackPad");
        //}
        //if (trigger.GetStateDown(trackedObj.inputSource))
        //{
        //    Debug.Log("trigger");
        //}
        //if (grip.GetStateDown(trackedObj.inputSource))
        //{
        //    Debug.Log("grip");
        //}
        if (!(trigger.GetState(trackedObj.inputSource))&&menu.GetStateDown(trackedObj.inputSource))  //点击菜单键
        {
            Debug.Log("menu");
            if (trigger.state)
            {

            }
            else
            {
                if (menuUI.activeInHierarchy)
                {
                    menuUI.SetActive(false);
                }
                else
                {
                    menuUI.SetActive(true);
                    //menuUI.transform.position = transform.position + transform.forward * (float)0.18;
                    menuUI.transform.position = transform.position ;
                    menuUI.transform.LookAt(transform.parent.GetChild(3));
                    menuUI.transform.localEulerAngles = new Vector3(0, menuUI.transform.localEulerAngles.y, 0);
                }
            }
            

        }
                                                       //按下扳机键同时点击菜单键
        if (trigger.GetState(trackedObj.inputSource) && menu.GetStateDown(trackedObj.inputSource))
        {
            Debug.Log("物体菜单开关");
            if (ChosenObjectMenuUI.activeInHierarchy) //物体菜单若已打开，则关闭物体菜单
            {
                ChosenObjectMenuUI.GetComponent<ChosenObjectMenuController>().CloseAll();
                ChosenObjectMenuUI.SetActive(false);
            }
            else //否则，检测被抓取的物体
            {
                GameObject beChosenObject;
                beChosenObject = GameObject.FindGameObjectWithTag("ChosenObject");
                //GameObject hand = GameObject.Find("RightHand");
                
                if (beChosenObject&&beChosenObject.GetComponent<Choosable>().isChosen)//菜单未打开时
                {
                    //打开物体菜单

                    ChosenObjectMenuUI.SetActive(true);

                    ChosenObjectMenuUI.GetComponent<ChosenObjectMenuController>().beChosenObject = beChosenObject;
                    ChosenObjectMenuUI.GetComponent<ChosenObjectMenuController>().beChosenObjectType = 1;
                    ChosenObjectMenuUI.GetComponent<ChosenObjectMenuController>().Open();

                    //Vector3 v = Vector3.Cross((transform.parent.GetChild(3).transform.position-beChosenObject.transform.position).normalized, new Vector3(0, 1, 0).normalized).normalized;
                    //ChosenObjectMenuUI.transform.position = beChosenObject.transform.position ;
                    //ChosenObjectMenuUI.transform.LookAt(transform.parent.GetChild(3));
                    //ChosenObjectMenuUI.transform.localEulerAngles = new Vector3(0, ChosenObjectMenuUI.transform.localEulerAngles.y, 0);
                    ChosenObjectMenuUI.transform.position = transform.position ;//直接赋值
                    ChosenObjectMenuUI.transform.LookAt(transform.parent.GetChild(3));
                    ChosenObjectMenuUI.transform.localEulerAngles = new Vector3(0, ChosenObjectMenuUI.transform.localEulerAngles.y, 0);

                }
                else
                {
                    beChosenObject = GameObject.FindGameObjectWithTag("ChosenTool");
                        
                    if (beChosenObject&&beChosenObject.GetComponent<Choosable>()&&beChosenObject.GetComponent<Choosable>().isChosen)
                    {
                        //打开工具物体菜单

                        ChosenObjectMenuUI.SetActive(true);
                        ChosenObjectMenuUI.GetComponent<ChosenObjectMenuController>().beChosenObject = beChosenObject;
                        ChosenObjectMenuUI.GetComponent<ChosenObjectMenuController>().beChosenObjectType = 2;
                        ChosenObjectMenuUI.GetComponent<ChosenObjectMenuController>().Open();

                        ChosenObjectMenuUI.transform.position = beChosenObject.transform.position;
                        ChosenObjectMenuUI.transform.LookAt(transform.parent.GetChild(3));
                        ChosenObjectMenuUI.transform.localEulerAngles = new Vector3(0, ChosenObjectMenuUI.transform.localEulerAngles.y, 0);
                    }
                    else
                    {
                        GameObject[] objs = GameObject.FindGameObjectsWithTag("ToolsChilds");
                        for (int i = 0; i < objs.Length; i++)
                        {
                            beChosenObject = objs[i];
                            if (beChosenObject.GetComponent<Choosable>() && beChosenObject.GetComponent<Choosable>().isChosen)//有子物体被抓取                  XXXXX
                            {
                                ChosenObjectMenuUI.SetActive(true);
                                ChosenObjectMenuUI.GetComponent<ChosenObjectMenuController>().beChosenObject = beChosenObject.transform.GetChild(i).gameObject;
                                ChosenObjectMenuUI.GetComponent<ChosenObjectMenuController>().beChosenObjectType = 2;
                                ChosenObjectMenuUI.GetComponent<ChosenObjectMenuController>().Open();

                                ChosenObjectMenuUI.transform.position = beChosenObject.transform.position;
                                ChosenObjectMenuUI.transform.LookAt(transform.parent.GetChild(3));
                                ChosenObjectMenuUI.transform.localEulerAngles = new Vector3(0, ChosenObjectMenuUI.transform.localEulerAngles.y, 0);
                                break;
                            }
                        }
                    }
                            
                        
                        
                        
                }
               

            }

        }








    }

}
