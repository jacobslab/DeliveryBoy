using UnityEngine;
using System.Collections;

public class ScreenAnchor : MonoBehaviour {
	public Camera anchorCamera;
	public Vector2 screenPercentOffset; //values should be from 0 - 100
	public float zAxisOffset;

	public enum AnchorPoint{
		topLeft,
		topRight,
		center,
		bottomLeft,
		bottomRight
	}

	public AnchorPoint anchor = AnchorPoint.topLeft;

	// Use this for initialization
	void Start () {
		SetScreenPosition ();
	}

	void SetScreenPosition(){
		Vector3 desiredScreenPos = Vector3.zero;
		Vector3 pixelOffset = Vector3.zero;
		pixelOffset.x = anchorCamera.pixelWidth * (screenPercentOffset.x / 100.0f);
		pixelOffset.y = anchorCamera.pixelHeight * (screenPercentOffset.y / 100.0f);

		switch (anchor) {
			case AnchorPoint.topLeft:
			pixelOffset.y *= -1;
			desiredScreenPos = new Vector3(0.0f, anchorCamera.pixelHeight, 0.0f) + (Vector3)pixelOffset;
			break;
			case AnchorPoint.topRight:
			pixelOffset.x *= -1;
			pixelOffset.y *= -1;
			desiredScreenPos = new Vector3(anchorCamera.pixelWidth, anchorCamera.pixelHeight, 0.0f) + (Vector3)pixelOffset;
			break;
			case AnchorPoint.center:
			desiredScreenPos = new Vector3(anchorCamera.pixelWidth/2, anchorCamera.pixelHeight/2, 0.0f) + (Vector3)pixelOffset;
			break;
			case AnchorPoint.bottomLeft:
			desiredScreenPos = new Vector3(0.0f, 0.0f, 0.0f) + (Vector3)pixelOffset;
			break;
			case AnchorPoint.bottomRight:
			pixelOffset.x *= -1;
			desiredScreenPos = new Vector3(anchorCamera.pixelWidth, 0.0f, 0.0f) + (Vector3)pixelOffset;
			break;
		}

		desiredScreenPos.z = anchorCamera.nearClipPlane;

		Vector3 worldPoint = anchorCamera.ScreenToWorldPoint (desiredScreenPos);
		transform.position = worldPoint + Vector3.forward*zAxisOffset;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
