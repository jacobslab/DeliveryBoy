using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class StoreController : MonoBehaviour {

	public Store[] stores;
	public List<AudioClip> allStoreAudioLeftToUse;

	// Use this for initialization
	bool isFirstInit = true;
	void Awake () {
		stores = GetStores ();

		InitAudio();

	}

	public void InitAudio(){
		if (!Config.isStoreCorrelatedDelivery || ExperimentSettings.isReplay) { //if it's replay, we draw from here to replay the audio

			allStoreAudioLeftToUse = new List<AudioClip> ();

			for (int i = 0; i < stores.Length; i++) {

				string folder = "StoreAudio/" + stores [i].name;

				AudioClip[] storeAudioClips = Resources.LoadAll<AudioClip> (folder);
				for (int j = 0; j < storeAudioClips.Length; j++) {
					allStoreAudioLeftToUse.Add (storeAudioClips [j]);
				}

			}
		} 
		else { // INDIVIDUAL STORE AUDIO
			//init audio for each store
			for(int i = 0; i < stores.Length; i++){
				stores[i].InitAudio();
			}

			if(isFirstInit && Experiment.sessionID != 0){
				//we should read in the last session's store & leftover item file and get rid of use items
				ParseOutUsedItemAudio();	
			}
		}

		isFirstInit = false;
	}

	void ParseOutUsedItemAudio(){
		if (Experiment.sessionID != 0) {
			string readLastSessionFilePath = ExperimentSettings.Instance.GetStoreItemFilePath (false);
			if(File.Exists(readLastSessionFilePath)){
				StreamReader sr = new StreamReader(readLastSessionFilePath);

				string line = sr.ReadLine();
				line = UsefulFunctions.ParseOutHiddenCharacters(line);

				Store currStore = null;
				List<string> unusedAudioNames = new List<string>();
				while(line != "" && line != null){
					string[] lineArr = line.Split('\t');

					if(lineArr.Length > 0){
						if(lineArr[0] == "BUILDING"){
							if(currStore != null){
								currStore.CleanOutAudioLeft(unusedAudioNames);
							}

							currStore = GetStoreByName(lineArr[1]);
							unusedAudioNames.Clear();
						}

						if(lineArr[0] == "ITEM"){
							if(currStore != null){
								unusedAudioNames.Add(lineArr[1]);
							}
						}
					}

					line = sr.ReadLine();
					line = UsefulFunctions.ParseOutHiddenCharacters(line);
				}

				//for the last store, make sure it gets cleaned.
				if(currStore != null){
					currStore.CleanOutAudioLeft(unusedAudioNames);
				}
			}
			else{
				Debug.Log("NO LAST SESSION STORE ITEM FILE");
			}
		}
	}

	public AudioClip GetAudioClipByName(string audioName){
		for (int i = 0; i < allStoreAudioLeftToUse.Count; i++) {
			if(allStoreAudioLeftToUse[i].name == audioName){
				return allStoreAudioLeftToUse[i];
			}
		}

		return null;
	}

	Store GetStoreByName(string storeName){
		for (int i = 0; i < stores.Length; i++) {
			if(stores[i].name == storeName){
				return stores[i];
			}
		}

		return null;
	}
	
	Store[] GetStores(){
		return GetComponentsInChildren<Store> ();
	}

	public List<Store> GetRandomDeliveryStores(){
		List<Store> deliveryStores = new List<Store>();

		List<int> randomIndices = UsefulFunctions.GetRandomIndexOrder (stores.Length);

		//only add the number of delivery stores
		for (int i = 0; i < Config.numDeliveryStores; i++) {
			deliveryStores.Add(stores[randomIndices[i]]);
		}

		return deliveryStores;
	}

	//STARTED ON dbCode.py on line 449
	public List<Store> GetLearningOrderStores(){
		List<Store> learningStores = new List<Store> ();

		//grab stores as a list for easy mutability
		List<Store> possibleNextStores = new List<Store> ();
		for (int i = 0; i < stores.Length; i++) {
			possibleNextStores.Add(stores[i]);
		}

		// weight vector (50%, 30%, 15%, 5%) for which closest store to choose
		int[] randWeights = {1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,3,3,3,4};

		Store currStore = stores[0]; //ASSUMING THAT THERE ARE MORE THAN ZERO STORES
		for(int i = 0; i < stores.Length - 1; i++){

			//add the very first store to the list!
			if(i == 0){
				learningStores.Add(currStore);
				//remove this store from possible next stores
				possibleNextStores.RemoveAt(i);
			}

			// which of the closest stores
			int nextDistWeight = randWeights[Random.Range(0,randWeights.Length)];

			//sort distance vector
			List<float> storeDistances = GetStoreDistances(currStore, possibleNextStores);
			List<int> orderedStoreIndices = GetOrderedStoreIndicesByDistance(storeDistances);

			/// HOW TO I USE THE WEIGHT DISTRIBUTION VECTOR EXACTLY?
			float nextStoreDist = 0.0f;

			if(nextDistWeight > orderedStoreIndices.Count){
				nextDistWeight = orderedStoreIndices.Count;
			}

			int nextStoreIndex = orderedStoreIndices[nextDistWeight - 1];
			nextStoreDist = storeDistances[nextStoreIndex];

			//save store to order list
			currStore = possibleNextStores[nextStoreIndex];
			learningStores.Add(currStore);
			//remove this store
			possibleNextStores.Remove(currStore);
		}

		return learningStores;

	}
	

	List<int> GetOrderedStoreIndicesByDistance(List<float> distancesFromStore){

		List<float> distancesCopy = new List<float> (distancesFromStore);

		List<int> orderedStoreIndices = new List<int> ();
		float minDistance = -1.0f;
		int minDistanceIndex = -1;

		int numOtherStores = distancesFromStore.Count;
		for (int i = 0; i < numOtherStores; i++) {
			for(int j = 0; j < distancesCopy.Count; j++){
				float currDist = distancesCopy[j];
				if(minDistance == -1.0f || minDistance > currDist){
					if(currDist > -1.0f){
						minDistanceIndex = j;
						minDistance = currDist;
					}
				}
			}

			distancesCopy[minDistanceIndex] = -1;	// tell the list we've already used this index
			orderedStoreIndices.Add(minDistanceIndex);	// add the min index to the ordered list
			minDistance = -1; //reset this to initialize it on the next round

		}

		return orderedStoreIndices;

	}

	//will not include itself. (unless otherStores does)
	List<float> GetStoreDistances(Store store, List<Store> otherStores){
		List<float> distances = new List<float>();

		for (int i = 0; i < otherStores.Count; i++) {
			float distance = (store.transform.position - otherStores[i].transform.position).magnitude;
			distances.Add(distance);
		}

		return distances;
	}

	void RecordStoresAndItemsLeft(){

		Debug.Log ("Recording stores & items left!");

		string recordPath = ExperimentSettings.Instance.GetStoreItemFilePath (true);

		StreamWriter sr = new StreamWriter (recordPath);

		sr.WriteLine ("NAME\t" + ExperimentSettings.currentSubject.name);
		sr.WriteLine ("SESSION\t" + Experiment.sessionID);

		for (int storeIndex = 0; storeIndex < stores.Length; storeIndex++) {
			Store currStore = stores[storeIndex];
			sr.WriteLine ("BUILDING\t" + currStore.name);
			for(int itemIndex = 0; itemIndex < currStore.audioLeftToUse.Count; itemIndex++){
				sr.WriteLine ("ITEM\t" + currStore.audioLeftToUse[itemIndex].name);
			}
		}

		sr.Flush ();
		sr.Close ();
	}

	void OnDestroy(){ //should get called when scene changes (or application quits?)
		RecordStoresAndItemsLeft ();
	}

}
