var speed = 6.0;
var jumpSpeed = 8.0;
var gravity = 20.0;

private var moveDirection = Vector3.zero;
private var grounded : boolean = false;

// *** Added for UseLadder ***
private var mainCamera : GameObject = null;
private var controller : CharacterController = null;
private var onLadder = false;
var ladderHopSpeed = 6.0;

function Start () {
	mainCamera = GameObject.FindWithTag("MainCamera");
	controller = GetComponent(CharacterController);
}

function FixedUpdate() {
	
	// *** Added for UseLadder ***
	// If we are on the ladder, let the ladder code take over and handle FixedUpdate calls.
	if(onLadder) {
		gameObject.SendMessage("LadderFixedUpdate", null, SendMessageOptions.RequireReceiver);
		return;
	}
	// ***
	
	if (grounded) {
		// We are grounded, so recalculate movedirection directly from axes
		moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		moveDirection = transform.TransformDirection(moveDirection);
		moveDirection *= speed;
		
		if (Input.GetButton ("Jump")) {
			moveDirection.y = jumpSpeed;
		}
	}

	// Apply gravity
	moveDirection.y -= gravity * Time.deltaTime;
	
	// Move the controller
	var flags = controller.Move(moveDirection * Time.deltaTime);
	grounded = (flags & CollisionFlags.CollidedBelow) != 0;
}

// *** Added for UseLadder ***
function OnLadder () {
	onLadder = true;
	moveDirection = Vector3.zero;
}

function OffLadder (ladderMovement) {
	onLadder = false;
	
	// perform off-ladder hop
	var hop : Vector3 = mainCamera.transform.forward;
	hop = transform.TransformDirection(hop);
	moveDirection = (ladderMovement.normalized + hop.normalized) * ladderHopSpeed;
}
// ***

@script RequireComponent(CharacterController)