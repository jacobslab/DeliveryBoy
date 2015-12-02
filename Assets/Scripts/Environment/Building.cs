using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour {

	public TextMesh myLabel;

	// Use this for initialization
	void Start () {
		myLabel.text = gameObject.name;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
