using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour {

	public TextMesh myLabel;
	Vector3 origPosition;
	Quaternion origRotation;

	// Use this for initialization
	void Awake () {
		if (myLabel) {
			myLabel.text = gameObject.name;
		}
		origPosition = transform.position;
		origRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Reset(){
		transform.position = origPosition;
		transform.rotation = origRotation;
	}
}
