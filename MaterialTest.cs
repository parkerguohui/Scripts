using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Material material;
        material = this.GetComponent<MeshRenderer>().materials[0];
        //material.color=
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
