using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class  RollBackHelper : MonoBehaviour {
    //用于执行回退功能 仅处理手术刀切割
    public static GameObject pre ;
    public static GameObject[] CutThings;//默认最多生成5个子物体 
                                                             // Use this for initialization
    private void Awake()
    {
        pre = new GameObject();
        CutThings = new GameObject[5];
    }
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
