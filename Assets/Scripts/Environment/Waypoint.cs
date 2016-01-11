using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour {
	VisibilityToggler[] waypointArrowVisuals;

	// Use this for initialization
	void Start () {
		waypointArrowVisuals = gameObject.GetComponentsInChildren<VisibilityToggler> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void LightUp(Vector3 positionToPointTo){
		VisibilityToggler closestArrow= waypointArrowVisuals[0];

		float shortestDistance = -1;

		for(int i = 0; i < waypointArrowVisuals.Length; i++){
			float arrowDistance = (positionToPointTo - waypointArrowVisuals[i].transform.position).magnitude;
			if(shortestDistance == -1 || arrowDistance < shortestDistance){
				shortestDistance = arrowDistance;
			}
			else if(arrowDistance < shortestDistance){
				shortestDistance = arrowDistance;
				closestArrow = waypointArrowVisuals[i];
			}
		}

		closestArrow.TurnVisible (true);
	}
}
