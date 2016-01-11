using UnityEngine;
using System.Collections;

public class VisibilityToggler : MonoBehaviour {

	public bool isVisibleAtStart = true;

	bool isVisible = true;
	float currentAlpha = 1.0f;

	// Use this for initialization
	void Start () {
		if (!isVisibleAtStart) {
			TurnVisible(false);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool GetVisibility(){
		return isVisible;
	}

	//function to turn off (or on) the object without setting it inactive -- because we want to keep logging on
	public void TurnVisible(bool shouldBeVisible){ 

		if(GetComponent<Renderer>() != null){
			GetComponent<Renderer>().enabled = shouldBeVisible;
		}
		
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		for(int i = 0; i < renderers.Length; i++){
			renderers[i].enabled = shouldBeVisible;
		}
		
		
		//turn off all colliders of an object
		if(GetComponent<Collider>() != null){
			GetComponent<Collider>().enabled = shouldBeVisible;
		}
		Collider[] colliders = GetComponentsInChildren<Collider>();
		for(int i = 0; i < colliders.Length; i++){
			colliders[i].enabled = shouldBeVisible;
		}
		
		
		isVisible = shouldBeVisible;
		
	}

	public void SetAlpha(float newAlpha){
		currentAlpha = newAlpha;

		Renderer myRenderer = GetComponent<Renderer> ();
		if(myRenderer != null){
			Color materialColor = myRenderer.material.color;
			myRenderer.material.color = new Color(materialColor.r, materialColor.g, materialColor.b, newAlpha);
		}
		
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		for(int i = 0; i < renderers.Length; i++){
			Color materialColor = renderers[i].material.color;
			renderers[i].material.color = new Color(materialColor.r, materialColor.g, materialColor.b, newAlpha);
		}
	}

	public float GetAlpha(){
		return currentAlpha;
	}
}
