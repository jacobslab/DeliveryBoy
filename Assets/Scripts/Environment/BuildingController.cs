using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildingController : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public Building[] GetBuildings(){
		return GetComponentsInChildren<Building> ();
	}

	public List<Building> GetRandomDeliveryBuildings(){
		List<Building> deliveryBuildings = new List<Building>();

		Building[] buildings = GetBuildings ();
		List<int> randomIndices = UsefulFunctions.GetRandomIndexOrder (buildings.Length);

		for (int i = 0; i < Config.numDeliveryStores; i++) {
			deliveryBuildings.Add(buildings[randomIndices[i]]);
		}

		return deliveryBuildings;
	}
}
