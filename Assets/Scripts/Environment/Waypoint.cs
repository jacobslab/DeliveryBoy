using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class Waypoint : MonoBehaviour {
	public Waypoint[] neighbors;
	public bool isIntersection = false;

	VisibilityToggler[] waypointArrowVisuals;

	[HideInInspector] public float DijkstraDistance; //for use by the waypoint controller in determining shortest path

	// Use this for initialization
	void Start () {
		InitChildren ();
	}

	void InitChildren(){
		string IDString = GetIDString ();
		foreach (Transform child in transform) {
			child.name += IDString;
		}

		waypointArrowVisuals = GetComponentsInChildren<VisibilityToggler> ();
		TurnOff ();
	}

	string GetIDString(){
		//also used in Replay.cs
		Regex numAlpha = new Regex("(?<Alpha>[a-zA-Z ']*)(?<Numeric>[0-9]*)");
		Match match = numAlpha.Match(gameObject.name);
		//string objShortName = match.Groups["Alpha"].Value;
		string IDString = match.Groups["Numeric"].Value;

		return IDString;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void TurnOff(){
		for(int i = 0; i < waypointArrowVisuals.Length; i++){
			waypointArrowVisuals[i].TurnVisible(false);
		}
	}

	void TurnOn(){
		for(int i = 0; i < waypointArrowVisuals.Length; i++){
			waypointArrowVisuals[i].TurnVisible(true);
		}
	}

	public void LightUp(Vector3 positionToPointTo){
		TurnOn ();

		RotateTowards (positionToPointTo);
	}

	//modified from FacePosition.cs FaceThePosition() function
	void RotateTowards(Vector3 positionToFace){
		Quaternion origRot = transform.rotation;
		transform.LookAt (positionToFace);
		float yRot = transform.rotation.eulerAngles.y;
		
		transform.rotation = Quaternion.Euler (origRot.eulerAngles.x, yRot, origRot.eulerAngles.z);
	}
}
