using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        Debug.Log(Input.GetAxis("Horizontal"));
        Debug.Log(Input.GetAxis("Vertical"));
        Debug.Log(Input.GetButton("x (continue)"));
        Debug.Log(Input.GetButton("b (pause)"));
        Debug.Log(Input.GetButton("q (secret)"));
	}
}
