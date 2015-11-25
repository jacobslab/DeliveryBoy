using UnityEngine;
using System.Collections;

public class TextMeshColorCycler : MonoBehaviour {

	public Color[] colors;

	TextMesh myTextMesh;

	// Use this for initialization
	void Start () {
		myTextMesh = GetComponent<TextMesh> ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	public IEnumerator CycleColors(){
		if (myTextMesh && colors.Length > 1) {

			float currentTime = 0.0f;
			myTextMesh.color = colors [0];

			while (true) {
				for (int i = 0; i < colors.Length; i++) {
					while (currentTime < 1.0f) {
						currentTime += Time.deltaTime;
						if (i < colors.Length - 1) {
							myTextMesh.color = Color.Lerp (colors [i], colors [i + 1], currentTime);
						} else {
							myTextMesh.color = Color.Lerp (colors [i], colors [0], currentTime);
						}
						yield return 0;
					}
					currentTime = 0.0f;
				}
				yield return 0;
			}

		}
		yield return 0;
	}
}
