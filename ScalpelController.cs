using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;
[RequireComponent(typeof(Interactable))]
public class ScalpelController : MonoBehaviour
{

    public bool isChosen;
    public Material transparency;
    private Material originalMaterial;
    private GameObject g;
    private bool cutting;
    private Plane cutPlane;

    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);
    private Interactable interactable;

    // Use this for initialization
    void Start()
    {
        isChosen = false;
        cutting = false;
    }

    private void Awake()
    {
        interactable = this.GetComponent<Interactable>();
        originalMaterial = GetComponent<MeshRenderer>().material;
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

        if (SteamVR_Actions.default_InteractUI.GetStateDown(SteamVR_Input_Sources.Any))
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


        //if (SteamVR_Actions.default_Teleport.GetState(SteamVR_Input_Sources.Any) 
        //    && SteamVR_Actions.default_InteractUI.GetState(SteamVR_Input_Sources.Any))//抓取手术刀的时候按下功能键，表示切割开始
        //{
        //    //transform.Translate(transform.TransformPoint(new Vector3(0, 0, transform.position.z)));
        //    if (cutting == false)//说明切割刚刚开始，记录下此刻手术刀所在平面来作为切面，然后将手术刀投影到这个切面上
        //    {
        //        cutting = true;
        //        g = Instantiate(gameObject, transform.position, transform.rotation);
        //        transform.GetComponent<MeshRenderer>().material = transparency;
        //        cutPlane = new Plane(transform.TransformVector(new Vector3(0, 0, 1)), transform.position);
        //    }
        //    g.transform.position = cutPlane.ClosestPointOnPlane(transform.position);
        //    int roop = 1;
        //    int n = Random.Range(2, 5);
        //    for (int j = 0; j < n; j++)
        //    {
        //        for (int i = 0; i < 500000; i++)
        //        {
        //            roop++;
        //        }
        //    }

        //}
        //else
        //{
        //    if (cutting == true)//说明切割刚刚结束
        //    {
        //        cutting = false;
        //        Destroy(g);
        //        transform.GetComponent<MeshRenderer>().material = originalMaterial;
        //        int roop = 1;
        //        for (int j = 0; j < 1; j++)
        //        {
        //            for (int i = 0; i < 80000000; i++)
        //            {
        //                roop++;
        //            }
        //        }
        //    }
        //}
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
    //    // Called when this GameObject is detached from the hand
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

    }
}
