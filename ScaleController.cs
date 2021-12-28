using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScaleController : MonoBehaviour {

    public GameObject X;
    public GameObject Y;
    public GameObject Z;
    public GameObject All;
    public GameObject Object;
    private bool scaleUpdate = false;
    public void SetXYZ()
    {
        X.GetComponent<XYZController>().SetSliderValue(Object.transform.localScale.x*10);
        Y.GetComponent<XYZController>().SetSliderValue(Object.transform.localScale.y * 10);
        Z.GetComponent<XYZController>().SetSliderValue(Object.transform.localScale.z * 10);
        All.GetComponent<AllController>().SetAllScale(0.5f);
        Debug.Log("x local scale :" + Object.transform.localScale.x);
        scaleUpdate = true;
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (scaleUpdate)
        {
            Object.transform.localScale = new Vector3(X.GetComponent<XYZController>().slider.GetComponent<Slider>().value*0.1f, Y.GetComponent<XYZController>().slider.GetComponent<Slider>().value * 0.1f, Z.GetComponent<XYZController>().slider.GetComponent<Slider>().value * 0.1f)* All.GetComponent<AllController>().slider.GetComponent<Slider>().value*2;
        }
	}
}
