using UnityEngine;
using System.Collections;

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
}
