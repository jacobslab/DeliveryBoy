using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour {
	public Waypoint[] neighbors;

	VisibilityToggler[] waypointArrowVisuals;

	[HideInInspector] public float DijkstraDistance; //for use by the waypoint controller in determining shortest path

	// Use this for initialization
	void Start () {
		waypointArrowVisuals = gameObject.GetComponentsInChildren<VisibilityToggler> ();
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

	//modified from FacePosiion.cs FaceThePosition() function
	void RotateTowards(Vector3 positionToFace){
		Quaternion origRot = transform.rotation;
		transform.LookAt (positionToFace);
		float yRot = transform.rotation.eulerAngles.y;
		
		transform.rotation = Quaternion.Euler (origRot.eulerAngles.x, yRot, origRot.eulerAngles.z);
	}
}
