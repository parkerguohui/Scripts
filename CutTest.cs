using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;
public class CutTest : MonoBehaviour {
    private GameObject beCutThing;
    Mesh mesh;
    Vector3[] vertices;
    // Use this for initialization
    void Start () {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        Debug.Log(transform.TransformPoint(vertices[1]));

        Debug.Log(transform.TransformPoint(vertices[3]));



    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(1) || SteamVR_Actions.default_Teleport.GetStateDown(SteamVR_Input_Sources.Any))
        {
            if (gameObject.tag == "ChosenTool")
            {
                beCutThing = GameObject.FindGameObjectWithTag("ChosenObject");
                if (beCutThing != null)
                {
                    
                    Debug.Log("cut begin~~~");
                    Cut();
                }
                else
                {
                    Debug.Log("there is nothing to cut!");
                }

            }
        }
    }

    private void Cut()
    {

    }

}
