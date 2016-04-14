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
	public IEnumerator WaitForStoreTrigger(Store store){

		bool areWayPointsEnabled = false;
		
		float timeWaiting = 0.0f;
		
		Debug.Log("WAITING FOR COLLISION/TRIGGER WITH: " + store.name);
		
		string lastTriggerName = "";
		while (lastTriggerName != store.name + " trigger") {
			if(waitForCollisionObject != null){
				lastTriggerName = waitForCollisionObject.name;
			}
			yield return 0;
			
			if(Config.shouldUseWaypoints){
				timeWaiting += Time.deltaTime;
				if(timeWaiting > Config.timeUntilWaypoints){
					//light up path -- should get enabled/updated every frame because player's position changes
					exp.waypointController.EnableWaypoints (exp.player.transform.position, store.transform.position);
					areWayPointsEnabled = true;
				}
			}
		}
		
		if (areWayPointsEnabled) {
			exp.waypointController.DisableWaypoints ();
		}

		Debug.Log ("FOUND STORE");

	}

	public GameObject GetCollisionObject(){
		return waitForCollisionObject;
	}
	
	void OnTriggerEnter(Collider collider){
		waitForCollisionObject = collider.gameObject;

		//log store collision
		if (collider.gameObject.tag == "StoreTrigger"){
			objLogTrack.LogCollision (collider.gameObject.name);
		}
	}

	void OnCollisionEnter(Collision collision){
		waitForCollisionObject = collision.gameObject;

		//log store collision
		if (collision.gameObject.tag == "Store"){
			objLogTrack.LogCollision (collision.gameObject.name);
		}
		
	}


}
