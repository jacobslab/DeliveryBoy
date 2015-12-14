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

	//STARTED ON dbCode.py on line 449
	public List<Building> GetLearningOrderBuildings(){
		List<Building> learningBuildings = new List<Building> ();

		//grab buildings as a list for easy mutability
		List<Building> possibleNextBuildings = new List<Building> ();
		for (int i = 0; i < buildings.Length; i++) {
			possibleNextBuildings.Add(buildings[i]);
		}

		// weight vector (50%, 30%, 15%, 5%) for which closest store to choose
		int[] randWeights = {1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,3,3,3,4};

		Building currBuilding = buildings[0]; //ASSUMING THAT THERE ARE MORE THAN ZERO BUILDINGS
		for(int i = 0; i < buildings.Length - 1; i++){

			//add the very first building to the list!
			if(i == 0){
				learningBuildings.Add(currBuilding);
				//remove this building from possible next buildings
				possibleNextBuildings.RemoveAt(i);
			}

			// which of the closest stores
			int nextDistWeight = randWeights[Random.Range(0,randWeights.Length)];

			//sort distance vector
			List<float> buildingDistances = GetBuildingDistances(currBuilding, possibleNextBuildings);
			List<int> orderedBuildingIndices = GetOrderedBuildingIndicesByDistance(buildingDistances);

			/// HOW TO I USE THE WEIGHT DISTRIBUTION VECTOR EXACTLY?
			float nextStoreDist = 0.0f;

			if(nextDistWeight > orderedBuildingIndices.Count){
				nextDistWeight = orderedBuildingIndices.Count;
			}

			int nextStoreIndex = orderedBuildingIndices[nextDistWeight - 1];
			nextStoreDist = buildingDistances[nextStoreIndex];

			//save building to order list
			currBuilding = possibleNextBuildings[nextStoreIndex];
			learningBuildings.Add(currBuilding);
			//remove this building
			possibleNextBuildings.Remove(currBuilding);
		}

		return learningBuildings;

	}
	

	List<int> GetOrderedBuildingIndicesByDistance(List<float> distancesFromBuilding){

		List<float> distancesCopy = new List<float> (distancesFromBuilding);

		List<int> orderedBuildingIndices = new List<int> ();
		float minDistance = -1.0f;
		int minDistanceIndex = -1;

		int numOtherBuildings = distancesFromBuilding.Count;
		for (int i = 0; i < numOtherBuildings; i++) {
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
			orderedBuildingIndices.Add(minDistanceIndex);	// add the min index to the ordered list
			minDistance = -1; //reset this to initialize it on the next round

		}

		return orderedBuildingIndices;

	}

	//will not include itself. (unless otherBuildings does)
	List<float> GetBuildingDistances(Building building, List<Building> otherBuildings){
		List<float> distances = new List<float>();

		for (int i = 0; i < otherBuildings.Count; i++) {
			float distance = (building.transform.position - otherBuildings[i].transform.position).magnitude;
			distances.Add(distance);
		}

		return distances;
	}

}
