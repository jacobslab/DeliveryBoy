using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildingController : MonoBehaviour {

	public Building[] buildings;

	// Use this for initialization
	void Awake () {
		buildings = GetBuildings ();
	}


	Building[] GetBuildings(){
		return GetComponentsInChildren<Building> ();
	}

	public List<Building> GetRandomDeliveryBuildings(){
		List<Building> deliveryBuildings = new List<Building>();

		List<int> randomIndices = UsefulFunctions.GetRandomIndexOrder (buildings.Length);

		//only add the number of delivery stores
		for (int i = 0; i < Config.numDeliveryStores; i++) {
			deliveryBuildings.Add(buildings[randomIndices[i]]);
		}

		return deliveryBuildings;
	}

	//START ON dbCode.py on line 449
	public List<Building> GetLearningOrderBuildings(){
		List<Building> learningBuildings = new List<Building> ();

		// weight vector (50%, 30%, 15%, 5%) for which closest store to choose
		int[] randWeights = {1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,3,3,3,4};

		float[,] buildingDistances = GetBuildingDistances();

		for(int i = 0; i < buildings.Length; i++){
			// which of the closest stores
			int nextDistWeight = randWeights[Random.Range(0,randWeights.Length)];

			//sort distance vector
			List<float> distancesFromBuilding = GetDistancesFromBuilding(i, buildingDistances);
			List<int> orderedBuildingIndices = GetOrderedBuildingIndicesByDistance(distancesFromBuilding);

			/// HOW TO I USE THE WEIGHT DISTRIBUTION VECTOR EXACTLY?
			//float nextStoreDist = distancesFromBuilding[
			//TODO: FINISH THIS!!!

		}

		return learningBuildings;

	}

	List<float> GetDistancesFromBuilding(int buildingIndex, float[,] allDistances){

		List<float> distancesFromBuilding = new List<float> ();
		//get all distances from building in the order of the buildings
		for (int i = 0; i < buildings.Length; i++) {
			distancesFromBuilding.Add(allDistances[buildingIndex, i]);
		}

		return distancesFromBuilding;

	}

	List<int> GetOrderedBuildingIndicesByDistance(List<float> distancesFromBuilding){

		List<int> orderedBuildingIndices = new List<int> ();
		float minDistance = 0.0f;
		int minDistanceIndex = -1;
		//for num buildings...
		for(int i = 0; i < buildings.Length; i++){
			//pick out the next min distance!
			for (int j = 0; j < distancesFromBuilding.Count; j++) {
				//don't use an index we've already used (which gets set to -1)
				if(distancesFromBuilding[j] != -1){
					//initialize it!
					if(minDistance == -1){
						minDistance = distancesFromBuilding[j];
					}
					else if(minDistance > distancesFromBuilding[j]){
						minDistance = distancesFromBuilding[j];
						minDistanceIndex = j;
					}
				}
			}
			orderedBuildingIndices.Add (minDistanceIndex);
			distancesFromBuilding[minDistanceIndex] = -1; // tell the list we've already used this index
			minDistance = -1; //reset this to initialize it on the next round
		}
		
		return orderedBuildingIndices;

	}
	
	float[,] GetBuildingDistances(){
		float[,] distances = new float[buildings.Length, buildings.Length];

		List<Building> buildingsCopy = new List<Building>();
		for(int i = 0; i < buildings.Length; i++){
			buildingsCopy.Add(buildings[i]);
		}

		for(int i = 0; i < buildings.Length; i++){
			for(int j = i; j < buildingsCopy.Count; j++){ //j = i --> skip all that came before it
				float distance = (buildings[i].transform.position - buildingsCopy[j].transform.position).magnitude;
				//fill in both spots so we don't have to do this distance calculation more than once
				distances[i,j] = distance;
				distances[j,i] = distance;
			}
		}

		return distances;
	}
}
