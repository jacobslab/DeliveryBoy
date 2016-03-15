using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class ObjectController : MonoBehaviour {

	public TextAsset itemTextFile;
	
	List<GameObject> deliverableObjects;
	public Dictionary<string, string> deliverableTextMap;

	List<GameObject> storeImages;

	public Transform deliverableParent;
	public Transform storeImageParent;

	//experiment singleton
	Experiment exp { get { return Experiment.Instance; } }



	// Use this for initialization
	void Start () {
		GetDeliverables ();
		CreateDeliverablesMap ();

		GetStoreImages ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//CREATES A MAP BETWEEN THE OBJECT AND ITS CORRESPONDING TEXT.
	//ASSUMING THAT THE OBJECTS IN THE LIST ARE IN THE SAME ORDER AS IN THE FILE.
	void CreateDeliverablesMap(){
		if (deliverableObjects.Count == 0) {
			GetDeliverables();
		}

		deliverableTextMap = new Dictionary<string, string> ();

		//parse text asset
		string itemTextFileAll = itemTextFile.text;
		string[] itemTextFileLines = itemTextFileAll.Split ('\n');

		int numItems = Mathf.Min (deliverableObjects.Count, itemTextFileLines.Length); //pick smallest amount to avoid any out of bounds exceptions. however, there SHOULD be the same #.
		for (int i = 0; i < numItems; i++) {
			string deliverableName = GetDeliverableName(deliverableObjects[i].name);
			string deliverableText = itemTextFileLines[i];

			if(deliverableText.Contains (deliverableName) ){
				deliverableTextMap.Add(deliverableObjects[i].name, deliverableText);
			}
		}
	}

	void GetDeliverables(){
		deliverableObjects = new List<GameObject> ();

		Object[] deliverables = Resources.LoadAll("Deliverables");
		for(int i = 0; i < deliverables.Length; i++){
			deliverableObjects.Add( (GameObject)deliverables[i] ) ;
		}
	}

	//used in replay
	void CreateCompleteDeliverableList (List<SpawnableObject> spawnableListToFill){
		spawnableListToFill.Clear();
		Object[] deliverables = Resources.LoadAll("Deliverables");
		for (int i = 0; i < deliverables.Length; i++) {
			SpawnableObject spawnable = ( (GameObject)deliverables[i] ).GetComponent<SpawnableObject>();
			spawnableListToFill.Add(spawnable);
		}
		//add in any other things that may need to be spawned here, that may not be deliverables.
	}

	public string GetDeliverableText(GameObject deliverable){
		string deliverableText = "";
		deliverableTextMap.TryGetValue (GetDeliverableName(deliverable.name), out deliverableText);

		if (deliverableText == "") {
			Debug.Log("TEXT IS EMPTY. LOOKING FOR: " + deliverable.name);
		}

		return deliverableText;
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
		CreateCompleteDeliverableList (allSpawnables);

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

	//STORE IMAGES
	void GetStoreImages(){
		storeImages = new List<GameObject> ();
		
		/*Object[] storeImageArray = Resources.LoadAll("StoreImages");
		for(int i = 0; i < storeImageArray.Length; i++){
			storeImages.Add( (GameObject)storeImageArray[i] ) ;
		}*/
		foreach (Transform child in storeImageParent) {
			storeImages.Add(child.gameObject);
		}
	}
	
	public GameObject GetStoreImage(string storeName){
		storeName = storeName + " image";
		for(int i = 0; i < storeImages.Count; i++){
			if(storeName == storeImages[i].name){
				return storeImages[i];
			}
		}
		return null;
	}


	public float GenerateRandomRotationY(){
		float randomRotY = Random.Range (0, 360);
		return randomRotY;
	}
	
	//for more generic object spawning -- such as in Replay!
	public GameObject SpawnObject( GameObject objToSpawn, Vector3 spawnPos ){
		GameObject spawnedObj = Instantiate(objToSpawn, spawnPos, objToSpawn.transform.rotation) as GameObject;

		if(spawnedObj.tag == "Deliverable"){
			spawnedObj.transform.SetParent(deliverableParent, false);
		}

		return spawnedObj;
	}

	//spawn random object at a specified location
	public GameObject SpawnDeliverable (Vector3 spawnPosition){
		GameObject itemToSpawn = ChooseRandomDeliverable ();
		if (itemToSpawn != null) {

			GameObject newItem = Instantiate(itemToSpawn, spawnPosition, Quaternion.identity) as GameObject;

			newItem.transform.SetParent(deliverableParent, false);
			//CurrentTrialSpecialObjects.Add(newObject);

			return newItem;
		}
		else{
			return null;
		}
	}

	/*public GameObject SpawnStoreImage (Vector3 storeImagePos, string storeName){
		GameObject imageToSpawn = GetStoreImage (storeName);
		if (imageToSpawn != null) {
			
			GameObject newItem = Instantiate(imageToSpawn, storeImagePos, Quaternion.identity) as GameObject;
			
			newItem.transform.SetParent(storeImageParent, false);
			//CurrentTrialSpecialObjects.Add(newObject);
			
			return newItem;
		}
		else{
			Debug.Log("No store image of this name!");
			return null;
		}
	}*/

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
