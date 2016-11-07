using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaypointController : MonoBehaviour {
	Experiment exp { get { return Experiment.Instance; } }

	Waypoint[] waypoints;

	bool areWaypointsEnabled = false;
	Vector3 pathStartPos;
	Vector3 pathEndPos;

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
		if (areWaypointsEnabled) {
			IlluminateShortestWaypointPath();
		}
	}

	public void EnableWaypoints(Vector3 startPosition, Vector3 endPosition){
		areWaypointsEnabled = true;
		pathStartPos = startPosition;
		pathEndPos = endPosition;

		exp.eventLogger.LogWayPoints (true);
	}

	public void DisableWaypoints(){
		areWaypointsEnabled = false;
		TurnOffAllWaypoints();

		exp.eventLogger.LogWayPoints (false);
	}

	void TurnOffAllWaypoints(){
		for (int i = 0; i < waypoints.Length; i++) {
			waypoints[i].TurnOff();
		}
	}

	void IlluminateShortestWaypointPath(){
		TurnOffAllWaypoints ();

		List<Waypoint> waypointPath = GetShortestWaypointPath(pathStartPos, pathEndPos);

		//light up the waypoints in the path
		for(int i = 0; i < waypointPath.Count; i++){
			Vector3 closePosition = pathEndPos;
			if(i != waypointPath.Count - 1){ //if i is not the last waypoint in the path...
				//point to the next waypoint in the path
				closePosition = waypointPath[i + 1].transform.position;
			}
			waypointPath[i].GetComponent<Waypoint>().LightUp(closePosition);
		}
	}

	public Waypoint GetClosestWaypoint(Vector3 position){
		Waypoint closestPoint = waypoints [0];
		float minDistanceToStart = (position - closestPoint.transform.position).magnitude;

		for (int i = 1; i < waypoints.Length; i++) { //already used the 0 index to initialize, so start at index 1
			Waypoint currPoint = waypoints[i];
			float startToCurrPointDist = (position - currPoint.transform.position).magnitude;
			
			//min distance check
			if(startToCurrPointDist < minDistanceToStart){
				closestPoint = currPoint;				//set new start point
				minDistanceToStart = startToCurrPointDist;	//set new min distance
			}
		}

		return closestPoint;
	}

	//can't just use end.dijkstra distance, because this could be an old path with recalculated distances.
	//thus, must recalculate the path length for accurate path length.
	public float GetPathLength(List<Waypoint> path){
		float length = 0.0f;

		if (path.Count > 0) {
			Waypoint startWP = path [0];
			Waypoint nextWP = path[0];
			for (int i = 1; i < path.Count; i++) {
				nextWP = path[i];
				length += UsefulFunctions.GetDistance(startWP.transform.position, nextWP.transform.position);
			}
		}
		return length;
	}


	//https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
	public List<Waypoint> GetShortestWaypointPath(Vector3 startPosition, Vector3 endPosition){
		//startPosition = new Vector3 (startPosition.x, 0.0f, startPosition.z);
		//endPosition = new Vector3 (endPosition.x, 0.0f, endPosition.z);

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

			if(smallestDistIndex > currNode.neighbors.Length || smallestDistIndex == -1){
				Debug.Log("BAD INDEX: " + smallestDistIndex + " FOR: " + currNode.name);
			}

			currNode = currNode.neighbors[smallestDistIndex];
		}

		//decide whether or not to add the start point:
			//if the start point is between the player(start position) and the next point, add it.
		//Vector3 nextPointPosition = endPosition;
		//if (waypointPath.Count > 0) {
		//	nextPointPosition = waypointPath[waypointPath.Count - 1].transform.position;
		//}
		//if(!CheckBetweenPoints(startPosition, startPoint.transform.position, nextPointPosition)){
			waypointPath.Add(startPoint);
		//}


		//since the current order is from the end point to the start point, reverse the list
		waypointPath.Reverse ();



		return waypointPath;
	}

	//THIS FUNCTION IS MERELY CHECKING IF THE BETWEEN POINT IS BETWEEN THE COORDINATES OF THE OTHER TWO POINTS.
	//WE ARE *NOT* CHECKING IF THE BETWEEN POINT LIES PERFECTLY ON THE LINE A-B.
	bool CheckBetweenPoints(Vector3 betweenPoint, Vector3 endPtA, Vector3 endPtB){
		//CASE 1: if points are the same, return true
		if (endPtA.x == endPtB.x && endPtA.z == endPtB.z) {
			return true;
		}

		//CASES 2&3: both endpoints share an x coord
		if (endPtA.x == endPtB.x) {
			if (endPtA.z < endPtB.z) {
				if (betweenPoint.z < endPtB.z && betweenPoint.z > endPtA.z) {
					return true;
				}
			} else if (endPtA.z > endPtB.z) {
				if (betweenPoint.z > endPtB.z && betweenPoint.z < endPtA.z) {
					return true;
				}
			} else {
				return false;
			}
		}
		//CASES 2&3: both endpoints share z coord
		else if (endPtA.z == endPtB.z) {
			if (endPtA.x < endPtB.x) {
				if (betweenPoint.x < endPtB.x && betweenPoint.x > endPtA.x) {
					return true;
				}
			} else if (endPtA.x > endPtB.x) {
				if (betweenPoint.x > endPtB.x && betweenPoint.x < endPtA.x) {
					return true;
				}
			} else {
				return false;
			}
		}

		//CASES 3&4: endpoints to not share either coordinate, A.x < B.x
		else if (endPtA.x < endPtB.x) {
			if (betweenPoint.x > endPtA.x && betweenPoint.x < endPtB.x) {
				//A.z < B.z
				if (endPtA.z < endPtB.z) {
					if (betweenPoint.z > endPtA.z && betweenPoint.z < endPtB.z) {
						return true;
					}
				}
				//A.z > B.z
				else if (endPtA.z > endPtB.z) {
					if (betweenPoint.z < endPtA.z && betweenPoint.z > endPtB.z) {
						return true;
					}
				}
			} else {
				return false;
			}
		}

		//CASES 5&6: endpoints to not share either coordinate, A.x > B.x
		else if (endPtA.x > endPtB.x) {
			if (betweenPoint.x < endPtA.x && betweenPoint.x > endPtB.x) {
				//A.z < B.z
				if (endPtA.z < endPtB.z) {
					if (betweenPoint.z > endPtA.z && betweenPoint.z < endPtB.z) {
						return true;
					}
				}
				//A.z > B.z
				else if (endPtA.z > endPtB.z) {
					if (betweenPoint.z < endPtA.z && betweenPoint.z > endPtB.z) {
						return true;
					}
				}
			} else {
				return false;
			}
		}

		return false;

	}
	

}
