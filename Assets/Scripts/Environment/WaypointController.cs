using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	void FindShortestWaypointPath(Vector3 startPoint, Vector3 endPoint){
		List<GameObject> waypointPath = new List<GameObject> ();

		//TODO: find shortest path of waypoints

		//light up the waypoints in the path
		for(int i = 0; i < waypointPath.Count; i++){
			Vector3 closePosition = endPoint;
			if(i != waypointPath.Count - 1){ //if i is not the last waypoint in the path...
				//point to the next waypoint in the path
				closePosition = waypointPath[i + 1].transform.position;
			}
			waypointPath[i].GetComponent<Waypoint>().LightUp(closePosition);
		}
	}
}
