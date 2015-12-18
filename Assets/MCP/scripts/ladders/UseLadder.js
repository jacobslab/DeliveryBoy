// The speed of the player up and down the ladder. Roughly equal to walking speed is a good starting point.
var climbSpeed = 6.0;
// In the range -1 to 1 where -1 == -90 deg, 1 = 90 deg, angle of view camera at which the user climbs down rather than up when moving with the forward key.
var climbDownThreshold = -0.4;

private var climbDirection : Vector3 = Vector3.zero;
private var lateralMove : Vector3 = Vector3.zero;
private var forwardMove : Vector3 = Vector3.zero;
private var ladderMovement : Vector3 = Vector3.zero;
private var currentLadder : Ladder = null;
private var latchedToLadder : boolean = false;
// DEBUG :: private var inMotionControl : boolean = false;
private var inLandingPad : boolean = false;
private var mainCamera : GameObject = null;
private var controller : CharacterController = null;
private var landingPads : ArrayList = null;
// DEBUG :: var landPadCount : int = 0;

function Start () {
	mainCamera = GameObject.FindWithTag("MainCamera");
	controller = GetComponent(CharacterController);
	landingPads = new ArrayList();
}

function OnTriggerEnter (other : Collider) {
	if(other.tag == "LadderLandingPad") {
		// add this landing pad to the list of landing pads if it's not already there
		if(!landingPads.Contains(other)){
			landingPads.Add(other);
		}
		
		// set the flag
		inLandingPad = true;
		// DEBUG :: landPadCount = landingPads.Count;
	}
	
	if(other.tag == "Ladder") {		
		LatchLadder(other.gameObject, other);
	}
}

function OnTriggerExit (other : Collider) {		
	if(other.tag == "LadderLandingPad") {
		// remove this landing pad from the list
		landingPads.Remove(other);
		
		// if the list is now empty, set the flag
		if(landingPads.Count == 0) inLandingPad = false;
		// DEBUG :: landPadCount = landingPads.Count;
	}

	if(other.tag == "Ladder") {
		UnlatchLadder();
	}
}

/**
 *	Connect the player to the ladder, and shunt FixedUpdate calls to the special ladder movment functions.
 */
function LatchLadder (latchedLadder : GameObject, collisionWaypoint : Collider) {
	// typecast the latchedLadder as a Ladder object from GameObject
	currentLadder = latchedLadder.GetComponent(Ladder);
	latchedToLadder = true;
	
	// get the climb direction
	climbDirection = currentLadder.ClimbDirection();
	
	// let the other scripts know we are on the ladder now
	gameObject.SendMessage("OnLadder", null, SendMessageOptions.RequireReceiver);
}

/**
 *	Shut off special ladder movement controls and return to normal movement operations.
 */
function UnlatchLadder () {
	latchedToLadder = false;
	currentLadder = null;
	
	// let the other scripts know we are off the ladder now
	gameObject.SendMessage("OffLadder", ladderMovement, SendMessageOptions.RequireReceiver);
	
	// DEBUG :: inMotionControl = false;
}

/**
 *	Convert the player's normal forward and backward motion into up and down motion on the ladder.
 */
function LadderFixedUpdate () {
	
	// If we jumpped, then revert back to the original behavior
	if (Input.GetButton("Jump")) {
		UnlatchLadder();
		gameObject.SendMessage("FixedUpdate", null, SendMessageOptions.RequireReceiver);
		return;
	}
	
	// DEBUG :: inMotionControl = true;
	
	// find the climbing direction
	verticalMove = climbDirection.normalized;
	
	// convert forward motion to vertical motion
	verticalMove *= Input.GetAxis("Vertical");
	verticalMove *= (mainCamera.transform.forward.y > climbDownThreshold) ? 1 : -1;
	
	// find lateral component of motion
	if (inLandingPad) {
		lateralMove = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
	} else {
		lateralMove = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
	}
	lateralMove = transform.TransformDirection(lateralMove);
		
	// move
	ladderMovement = verticalMove + lateralMove;
	var flags = controller.Move(ladderMovement * climbSpeed * Time.deltaTime);
}

@script RequireComponent(CharacterController)