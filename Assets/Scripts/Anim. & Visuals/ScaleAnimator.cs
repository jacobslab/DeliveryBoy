using UnityEngine;
using System.Collections;

public class ScaleAnimator : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public IEnumerator AnimateScaleUp(float time, float endScaleMult, float startScaleMult){
		if (Config_CoinTask.isJuice) {
			Vector3 fullScale = transform.localScale * endScaleMult;
			Vector3 smallScale = fullScale * startScaleMult;

			Vector3 origScale = transform.localScale;
			
			float scaleMultDifference = endScaleMult - startScaleMult;
			
			
			transform.localScale = smallScale;
			
			float currentTime = 0.0f;
			
			while (currentTime < time) {
				currentTime += Time.deltaTime;
				if(currentTime > time){
					currentTime = time;
				}
				float scaleMult = startScaleMult + (scaleMultDifference * (currentTime / time));
				transform.localScale = origScale * scaleMult;
				yield return 0;
			}

			transform.localScale = transform.localScale;
		
		} else {
			yield return 0;
		}
	}
}
