﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaypointController : MonoBehaviour {
	Experiment exp { get { return Experiment.Instance; } }

	Waypoint[] waypoints;

	void Awake(){
		GetWaypoints ();
	}

	void GetWaypoints(){
		waypoints = gameObject.GetComponentsInChildren<Waypoint> ();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void TurnOffAllWaypoints(){
		for (int i = 0; i < waypoints.Length; i++) {
			waypoints[i].TurnOff();
		}
	}

	public void IlluminateShortestWaypointPath(Vector3 startPosition, Vector3 endPosition){
		TurnOffAllWaypoints ();

		List<Waypoint> waypointPath = GetShortestWaypointPath(startPosition, endPosition);

		//light up the waypoints in the path
		for(int i = 0; i < waypointPath.Count; i++){
			Vector3 closePosition = endPosition;
			if(i != waypointPath.Count - 1){ //if i is not the last waypoint in the path...
				//point to the next waypoint in the path
				closePosition = waypointPath[i + 1].transform.position;
			}
			waypointPath[i].GetComponent<Waypoint>().LightUp(closePosition);
		}
	}

	//https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
	List<Waypoint> GetShortestWaypointPath(Vector3 startPosition, Vector3 endPosition){

		List<Waypoint> waypointPath = new List<Waypoint> ();

		//1. Find starting node, ending node, set all other node distances to infinity (incl. end node)
		Waypoint startPoint = waypoints [0];
		startPoint.DijkstraDistance = 0;
		float minDistanceToStart = (startPosition - startPoint.transform.position).magnitude;

		Waypoint endPoint = waypoints [0]; //don't set endpoint distance to -inf until later (though it should get set in looking for the start node anyway...)
		float minDistanceToEnd = (endPosition - endPoint.transform.position).magnitude;

		for (int i = 1; i < waypoints.Length; i++) { //already used the 0 index to initialize, so start at index 1
			Waypoint currPoint = waypoints[i];
			float startToCurrPointDist = (startPosition - currPoint.transform.position).magnitude;
			float endToCurrPointDist = (endPosition - currPoint.transform.position).magnitude;

			//min distance check
			if(startToCurrPointDist < minDistanceToStart){
				startPoint.DijkstraDistance = Mathf.Infinity;	//set old start point distance to infinity
				currPoint.DijkstraDistance = 0;		//set new start point distance to 0
				startPoint = currPoint;				//set new start point
				minDistanceToStart = startToCurrPointDist;	//set new min distance
			}
			else{
				waypoints[i].DijkstraDistance = Mathf.Infinity;	//if it's not the shortest distance, set it's distance to infinity
			}

			//max distance check
			if(endToCurrPointDist < minDistanceToEnd){
				endPoint = currPoint;						//set new end point
				minDistanceToEnd = endToCurrPointDist;	//set new max distance
			}
		}

		if (endPoint != startPoint) {
			endPoint.DijkstraDistance = Mathf.Infinity;
		}

		//2. Set the initial node as current. Mark all other nodes unvisited. Create a set of all the unvisited nodes called the unvisited set.
		Waypoint currNode = startPoint;
		List<Waypoint> unvisitedPoints = new List<Waypoint> ();
		for (int i = 0; i < waypoints.Length; i++) {
			if(waypoints[i] != startPoint){
				unvisitedPoints.Add(waypoints[i]);
			}
		}


		while (unvisitedPoints.Count > 0) {

			//3. For the current node, consider all of its unvisited neighbors and calculate their tentative distances.
			//Compare the newly calculated tentative distance to the current assigned value and assign the smaller one.
			//For example, if the current node A is marked with a distance of 6, and the edge connecting it with a neighbor B has length 2,
			//...then the distance to B (through A) will be 6 + 2 = 8. If B was previously marked with a distance greater than 8 then change it to 8.
			//Otherwise, keep the current value.
			float neighborDistance = 0;
			for (int i = 0; i < currNode.neighbors.Length; i++) {
				Waypoint currNeighbor = currNode.neighbors [i];
				neighborDistance = (currNode.transform.position - currNeighbor.transform.position).magnitude;
				if (currNeighbor.DijkstraDistance > neighborDistance + currNode.DijkstraDistance) {
					currNeighbor.DijkstraDistance = neighborDistance + currNode.DijkstraDistance;
				}
			}

			//4. When we are done considering all of the neighbors of the current node, mark the current node as visited and remove it
				//from the unvisited set. A visited node will never be checked again.
			unvisitedPoints.Remove (currNode);

			//5. If the destination node has been marked visited (when planning a route between two specific nodes)
			//or if the smallest tentative distance among the nodes in the unvisited set is infinity (when planning a complete traversal;
			//occurs when there is no connection between the initial node and remaining unvisited nodes), then stop. The algorithm has finished.
			if(currNode == endPoint){
				break;
			}

			//6. Otherwise, select the unvisited node that is marked with the smallest tentative distance, set it as the new "current node", and go back to step 3.
			float smallestDist = Mathf.Infinity;
			int smallestDistIndex = -1;
			for(int i = 0; i < unvisitedPoints.Count; i++){
				if(unvisitedPoints[i].DijkstraDistance < smallestDist){
					smallestDist = unvisitedPoints[i].DijkstraDistance;
					smallestDistIndex = i;
				}
			}

			currNode = unvisitedPoints[smallestDistIndex];
		}

		//7. Get the path!
		//currNode  at the start of this loop currently = endPoint
		while(currNode != startPoint){
			waypointPath.Add(currNode);

			//go through the neighbors, pick the one with the smallest distance
			float smallestDist = currNode.DijkstraDistance;	//distance should definitely be smaller than the current node
			int smallestDistIndex = -1;
			for(int i = 0; i < currNode.neighbors.Length; i++){
				if(currNode.neighbors[i].DijkstraDistance < smallestDist){
					smallestDist = currNode.neighbors[i].DijkstraDistance;
					smallestDistIndex = i;
				}
			}

			currNode = currNode.neighbors[smallestDistIndex];
		}

		//since the current order is from the end point to the start point, reverse the list
		waypointPath.Reverse ();



		return waypointPath;
	}
}
