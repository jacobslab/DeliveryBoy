using UnityEngine;
using System.Collections;

public class WaypointController : MonoBehaviour {
	GameObject[] waypoints;

	void Awake(){
		GetWaypoints ();
	}

	void GetWaypoints(){
		waypoints = GameObject.FindGameObjectsWithTag ("Waypoint");
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
