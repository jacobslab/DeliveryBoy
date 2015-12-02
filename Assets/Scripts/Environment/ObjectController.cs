using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class ObjectController : MonoBehaviour {

	string deliverableItemFilePath = "Assets/Text/objectNames.txt";

	public List<string> DeliverableItemText;
	List<GameObject> deliverableObjects;


	//experiment singleton
	Experiment exp { get { return Experiment.Instance; } }



	// Use this for initialization
	void Start () {
		CreateDeliverablesList ();
		GetDeliverables ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void CreateDeliverablesList(){
		DeliverableItemText = new List<string> ();

		//read in from object file.
		StreamReader fileReader = new StreamReader (deliverableItemFilePath);

		string currentLine = fileReader.ReadLine ();

		//PARSE
		while (currentLine != null) {
			DeliverableItemText.Add(currentLine);

			currentLine = fileReader.ReadLine();
		}

		int a = 0;
	}

	void GetDeliverables(){
		deliverableObjects = new List<GameObject> ();

		Object[] deliverables = Resources.LoadAll("Deliverables");
		for(int i = 0; i < deliverables.Length; i++){
			deliverableObjects.Add( (GameObject)deliverables[i] ) ;
		}
	}

	//used in replay
	void CreateCompleteSpawnableList (List<SpawnableObject> spawnableListToFill){
		spawnableListToFill.Clear();
		Object[] specialPrefabs = Resources.LoadAll("Prefabs/Objects");
		Object[] otherPrefabs = Resources.LoadAll("Prefabs/OtherSpawnables");
		for (int i = 0; i < specialPrefabs.Length; i++) {
			SpawnableObject spawnable = ( (GameObject)specialPrefabs[i] ).GetComponent<SpawnableObject>();
			spawnableListToFill.Add(spawnable);
		}
		for (int i = 0; i < otherPrefabs.Length; i++) {
			SpawnableObject spawnable = ( (GameObject)otherPrefabs[i] ).GetComponent<SpawnableObject>();
			spawnableListToFill.Add(spawnable);
		}
	}

	public string GetDeliverableText(GameObject deliverable){
		string deliverableText = "";
		for(int i = 0; i < DeliverableItemText.Count; i++){
			deliverableText = DeliverableItemText[i];

			string deliverableName = GetDeliverableName(deliverable.name);

			if(deliverableText.Contains (deliverableName) ){
				return deliverableText;
			}
		}

		Debug.Log("NO DELIVERABLE IN LIST. LOOKING FOR: " + deliverable.name);

		return "no item";
	}

	string GetDeliverableName(string origName){
		string name = origName;
		name = Regex.Replace( name, "(Clone)", "" );
		name = Regex.Replace( name, "[()]", "" );
		
		return name;
	}

	//used in replay
	public GameObject ChooseSpawnableObject(string objectName){
		List<SpawnableObject> allSpawnables = new List<SpawnableObject>(); //note: this is technically getting instantiated twice now... as it's instantiated in CREATE as well.
		CreateCompleteSpawnableList (allSpawnables);

		for (int i = 0; i < allSpawnables.Count; i++) {
			if(allSpawnables[i].GetName() == objectName){
				return allSpawnables[i].gameObject;
			}
		}
		return null;
	}

	GameObject ChooseRandomDeliverable(){
		if (deliverableObjects.Count == 0) {
			Debug.Log ("No MORE objects to pick! Recreating object list.");
			GetDeliverables(); //IN ORDER TO REFILL THE LIST ONCE ALL OBJECTS HAVE BEEN USED
			if(deliverableObjects.Count == 0){
				Debug.Log ("No objects to pick at all!"); //if there are still no objects in the list, then there weren't any to begin with...
				return null;
			}
		}


		int randomObjectIndex = Random.Range(0, deliverableObjects.Count);
		GameObject chosenItem = deliverableObjects[randomObjectIndex];
		deliverableObjects.RemoveAt(randomObjectIndex);
		
		return chosenItem;
	}


	public float GenerateRandomRotationY(){
		float randomRotY = Random.Range (0, 360);
		return randomRotY;
	}
	
	//for more generic object spawning -- such as in Replay!
	public GameObject SpawnObject( GameObject objToSpawn, Vector3 spawnPos ){
		GameObject spawnedObj = Instantiate(objToSpawn, spawnPos, objToSpawn.transform.rotation) as GameObject;

		return spawnedObj;
	}

	//spawn random object at a specified location
	public GameObject SpawnDeliverable (Vector3 spawnPosition){
		GameObject itemToSpawn = ChooseRandomDeliverable ();
		if (itemToSpawn != null) {

			GameObject newItem = Instantiate(itemToSpawn, spawnPosition, Quaternion.identity) as GameObject;

			//CurrentTrialSpecialObjects.Add(newObject);

			return newItem;
		}
		else{
			return null;
		}
	}



	List<Vector2> SortByNextClosest(List<Vector2> positions, Vector2 distancePos){

		List<Vector2> sortedPositions = new List<Vector2>();
		int numPositions = positions.Count;

		Vector2 closestPos = GetClosest (distancePos, positions);
		sortedPositions.Add (closestPos);
		positions.Remove (closestPos);
		for (int i = 1; i < numPositions; i++) {
			closestPos = GetClosest (closestPos, positions);
			sortedPositions.Add (closestPos);
			positions.Remove (closestPos);
		}

		return sortedPositions;
	}

	Vector2 GetClosest(Vector2 position, List<Vector2> otherPositions){
		float minDist = 0.0f;
		int minDistIndex = 0;
		for (int i = 0; i< otherPositions.Count; i++) {
			float dist = UsefulFunctions.GetDistance(position, otherPositions[i]);
			if(i == 0){
				minDist = dist;
				minDistIndex = i;
			}
			else if(dist < minDist){
				minDist = dist;
				minDistIndex = i;
			}
		}

		return otherPositions [minDistIndex];
	}
	

}
