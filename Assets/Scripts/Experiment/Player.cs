using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	Experiment exp { get { return Experiment.Instance; } }

	public PlayerControls controls;
	public GameObject visuals;

	ObjectLogTrack objLogTrack;
	

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

	GameObject waitForCollisionObject;
	bool isLookingForBuilding = false;
	public IEnumerator WaitForBuildingCollision(GameObject building, bool shouldUseWaypoints){
		float timeWaiting = 0.0f;

		isLookingForBuilding = true;
		Debug.Log("WAITING FOR COLLISION WITH: " + building.name);
		
		string lastCollisionName = "";
		while (lastCollisionName != building.name) {
			if(waitForCollisionObject != null){
				lastCollisionName = waitForCollisionObject.name;
			}
			yield return 0;

			if(shouldUseWaypoints){
				timeWaiting += Time.deltaTime;
				if(timeWaiting > Config.timeUntilWaypoints){
					//light up path
					exp.waypointController.IlluminateShortestWaypointPath (exp.player.transform.position, building.transform.position);
				}
			}
		}

		Debug.Log ("FOUND BUILDING");
		
		isLookingForBuilding = false;

	}

	public GameObject GetCollisionObject(){
		return waitForCollisionObject;
	}

	void OnCollisionEnter(Collision collision){
		waitForCollisionObject = collision.gameObject;

		//log building collision
		if (collision.gameObject.tag == "Building"){
			objLogTrack.LogCollision (collision.gameObject.name);
		}
		
	}


}
