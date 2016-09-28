using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using iView;

public class GazeFollower2D : MonoBehaviour {

	public Canvas myCanvas;
	Transform gazeFollower;

	void Awake(){
		gazeFollower = GetComponent<Transform> ();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (gazeFollower != null) {
			Vector2 screenGazePos = SMIGazeController.Instance.GetSample ().averagedEye.gazePosInUnityScreenCoords ();
			Debug.Log("SCREEN POS: " + screenGazePos);
			Vector3 worldGazePos = Camera.main.ScreenToWorldPoint(new Vector3 ( screenGazePos.x, screenGazePos.y, gazeFollower.position.z));
			Debug.Log("WORLD POS: " + worldGazePos);
			//gazeFollower.position = new Vector3(screenGazePos.x, screenGazePos.y, gazeFollower.position.z);
			//gazeFollower.position = worldGazePos;


			Vector2 pos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, screenGazePos, myCanvas.worldCamera, out pos);
			gazeFollower.position = myCanvas.transform.TransformPoint(pos);

		}
	}
}
