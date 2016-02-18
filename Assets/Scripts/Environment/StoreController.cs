using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StoreController : MonoBehaviour {

	public Store[] stores;
	public List<AudioClip> allStoreAudioLeftToUse;

	// Use this for initialization
	void Awake () {
		stores = GetStores ();

		if (!Config.isStoreCorrelatedDelivery) {
			InitAudio();
		}
	}

	public void InitAudio(){
		allStoreAudioLeftToUse = new List<AudioClip> ();

		for(int i = 0; i < stores.Length; i++){

			string folder = "StoreAudio/" + stores[i].name;

			AudioClip[] storeAudioClips = Resources.LoadAll<AudioClip> (folder);
			for (int j = 0; j < storeAudioClips.Length; j++) {
				allStoreAudioLeftToUse.Add (storeAudioClips [j]);
			}

		}
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

}
