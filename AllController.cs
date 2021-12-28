using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AllController : MonoBehaviour {
    public GameObject slider;
    public GameObject inputField;
    private Slider s;
    private SliderController sc;
    public void SetAllScale(float value)
    {
        sc = slider.GetComponent<SliderController>();
        sc.SetValue(value);
    }

    // Use this for initialization
    void Start () {
        s = slider.GetComponent<Slider>();
    }
	
	// Update is called once per frame
	void Update () {
        inputField.GetComponent<InputField>().text = (s.value * 2).ToString();
    }
}
