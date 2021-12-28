using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 用于修改物体标签
/// </summary>
public class TagChanger : MonoBehaviour {
    GameObject BeChoosenObject = GameObject.FindGameObjectWithTag("BeChoosenObject");
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("ok");
    }



}
