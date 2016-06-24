using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	Experiment exp { get { return Experiment.Instance; } }

	public PlayerControls controls;
	public GameObject visuals;

	ObjectLogTrack objLogTrack;

	//WAYPOINTS
	int numWrongTurns = 0;
	Waypoint closestWP;


	// Use this for initialization
	void Start () {
		objLogTrack = GetComponent<ObjectLogTrack> ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void TurnOnVisuals(bool isVisible){
		visuals.SetActive (isVisible);
	}

	GameObject waitForStoreTriggerObject;
	public IEnumerator WaitForStoreTrigger(Store store){

		bool areWayPointsEnabled = false;


		Debug.Log("WAITING FOR COLLISION/TRIGGER WITH: " + store.name);


		string lastTriggerName = "";
		while (lastTriggerName != store.name + " trigger") {
			if(waitForStoreTriggerObject != null){
				lastTriggerName = waitForStoreTriggerObject.name;
			}
			yield return 0;
			
			if(Config.shouldUseWaypoints){

				//set old closest waypoint
				Waypoint oldClosestWP = closestWP;

				//set new closest waypoint
				closestWP = exp.waypointController.GetClosestWaypoint(transform.position);

				//if not enough wrong turns to turn on waypoints, check if we've just made a wrong turn
				if(numWrongTurns < Config.maxNumWrongTurns){
					//if old closest waypoint is not null and we've moved onto the next waypoint
					if(oldClosestWP != null && oldClosestWP != closestWP){
						//if player's old closest WP was an intersection
						if(oldClosestWP.isIntersection){

							//if the new shortest path is longer, we've made a wrong turn!

							//get old path length
							List<Waypoint> path = exp.waypointController.GetShortestWaypointPath(oldClosestWP.transform.position, store.transform.position);
							float oldShortestPathLength = exp.waypointController.GetPathLength(path);

							//get new path length
							path.Clear();
							path = exp.waypointController.GetShortestWaypointPath(closestWP.transform.position, store.transform.position);
							float newShortestPathLength = exp.waypointController.GetPathLength(path);

							//if new length is larger than old length, increment num wrong turns
							if(newShortestPathLength > oldShortestPathLength){
								numWrongTurns++;
								Debug.Log("wrong turn! NUM WRONG: " + numWrongTurns);
							}
						}
					}
				}


				if(numWrongTurns >= Config.maxNumWrongTurns){
					//light up path -- should get enabled/updated every frame because player's position changes
					exp.waypointController.EnableWaypoints (exp.player.transform.position, store.transform.position);
					areWayPointsEnabled = true;
				}




			}
		}
		
		if (areWayPointsEnabled) {
			exp.waypointController.DisableWaypoints ();
			numWrongTurns = 0;
		}

		Debug.Log ("FOUND STORE");

	}

	public GameObject GetStoreTriggerObject(){
		return waitForStoreTriggerObject;
	}

	public void ResetStoreTriggerObject(){
		waitForStoreTriggerObject = null;
	}
	
	void OnTriggerEnter(Collider collider){
		//log store collision
		if (collider.gameObject.tag == "StoreTrigger"){
			waitForStoreTriggerObject = collider.gameObject;
			objLogTrack.LogCollision (collider.gameObject.name);
		}
	}

	void OnCollisionEnter(Collision collision){
		//waitForCollisionObject = collision.gameObject;

		//log store collision
		if (collision.gameObject.tag == "Store"){
			objLogTrack.LogCollision (collision.gameObject.name);
		}
		
	}





}
