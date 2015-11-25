using UnityEngine;
using System.Collections;

public class ColorChanger : MonoBehaviour {
	public bool isTextMesh;

	public Renderer[] renderers;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public IEnumerator LerpChangeColor(Color newColor, float time){
		float currentTime = 0.0f;

		float timeMult = 1.0f / time;

		Color origColor = new Color ();

		if (isTextMesh) {
			origColor = GetComponent<TextMesh>().color;
		}
		else if (renderers.Length > 0) {
			origColor = renderers [0].material.color;
		}

		while(currentTime < 1.0f){
			currentTime += Time.deltaTime*timeMult;
			for (int i = 0; i < renderers.Length; i++) {
				renderers[i].material.color = Color.Lerp(origColor, newColor, currentTime);
			}
			if(isTextMesh){
				GetComponent<TextMesh>().color = Color.Lerp(origColor, newColor, currentTime);
			}
			yield return 0;
		}
	}

	//instant color change
	public void ChangeColor(Color newColor){
		for (int i = 0; i < renderers.Length; i++) {
			renderers[i].material.color = newColor;
		}
		if(isTextMesh){
			GetComponent<TextMesh>().color = newColor;
		}
	}
}
