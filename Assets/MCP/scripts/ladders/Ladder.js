var ladderBottom : GameObject;
var ladderTop : GameObject;

private var climbDirection : Vector3 = Vector3.zero;

function Start () {
	climbDirection = ladderTop.transform.position -  ladderBottom.transform.position;
}

function ClimbDirection () {
	return climbDirection;
}