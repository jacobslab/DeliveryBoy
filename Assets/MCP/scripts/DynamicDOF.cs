using UnityEngine;
using System.Collections;

public class DynamicDOF : MonoBehaviour {
	public Transform origin;
	public GameObject target;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {


		Ray ray = new Ray(origin.transform.position, origin.transform.forward);
		RaycastHit hit= new RaycastHit();
		if(Physics.Raycast(ray, out hit, Mathf.Infinity))
		{
			Debug.DrawRay(origin.transform.position, origin.transform.forward, Color.cyan);
			target.transform.position = hit.point;
		}
		else
		{
			Debug.DrawRay(origin.transform.position, origin.transform.forward, Color.cyan);
		}
	}
}
