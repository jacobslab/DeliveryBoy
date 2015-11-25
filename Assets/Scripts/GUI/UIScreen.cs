using UnityEngine;
using System.Collections;

public class UIScreen : MonoBehaviour {

	Experiment exp { get { return Experiment.Instance; } } 
	
	// Use this for initialization
	void Start () {
		Enable (false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void Play(){
		Enable (true);
	}
	
	public void Stop(){
		Enable (false);
	}
	
	void Enable(bool shouldEnable){
		//GetComponent<EnableChildrenLogTrack>().LogChildrenEnabled(shouldEnable);
		//TODO: FIX WEIRD AABB ERROR WHEN PAUSE UI CALLS THIS.
		UsefulFunctions.EnableChildren( transform, shouldEnable );
	}
}
