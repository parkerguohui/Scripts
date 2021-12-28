using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class XYZController : MonoBehaviour {
    public GameObject slider;
    public GameObject inputField;

    //private GameObject inputFieldText;
    private Slider s;
    private SliderController sc;
    public void SetSliderValue(float value)
    {
        sc = slider.GetComponent<SliderController>();
        sc.SetValue(value);
        
    }

	// Use this for initialization
	void Start () {
        //inputFieldText = inputField.transform.Find("Text").gameObject;

        s = slider.GetComponent<Slider>();
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(s.value);
        //inputFieldText.GetComponent<Text>().text = (s.value*100).ToString();
        inputField.GetComponent<InputField>().text = (s.value * 100).ToString();




    }
}
