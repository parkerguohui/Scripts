using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR.InteractionSystem;
using Valve.VR;



[RequireComponent(typeof(Interactable))]
public class Choosable : MonoBehaviour {

    public bool isChosen;


    //public GameObject handle;
    //public SteamVR_Input_Sources source = SteamVR_Input_Sources.Any;
    //public SteamVR_Action_Boolean spawn;
    //private SteamVR_Behaviour_Pose trackedObj;



    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);
    private Interactable interactable;

    // Use this for initialization
    void Start () {
        isChosen = false;

    }

    private void Awake()
    {
        interactable = this.GetComponent<Interactable>();
        
    }

    

    //-------------------------------------------------
    // Called when a Hand starts hovering over this object
    //-------------------------------------------------
    private void OnHandHoverBegin(Hand hand)
    {
        
    }

    //-------------------------------------------------
    // Called when a Hand stops hovering over this object
    //-------------------------------------------------
    private void OnHandHoverEnd(Hand hand)
    {
       
    }

    //-------------------------------------------------
    // Called every Update() while a Hand is hovering over this object
    //-------------------------------------------------
    private void HandHoverUpdate(Hand hand)
    {
        //trackedObj = hand.GetComponent<SteamVR_Behaviour_Pose>();
        GrabTypes startingGrabType = hand.GetGrabStarting();
        bool isGrabEnding = hand.IsGrabEnding(this.gameObject);

        if (SteamVR_Actions.default_InteractUI.GetStateDown(SteamVR_Input_Sources.Any))//抓取物体 设置chosenobject标签
        {
            isChosen = true;


            if (transform.tag == "Objects")
            {

                GameObject o = GameObject.FindGameObjectWithTag("ChosenObject");

                

                if (o)
                {
                    o.tag = "Objects";
                    //ThingToBeCut
                    //DestroyImmediate(o.GetComponent<GetBeCutThing>());//清除识别带切割物体的组件脚本

                }
                transform.tag = "ChosenObject";
                //o.AddComponent<GetBeCutThing>();//增加识别带切割物体的组件脚本 07.28
            }
            else if (transform.tag == "Tools")//抓取工具 设置chosentool标签
            {
                //清理所有被选中工具 因为可能存在复制问题 导致标签重复 2020.08.08
                GameObject[] ChosenTools = GameObject.FindGameObjectsWithTag("ChosenTool");
                for (int i = 0; i < ChosenTools.Length; i++)
                    ChosenTools[i].tag = "Tools";

                GameObject o = GameObject.FindGameObjectWithTag("ChosenTool");
                if (o)
                {
                    o.tag = "Tools";
                }
                transform.tag = "ChosenTool";
            }
            else if (transform.tag == "ToolsChilds")
            {    
                GameObject o = GameObject.FindGameObjectWithTag("ChosenTool");
                if (o)
                {
                    o.tag = "Tools";
                }
                transform.parent.tag = "ChosenTool";
            }
        }

        if (interactable.attachedToHand == null && startingGrabType != GrabTypes.None)   //抓取开始，被抓取物体绑定在手柄上
        {
            

            // Call this to continue receiving HandHoverUpdate messages,
            // and prevent the hand from hovering over anything else
            hand.HoverLock(interactable);

            // Attach this object to the hand
            hand.AttachObject(gameObject, startingGrabType, attachmentFlags);//------------------------------------------------------
        }
        else if (isGrabEnding)
        {
            // Detach this object from the hand
            hand.DetachObject(gameObject);

            // Call this to undo HoverLock
            hand.HoverUnlock(interactable);

            
        }

        if(SteamVR_Actions.default_Menu.GetState(SteamVR_Input_Sources.Any)&& SteamVR_Actions.default_InteractUI.GetState(SteamVR_Input_Sources.Any))//在抓取物体的同时按下菜单键，弹出设置菜单
        {
            if (CompareTag("Objects"))
            {
                OpenChosenObjectMenu();
            }
            else if (CompareTag("Tools"))
            {
                OpenChosenToolMenu();
            }
        }
    }

    private void OpenChosenObjectMenu()
    {

    }
    private void OpenChosenToolMenu()
    {

    }




    //-------------------------------------------------
    // Called when this GameObject becomes attached to the hand
    //-------------------------------------------------
    private void OnAttachedToHand(Hand hand)                                //被手柄抓取
    {
        isChosen = true;

        if (transform.tag == "Objects")
        {

            
            GameObject o = GameObject.FindGameObjectWithTag("ChosenObject");
            if (o)
            {
                o.tag = "Objects";
            }
            transform.tag = "ChosenObject";
        }
        else if (transform.tag == "Tools")
        {

            
            GameObject o = GameObject.FindGameObjectWithTag("ChosenTool");
            if (o)
            {
                o.tag = "Tools";
            }
            transform.tag = "ChosenTool";
        }
        //else if (transform.tag == "ToolsChilds")                     
        //{
        //    
        //    GameObject o = GameObject.FindGameObjectWithTag("ChosenTool");
        //    if (o)
        //    {
        //        o.tag = "Tools";
        //    }
        //    transform.parent.tag = "ChosenTool";
        //}

    }

    //-------------------------------------------------
    // Called when this GameObject is detached from the hand
    //-------------------------------------------------
    private void OnDetachedFromHand(Hand hand)                              //被手柄释放
    {
        isChosen = false;
    }

    //-------------------------------------------------
    // Called every Update() while this GameObject is attached to the hand
    //-------------------------------------------------
    private void HandAttachedUpdate(Hand hand)
    {

    }
    private void Update()
    {
        if (SteamVR_Actions.default_Teleport.GetState(SteamVR_Input_Sources.Any))
        {
            //Debug.Log("teleport");
        }
    }


}
